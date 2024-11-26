
using System;
using System.Data;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class CellFieldCollapse<T, A> : IDisposable
        where T : Module<T>
    {
        // --- CSP field --- //
        private ICSPfield<T, A> _cspField;
        private Heap<CellSuperPosition<T, A>> _entropyHeap;
        public bool AllCellsCollapsed => _entropyHeap.Count == 0;
        private System.Random _random;        


        // --- Modules --- //

        public ModuleSet<T> ModuleSet => _moduleSet;
        protected ModuleSet<T> _moduleSet;
        protected CellConstraintSet[] _cellConstraintSets;
        private CellConstraintSet _combinedConstraints;

        public CellFieldCollapse(int size, int seed, ModuleSet<T> moduleSet)
        {
            _moduleSet = moduleSet;
            _cspField = createCSPfield(size, this);
            _entropyHeap = new Heap<CellSuperPosition<T, A>>(_cspField.Count);
            _random = new System.Random(seed);
            _cellConstraintSets = moduleSet.CellConstraintSets;
        }
        
        protected abstract ICSPfield<T, A> createCSPfield(int size, CellFieldCollapse<T, A> cfc);
        public CellSuperPosition<T, A> GetCSP(A address) => _cspField.GetCSP(address);
        public void AlterSeed(int seed) => _random = new System.Random(seed);


        // --- Collapse Control --- //

        public abstract void CollapseInitialCell();

        public void Collapse(CellSuperPosition<T, A> csp, int superPosIndex, int orientationIndex)
        {
            csp.Collapse(superPosIndex, orientationIndex);
            ConstraintAdjacentCells(csp.Address);
        }

        public void CollapseNext()
        {
            if(AllCellsCollapsed)
                return;

            CellSuperPosition<T, A> csp = _entropyHeap.RemoveFirst();

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
            _cspField.GetCSP(address).CollapseToModule(moduleIndex, _random);
            ConstraintAdjacentCells(address);
        }


        // --- Propagate Constraints --- //     

        public void ConstraintAdjacentCells(A address)
        {
            bool[] adjacentEntropyChange;
            IAdjacentCell<CellSuperPosition<T, A>> adjacentCSP;

            // The constraints of adjacent cells recursively
            CellSuperPosition<T, A> csp = _cspField.GetCSP(address);
            adjacentCSP = _cspField.GetAdjacentCSP(address);
            adjacentEntropyChange = new bool[adjacentCSP.Length];
            _combinedConstraints = csp.CombinedConstraints(_cellConstraintSets);
            
            // Constraint adjacent cells and check for changes in entropy
            for (byte side = 0; side < adjacentCSP.Length; side ++)
            {                
                if (adjacentCSP.IsValid(side))
                    adjacentCSP.GetCell(side).AddConstraint(_combinedConstraints[side], out adjacentEntropyChange[side]);
                else
                    adjacentEntropyChange[side] = false;
            }

            // Constraint adjacent cells of all adjacent cells, had a change in entropy
            for (byte side = 0; side < adjacentEntropyChange.Length; side ++)
            {
                if(adjacentEntropyChange[side])
                {
                    add2EntropyHeap(adjacentCSP.GetCell(side));
                    ConstraintAdjacentCells(adjacentCSP.GetCell(side).Address);
                }
            }
        }

        private void add2EntropyHeap(CellSuperPosition<T, A> csp)
        {
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
            // DISPOSE OF ALL UNMANAGED MEMORY IN _cspField AND _cellConstraintSets
            _cspField = null;
            _entropyHeap = null;
            _cellConstraintSets = null;
        }
    }

    public interface ICSPfield<T, A>
        where T : Module<T>
    {
        int Count { get; }
        CellSuperPosition<T, A> GetCSP(A a);
        IAdjacentCell<CellSuperPosition<T, A>> GetAdjacentCSP(A a);
    }
}