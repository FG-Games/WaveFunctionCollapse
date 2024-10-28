using System;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class CellSuperPosition<T, A> : IHeapItem<CellSuperPosition<T, A>>
        where T : Module<T>
    {
        public Cell<T, A> Cell;
        public List<SuperPosition> SuperPositions; // USE CONSTRAINT HERE
        public int Entropy { get => getEntropy(); }
        public bool Collapsed { get => _collapsedPosition != -1; }
        private CellFieldCollapse<T, A> _wfc;
        private event Action<SuperPosition> _collapse;
        [SerializeField] private int _collapsedPosition = -1;
        [SerializeField] private int _collapsedOrientation = -1;
        private ModuleSet<T> _moduleSet;
        private IAdjacentCell<Cell<T, A>> _adjacentCells;
        private bool[] _adjacentEntropyChange;
        private CellSuperPosition<T, A>[] _adjacentCSP;
        private SuperPosition _intersection;


        private int _recursionCounter; // TMP


        // --- Setup --- //

        public CellSuperPosition(Cell<T, A> cell, CellFieldCollapse<T, A> wfc, ModuleSet<T> moduleSet)
        {
            Cell = cell;
            _wfc = wfc;
            _moduleSet = moduleSet;

            // WFC Events
            _collapse += Cell.OnCollapse;
            setSuperPosition();
        }

        private void setSuperPosition()
        {
            _collapsedPosition = -1;
            _collapsedOrientation = -1;

            // Load Modules
            T[] allModules = _moduleSet.Modules;
            SuperPositions = new List<SuperPosition>(allModules.Length);
            
            for (int i = 0; i < allModules.Length; i ++)
                SuperPositions.Add(new SuperPosition(allModules[i].Orientations, i));
        }

        public void SubscribeToCollapse(Action<SuperPosition> action) => _collapse += action;


        // --- Collapse --- //

        public void Collapse (int superPosIndex, int orientationIndex)
        {
            setCollapsedPosition(superPosIndex);
            setCollapsedOrientation(orientationIndex);
            collapseSuperPosition();
        }

        public void CollapseToModule (int moduleIndex, System.Random random)
        {
            for (int i = 0; i < SuperPositions.Count; i ++)
            {
                if(SuperPositions[i].ModuleIndex == moduleIndex)
                {
                    setCollapsedPosition(i);
                    setCollapsedOrientation(random.Next(0, SuperPositions[_collapsedPosition].Orientations.Count));
                    collapseSuperPosition();
                    return;
                }
            }
            CollapseRandom(random);
        }

        public void CollapseRandom(System.Random random)
        {
            setCollapsedPosition(random.Next(0, SuperPositions.Count));
            setCollapsedOrientation(random.Next(0, SuperPositions[_collapsedPosition].Orientations.Count));
            collapseSuperPosition();
        }

        public void SetModule (int moduleIndex, int orientationIndex)
        {
            // This method assumes the CSP doesn't have any constraints and the sequence
            // of all orientations and modules is therefore identical with their array positions 
            _collapsedPosition = moduleIndex;
            _collapsedOrientation = orientationIndex;
            _collapse?.Invoke(CollapsedPosition);
        }

        private void setCollapsedPosition(int index) => _collapsedPosition = index;
        private void setCollapsedOrientation(int index) => _collapsedOrientation = SuperPositions[_collapsedPosition].Orientations[index];

        private void collapseSuperPosition()
        {
            _collapse?.Invoke(CollapsedPosition);

            _recursionCounter = 0;
            ConstraintAdjacentCells();
        }

        private int getEntropy()
        {
            int entropy = 0;

            for (int i = 0; i < SuperPositions.Count; i ++)
                entropy += SuperPositions[i].Orientations.Count;

            return entropy;
        }


        // --- Collapsed Position --- //

        public SuperPosition CollapsedPosition
        {
            get
            {
                if ( _collapsedPosition == -1 || _collapsedOrientation == -1)
                    UnityEngine.Debug.LogError("These's no collapsed position at " + Cell.Address);

                SuperPosition pos = SuperPositions[_collapsedPosition];
                SuperOrientation orientations = new SuperOrientation((int)Mathf.Pow(2, _collapsedOrientation));
                return new SuperPosition(orientations, pos.ModuleIndex);
            }
        }


        // --- Constraints --- //

        
        private void addConstraint(CellConstraint constraint, out bool entropyChange)
        {
            _recursionCounter ++;

            if(Collapsed)
            {
                entropyChange = false;
                return;
            }

            int previousEntropy = Entropy;

            // Go through SuperPosition and check for common States / intersections
            for (int i = 0; i < SuperPositions.Count; i ++)
            {
                // Test SuperPosition against SuperPositions in Constraint
                if(constraint.Intersection(SuperPositions[i], out _intersection))
                {
                    SuperPositions[i] = _intersection;
                }
                else
                {
                    SuperPositions.Remove(SuperPositions[i]);
                    i--;                    
                }

                if(SuperPositions.Count == 0)
                    UnityEngine.Debug.LogError("No collapse possible at " + Cell.Address);
            }

            entropyChange = Entropy != previousEntropy;            
        }

        public void ConstraintAdjacentCells()
        {
            // The constraints of adjacent cells recursively

            if(_adjacentCells == null)
            {
                _adjacentCells = Cell.GetAdjacentCells();
                _adjacentEntropyChange = new bool[_adjacentCells.Length];
                _adjacentCSP = new CellSuperPosition<T, A>[_adjacentCells.Length];
            }

            for (byte side = 0; side < _adjacentCells.Length; side ++)
            {
                // Constraint adjacent cells and check for changes in entropy
                if (_adjacentCells.IsValid(side))
                {
                    _adjacentCSP[side] = _wfc.GetCellSuperPosition(_adjacentCells.GetCell(side).Address);
                    _adjacentCSP[side].addConstraint(_wfc.CombinedConstraints(this)[side], out _adjacentEntropyChange[side]);
                }
                else
                {
                    _adjacentEntropyChange[side] = false;
                }
            }

            // Constraint adjacent cells of all adjacent cells, had a change in entropy
            for (byte side = 0; side < _adjacentEntropyChange.Length; side ++)
            {
                if(_adjacentEntropyChange[side])
                {
                    _wfc.Add2EntropyHeap(_adjacentCSP[side]);
                    _adjacentCSP[side].ConstraintAdjacentCells();
                }
            }
        }

        // TEMPORARARY HACK

        public CellConstraintSet CombinedConstraints => _wfc.CombinedConstraints(this);


        // --- Heap --- //

        [SerializeField] int _heapIndex;
        public int HeapIndex { get => _heapIndex; set => _heapIndex = value; }
        public int CompareTo(CellSuperPosition<T, A> cspToCompare)
        {
            int compare = Entropy.CompareTo(cspToCompare.Entropy);
            return -compare;
        }
    }

    public interface ICSPfield<T, A>
        where T : Module<T>
    {
        int Count { get; }
        CellSuperPosition<T, A> GetCellSuperPosition(A a);
    }

    public interface IAdjacentCell<T>
    {
        int Length { get; }
        bool IsValid(int i);
        T GetCell(int i);
        void SetCell(int i, T value);
    }
}