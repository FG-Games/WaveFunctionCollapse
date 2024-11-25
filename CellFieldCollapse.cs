
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
                csp.CollapseRandom(_random);
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
        }


        // --- Constraints --- //

        private CellConstraintSet rotatedContraints(SuperPosition superPosition, int i) => _moduleSet.Modules[superPosition.ModuleIndex].Constraints * superPosition.Orientations[i]; // COMES OUT OF SUPERPOS 
        public CellConstraintSet rotatedContraints(CollapsedPosition collapsedPosition) => _moduleSet.Modules[collapsedPosition.ModuleIndex].Constraints * collapsedPosition.Orientation;

        public CellConstraintSet SuperConstraints(SuperPosition superPosition)
        {
            // Combine module constraints of all possible orientations
            CellConstraintSet superConstraints = rotatedContraints(superPosition, 0);

            for (int i = 1; i < superPosition.Orientations.Count; i ++)
                superConstraints += rotatedContraints(superPosition, i);

            return superConstraints;
        }

        public CellConstraintSet CombinedConstraints (CellSuperPosition<T, A> csp) // COMES OUT OF CellSuperPosition 
        {
            if(csp.Collapsed)
            {
                return rotatedContraints(csp.GetCollapsedPosition);
            }
            else
            {
                CellConstraintSet combinedConstraints = SuperConstraints(csp.SuperPositions[0]);

                for (int i = 1; i < csp.SuperPositions.Count; i++)
                    combinedConstraints += SuperConstraints(csp.SuperPositions[i]);

                return combinedConstraints;
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