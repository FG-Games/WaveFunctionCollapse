using UnityEngine;

namespace WaveFunctionCollapse
{
    public class CellFieldCollapse<T, A>
        where T : Module<T>
    {
        public bool AllCellsCollapsed { get => _entropyHeap.Count == 0; }
        private ICSPfield<T, A> _cspField;
        private Heap<CellSuperPosition<T, A>> _entropyHeap;
        private System.Random _random;
        private ModuleSet<T> _moduleSet;


        public CellFieldCollapse(ICellField<T, A> cellField, ModuleSet<T> moduleSet)
        {
            _moduleSet = moduleSet;
            _cspField = cellField.CreateCellSuperPositions(this);
            _entropyHeap = new Heap<CellSuperPosition<T, A>>(_cspField.Count);
            _random = new System.Random(cellField.Seed);
        }

        public void UpdateSeed(int seed) => _random = new System.Random(seed);

        public CellSuperPosition<T, A> GetCellSuperPosition(A address) => _cspField.GetCellSuperPosition(address);

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

        public void CollapseAll()
        {
            while (!AllCellsCollapsed)
                CollapseNext();
        }


        public void CollapseAt(A address, int index)
        {
            _cspField.GetCellSuperPosition(address).CollapseToModule(index, _random);
        }


        // --- Constraints --- //

        public CellConstraintSet RotatedContraints(SuperPosition superPosition, int i) => _moduleSet.Modules[superPosition.ModuleIndex].Constraints * superPosition.Orientations[i]; // COMES OUT OF SUPERPOS 

        public CellConstraintSet SuperConstraints(SuperPosition superPosition)
        {
            // Combine module constraints of all possible orientations
            CellConstraintSet superConstraints = RotatedContraints(superPosition, 0);

            for (int i = 1; i < superPosition.Orientations.Count; i ++)
                superConstraints += RotatedContraints(superPosition, i);

            return superConstraints;
        }

        public CellConstraintSet CombinedConstraints (CellSuperPosition<T, A> csp) // COMES OUT OF CellSuperPosition 
        {
            if(csp.Collapsed)
            {
                return RotatedContraints(csp.CollapsedPosition, 0);
            }
            else
            {
                CellConstraintSet combinedConstraints = SuperConstraints(csp.SuperPositions[0]);

                for (int i = 1; i < csp.SuperPositions.Count; i++)
                    combinedConstraints += SuperConstraints(csp.SuperPositions[i]);

                return combinedConstraints;
            }
        }
    }
}