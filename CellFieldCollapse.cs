/// <summary>
/// The CellFieldCollapse class manages the collapse process of a CSPField.
/// It serves as the primary driver of the Wave Function Collapse algorithm,
/// and also acts as the basis for a debugging tool within the WFC setup.
/// </summary>

using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class CellFieldCollapse<T, A> : IDisposable
        where T : Module<T>
    {
        // --- CSP field --- //
        public bool AllCellsCollapsed => _entropyHeap == null || _entropyHeap.Count == 0;
        protected abstract ICSPfield<A> _cspField { get; }
        protected abstract void createCSPfield(int size);
        protected Heap<CellSuperPosition<A>> _entropyHeap;
        protected System.Random _random;


        // --- Modules --- //

        public ModuleSet<T> ModuleSet { get; private set; }
        private CellConstraintSet[] _cellConstraintSets;

        public CellFieldCollapse(int size, int seed, ModuleSet<T> moduleSet)
        {
            ModuleSet = moduleSet;

            createCSPfield(size);
            _entropyHeap = new Heap<CellSuperPosition<A>>(_cspField.Count);
            _random = new System.Random(seed);
            _cellConstraintSets = moduleSet.CellConstraintSets;
        }


        // --- Collapse Control --- //

        public abstract void CollapseInitialCell();

        public void Collapse(CellSuperPosition<A> csp, int superPosIndex, int orientationIndex)
        {
            csp.Collapse(superPosIndex, orientationIndex);
            ConstraintAdjacentCells(csp.Address);
        }

        public void CollapseNext()
        {
            if (AllCellsCollapsed)
                return;

            CellSuperPosition<A> csp = _entropyHeap.RemoveFirst();

            if (csp.Collapsed)
                CollapseNext();
            else
            {
                csp.CollapseRandom(_random);
                ConstraintAdjacentCells(csp.Address);
            }
        }

        public void InstantCollapseAll()
        {
            CollapseInitialCell();
            CollapseRest();
        }

        public void CollapseRest()
        {
            while (!AllCellsCollapsed)
                CollapseNext();
        }

        public void CollapseAt(A address, int moduleIndex)
        {
            if (_cspField.GetCSP(address).Collapsed)
                return;

            _cspField.GetCSP(address).CollapseToModule(moduleIndex, _random);
            ConstraintAdjacentCells(address);
        }


        // --- Propagate Constraints --- //

        public void ConstraintAdjacentCells(A address)
        {
            CellSuperPosition<A> csp = _cspField.GetCSP(address);
            CellConstraintSet constraintSet = csp.GetConstraintSet(_cellConstraintSets);
            IAdjacentCell<A> adjacentCSP = _cspField.GetAdjacentCSP(address);

            // Constraint adjacent cells and check for changes in entropy

            for (byte side = 0; side < adjacentCSP.Length; side++)
            {
                bool entropyChange = false;

                if (adjacentCSP.IsCollapsed(side))
                {
                    adjacentCSP.GetCell(side).AddConstraint(constraintSet.GetCellConstraint(side), out entropyChange);

                    if (entropyChange)
                    {
                        // Constraints adjacent cells recursively

                        addToEntropyHeap(adjacentCSP.GetCell(side));
                        ConstraintAdjacentCells(adjacentCSP.GetCell(side).Address);
                    }
                }
            }
        }

        private void addToEntropyHeap(CellSuperPosition<A> csp)
        {
            if (csp.Collapsed)
                Debug.LogError("Can't register collapsed cell as SuperPosition");
            else if (csp.HeapIndex == -1)
                Debug.LogWarning("CellSuperPosition has heap index of -1");

            if (_entropyHeap.Contains(csp))
                _entropyHeap.UpdateItem(csp);
            else
                _entropyHeap.Add(csp);
        }


        // --- Memory --- //

        public virtual void Dispose()
        {
            _cspField.Dispose();
            _entropyHeap = null;
            _cellConstraintSets = null;
        }
    }

    /// <summary>
    /// The ICSPfield interface defines the contract for a CSPField that can be collapsed by a CellFieldCollapse.
    /// It provides access to all CellSuperPositions in the field and their adjacency relationships.
    /// The IAdjacentCell represents the neighboring context of a single CellSuperPosition in a CSPField.
    /// It allows the CellFieldCollapse to remain tessellation-agnostic by exposing adjacency uniformly.
    /// </summary>

    public interface ICSPfield<A> : IDisposable
    {
        int Count { get; }
        CellSuperPosition<A> GetCSP(A a);
        IAdjacentCell<A> GetAdjacentCSP(A a);
    }
    
    public interface IAdjacentCell<A>
    {
        int Length { get; }
        bool IsCollapsed(int i);
        CellSuperPosition<A> GetCell(int i);
    }
}