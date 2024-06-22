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
        public List<SuperPosition<T>> SuperPositions; // Multiple module options each in multiple orientations
        public int Entropy { get => getEntropy(); }
        public bool Collapsed { get => _collapsedPosition != -1; }
        private CellFieldCollapse<T, A> _wfc;
        private event Action<SuperPosition<T>> _collapse;
        [SerializeField] private int _collapsedPosition = -1;
        [SerializeField] private int _collapsedOrientation = -1;
        private List<int> _activeModules; // PriorityModules
        private Cell<T, A>[] _adjacentCells;
        private CellSuperPosition<T, A> _adjacentCSP;
        private SuperPosition<T> _intersection;


        // --- Setup --- //

        public CellSuperPosition(Cell<T, A> cell, CellFieldCollapse<T, A> wfc)
        {
            Cell = cell;
            _wfc = wfc;            

            // WFC Events
            _collapse += Cell.OnCollapse;
            setSuperPosition();
        }

        private void setSuperPosition()
        {
            _collapsedPosition = -1;
            _collapsedOrientation = -1;

            // Load Modules
            List<T> allModules = ModuleImporter<T>.GetAllModules();
            SuperPositions = new List<SuperPosition<T>>();
            
            for (int i = 0; i < allModules.Count; i ++)
                SuperPositions.Add(new SuperPosition<T>(allModules[i]));
        }

        public void SubscribeToCollapse(Action<SuperPosition<T>> action) => _collapse += action;


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
                if(SuperPositions[i].Module.Index == moduleIndex)
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
            _activeModules = new List<int>();

            for(int i = 0; i < SuperPositions.Count; i ++)
                if(!SuperPositions[i].Module.Passive)
                    _activeModules.Add(i);

            if(_activeModules.Count > 0)
                setCollapsedPosition(_activeModules[random.Next(0, _activeModules.Count)]);
            else
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

        public SuperPosition<T> CollapsedPosition
        {
            get
            {
                if ( _collapsedPosition == -1 || _collapsedOrientation == -1)
                    Debug.LogError("These's no collapsed position at " + Cell.Address);

                SuperPosition<T> pos = SuperPositions[_collapsedPosition];
                SuperOrientation orientations = new SuperOrientation((int)Mathf.Pow(2, _collapsedOrientation));
                return new SuperPosition<T>(orientations, pos.Module);
            }
        }


        // --- Constraints --- //

        public CellConstraintSet<T> CombinedConstraints
        {
            get
            {
                if(Collapsed)
                {
                    return CollapsedPosition.RotatedContraints(0);
                }
                else
                {
                    CellConstraintSet<T> combinedConstraints = SuperPositions[0].SuperConstraints;

                    for (int i = 1; i < SuperPositions.Count; i++)
                        combinedConstraints += SuperPositions[i].SuperConstraints;

                    return combinedConstraints;
                }
            }
        }
        
        private void addConstraint(CellConstraint<T> constraint)
        {
            if(Collapsed)
                return;

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
                    Debug.LogError("No collapse possible at " + Cell.Address);
            }

            // Check if constraint had effect on entropy
            if(Entropy == previousEntropy)
                return;
    
            // Propagate effect
            _wfc.Add2EntropyHeap(this);
            ConstraintAdjacentCells(); // TEMPORARILY TURNED OFF
        }

        public void ConstraintAdjacentCells()
        {
            // The constraints of adjacent cells should ripple / iteratively through the map:
            // If a collapsed cell enforces a coast tile next to it this will constraints it's neighbours
            // Combine AdjacentSuperModule<T> of all SuperOrientedModule<T> and continue waves of constrainst
            // the chain of constraints must only stop if the contraint doesn't have any effect on the cell
            // You could just use this method, but use combined constraints in case _collapsedState == -1

            if(_adjacentCells == null)
                _adjacentCells = Cell.GetAdjacentCells();

            for (byte side = 0; side < _adjacentCells.Length; side ++)
            {
                if (_adjacentCells[side] == null)
                    continue;

                _adjacentCSP = _wfc.GetCellSuperPosition(_adjacentCells[side].Address);
                _adjacentCSP.addConstraint(CombinedConstraints[side]);
            }
        }


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
}