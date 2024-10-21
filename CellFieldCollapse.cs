using UnityEngine;
using Unity.Collections;

namespace WaveFunctionCollapse
{
    public abstract class CellFieldCollapse<T, A>
        where T : Module<T>
    {
        public bool AllCellsCollapsed { get => _entropyHeap.Count == 0; }
        private ICSPfield<T, A> _cspField;
        private Heap<CellSuperPosition<T, A>> _entropyHeap;
        private System.Random _random;

        // Constraint provision
        ModuleSet<T> _moduleSet;
        private CellConstraintSet[] _moduleConstraintSets;


        // Constraint propagation
        private CellSuperPosition<T, A> _csp;
        protected IAdjacentType<A> _adjacentCSP;
        protected IAdjacentType<bool> _adjacentChanges;
        bool _entropyChange;
        CellConstraintSet _constraintSet;

        public CellFieldCollapse(CellField<T, A> cellField)
        {
            _moduleSet = cellField.ModuleSet;
            _cspField = cellField.CreateCellSuperPositions(this);
            _entropyHeap = new Heap<CellSuperPosition<T, A>>(_cspField.Count);
            _random = new System.Random(cellField.Seed);

            setUpAdjacentTypes();
            provideModuleConstraints();
        }

        protected abstract void setUpAdjacentTypes();

        public void UpdateSeed(int seed) => _random = new System.Random(seed);

        public CellConstraint UnconstraintedSuperPositions { get => CreateUnmanaged<T>.CellConstraint(_moduleSet); }

        public CellSuperPosition<T, A> CSPdata(A address) => _cspField.GetCSP(address);
        public void CollapseAt(A address, int moduleIndex, int orientationIndex) => _cspField.CollapseAt(address, moduleIndex, orientationIndex);

        private void AddToEntropyHeap(CellSuperPosition<T, A> csp)
        {
            if(csp.Collapsed)
                Debug.LogError("Can't register collapsed cell as SuperPosition");
            else if(csp.HeapIndex == -1)
                _entropyHeap.Add(csp);
            else if (_entropyHeap.Contains(csp))
                _entropyHeap.UpdateItem(csp);
            else
                Debug.LogWarning("Weird shit");
        }

        public void CollapseNext() // CollapseNext returns address to updaty constraints / the constraint update would be a recursive process
        {
            CellSuperPosition<T, A> next = _entropyHeap.RemoveFirst();

            if (next.Collapsed)
            {
                CollapseNext();
            }
            else
            {
                next.CollapseRandom(_random);
                ConstraintAdjacentCells(next);
            }
        }

        public void CollapseAll()
        {
            while (!AllCellsCollapsed)
                CollapseNext();

            disposeNativeArrays();
        }

        public void CollapseAt(A address, int modulIndex)
        {
            _csp = _cspField.GetCSP(address);
            _csp.CollapseToModule(modulIndex, _random);
            ConstraintAdjacentCells(_csp);
        }


        // --- Module Constraint Provision --- //

        private void provideModuleConstraints()
        {
            //if(!_moduleConstraintSets.IsCreated)
            //{
                _moduleConstraintSets = new CellConstraintSet[_moduleSet.Modules.Length];

                for (int i = 0; i < _moduleSet.Modules.Length; i++)
                    _moduleConstraintSets[i] = _moduleSet.Modules[i].Constraints;
            //}
        }

        private void disposeNativeArrays()
        {
            //if (_moduleConstraintSets.IsCreated)
            //    _moduleConstraintSets.Dispose();

            //_cspField.LoopThroughCells((A address) => _cspField.GetCSP(address).SuperPositions.Dispose());
        }

        private CellConstraintSet rotatedContraints(int i, int rotation)
        {
            return _moduleConstraintSets[i] * rotation;
        }


        // --- Constraints --- //

        public void ConstraintAdjacentCells(A address) => ConstraintAdjacentCells( _cspField.GetCSP(address));

        public void ConstraintAdjacentCells(CellSuperPosition<T, A> csp)
        {
            _constraintSet = CellSuperConstraints(csp);
            _adjacentCSP = _cspField.AdjacentCSPaddresses(csp.Address);

            for (byte side = 0; side < _adjacentCSP.Count; side ++)
            {
                // Constraint adjacent cells and check for changes in entropy
                if(_adjacentCSP.isValid(side))
                {
                    _csp = _cspField.GetCSP(_adjacentCSP.get(side));
                    _csp.AddConstraint(_constraintSet[side], out _entropyChange, _cspField);
                    _adjacentChanges.set(side, _entropyChange);
                }
            }

            // Constraint adjacent cells of all adjacent cells, that had a change in entropy
            for (byte side = 0; side < _adjacentChanges.Count; side ++)
            {
                if(_adjacentChanges.get(side))
                {
                    _csp = _cspField.GetCSP(_adjacentCSP.get(side));
                    AddToEntropyHeap(_csp);
                    ConstraintAdjacentCells(_csp);
                }
            }
        }

        private CellConstraintSet superConstraints(SuperPosition superPosition)
        {
            // Create constraint of all possible module orientations
            CellConstraintSet superConstraints = rotatedContraints(superPosition.ModuleIndex, superPosition.Orientations.First);

            for (int i = 1; i < superPosition.Orientations.Count; i ++)
                superConstraints += rotatedContraints(superPosition.ModuleIndex, superPosition.Orientations[i]);

            return superConstraints;
        }

        public CellConstraintSet CellSuperConstraints(CellSuperPosition<T, A> csp)
        {
            if(csp.Collapsed)
            {
                return rotatedContraints(csp.DefinedRotatedModule.x, csp.DefinedRotatedModule.y);
            }
            else
            {
                CellConstraintSet combinedConstraints = superConstraints(csp.SuperPositions.GetPossiblePosition(0));

                for (int i = 1; i < csp.SuperPositions.PossiblePositions; i++)
                    combinedConstraints += superConstraints(csp.SuperPositions.GetPossiblePosition(i));

                return combinedConstraints;
            }
        }
    }
}