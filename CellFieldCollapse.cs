using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class CellFieldCollapse<T, A> : IDisposable
        where T : Module<T>
    {
        // --- CSP field --- //
        public bool AllCellsCollapsed => _entropyHeap == null || _entropyHeap.Count == 0;
        protected Heap<CellSuperPosition<A>> _entropyHeap;
        protected abstract ICSPfield<A> _cspField { get; }
        private System.Random _random;


        // --- Modules --- //

        public ModuleSet<T> ModuleSet { get; private set; }
        protected CellConstraintSet[] _cellConstraintSets;
        protected CellConstraintSet _combinedConstraints;

        public CellFieldCollapse(int size, int seed, ModuleSet<T> moduleSet)
        {
            ModuleSet = moduleSet;

            // CellConstraint.SetCellConstraintLength(moduleSet.Modules.Length);
            // CellConstraintSet.SetCellConstraintSetLength(6); // TMP

            _random = new System.Random(seed);
            _cellConstraintSets = moduleSet.CellConstraintSets;
        }

        public CellSuperPosition<A> GetCSP(A address) => _cspField.GetCSP(address);


        // --- Collapse Control --- //

        public abstract void CollapseInitialCell();

        public void Collapse(CellSuperPosition<A> csp, int superPosIndex, int orientationIndex)
        {
            csp.Collapse(superPosIndex, orientationIndex);
            ConstraintAdjacentCells(csp.Address);
        }

        public void CollapseNext()
        {
            if(AllCellsCollapsed)
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

            while (!AllCellsCollapsed)
                CollapseNext();
        }

        public void CollapseAll()
        {
            while (!AllCellsCollapsed)
                CollapseNext();
        }

        public void CollapseAt(A address, int moduleIndex)
        {
            if(_cspField.GetCSP(address).Collapsed)
                return;

            _cspField.GetCSP(address).CollapseToModule(moduleIndex, _random);
            ConstraintAdjacentCells(address);
        }


        // --- Propagate Constraints --- //     

        public void ConstraintAdjacentCells(A address)
        {
            bool[] adjacentEntropyChange;
            IAdjacentCell<CellSuperPosition<A>> adjacentCSP;

            // The constraints of adjacent cells recursively
            CellSuperPosition<A> csp = _cspField.GetCSP(address);
            adjacentCSP = _cspField.GetAdjacentCSP(address);
            adjacentEntropyChange = new bool[adjacentCSP.Length];
            _combinedConstraints = csp.CombinedConstraints(_cellConstraintSets);
            
            // Constraint adjacent cells and check for changes in entropy
            for (byte side = 0; side < adjacentCSP.Length; side ++)
            {
                if (adjacentCSP.IsValid(side))
                    adjacentCSP.GetCell(side).AddConstraint(_combinedConstraints.GetCellConstraint(side), out adjacentEntropyChange[side]);
                else
                    adjacentEntropyChange[side] = false;
            }

            _combinedConstraints.Dispose();

            // Constraint adjacent cells of all adjacent cells, had a change in entropy
            for (byte side = 0; side < adjacentEntropyChange.Length; side ++)
            {
                if(adjacentEntropyChange[side])
                {
                    addToEntropyHeap(adjacentCSP.GetCell(side));
                    ConstraintAdjacentCells(adjacentCSP.GetCell(side).Address);
                }
            }
        }

        private void addToEntropyHeap(CellSuperPosition<A> csp)
        {
            if(_entropyHeap == null)
                _entropyHeap = new Heap<CellSuperPosition<A>>(_cspField.Count);

            if(csp.Collapsed)
                Debug.LogError("Can't register collapsed cell as SuperPosition");
            else if(csp.HeapIndex == -1)
                Debug.LogWarning("CellSuperPosition has heap index of -1");

            if(_entropyHeap.Contains(csp))
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

    public interface ICSPfield<A> : IDisposable
    {
        int Count { get; }
        CellSuperPosition<A> GetCSP(A a);
        IAdjacentCell<CellSuperPosition<A>> GetAdjacentCSP(A a);
    }
}