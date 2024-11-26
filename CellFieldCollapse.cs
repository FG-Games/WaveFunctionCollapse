
using System;
using System.Data;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class CellFieldCollapse<T, A> : IDisposable
        where T : Module<T>
    {
        // --- CSP field --- //
        public ICSPfield<T, A> _cspField; // TMP PUBLIC TO TEST IN CSP
        private Heap<CellSuperPosition<T, A>> _entropyHeap;
        private System.Random _random;


        // --- Events --- //

        public bool AllCellsCollapsed { get => _entropyHeap.Count == 0; }


        // --- Modules --- //

        public ModuleSet<T> ModuleSet { get => _moduleSet; }
        private ModuleSet<T> _moduleSet;
        private CellConstraintSet[] _cellConstraintSets;

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

        public void Add2EntropyHeap(CellSuperPosition<T, A> csp)
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


        // --- Collapse Control --- //

        public abstract void CollapseInitialCell();

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
                csp.ConstraintAdjacentCells(_cellConstraintSets);
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
            _cspField.GetCSP(address).ConstraintAdjacentCells(_cellConstraintSets);
        }


        // --- Constraints --- //     

        public CellConstraintSet CombinedConstraints (CellSuperPosition<T, A> csp, CellConstraintSet[] constraintSets)
        {
            if(csp.Collapsed)
            {
                CollapsedPosition collapsedPosition = csp.GetCollapsedPosition;
                CellConstraintSet cellConstraintSet = constraintSets[collapsedPosition.ModuleIndex];
                return cellConstraintSet * collapsedPosition.Orientation;
            }
            else
            {
                CellConstraintSet combinedConstraints = SuperConstraints(csp.SuperPositions[0]);

                for (int i = 1; i < csp.SuperPositions.Count; i++)
                    combinedConstraints += SuperConstraints(csp.SuperPositions[i]);

                return combinedConstraints;
            }

            CellConstraintSet SuperConstraints(SuperPosition superPosition)
            {
                // Combine module constraints of all possible orientations
                CellConstraintSet superConstraints = rotatedContraints_S(superPosition, 0);

                for (int i = 1; i < superPosition.Orientations.Count; i ++)
                    superConstraints += rotatedContraints_S(superPosition, i);

                return superConstraints;
            }

            CellConstraintSet rotatedContraints_S(SuperPosition superPosition, int i)
            {
                CellConstraintSet cellConstraintSet = constraintSets[superPosition.ModuleIndex];
                return cellConstraintSet * superPosition.Orientations[i];
            }
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