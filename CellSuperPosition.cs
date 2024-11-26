using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class CellSuperPosition<T, A> : IHeapItem<CellSuperPosition<T, A>>
        where T : Module<T>
    {

        // --- CSP Field
        public A Address { get => _address; }


        // --- WFC
        private CellFieldCollapse<T, A> _wfc;
        public List<SuperPosition> SuperPositions; // USE CONSTRAINT HERE
        public int Entropy { get => getEntropy(); }
        public bool Collapsed { get => _collapsedPosition != -1; }
        [SerializeField] private A _address;
        [SerializeField] private int _collapsedPosition = -1;
        [SerializeField] private int _collapsedOrientation = -1;
        private event Action<CollapsedPosition> _collapse;


        // --- Memory
        private bool[] _adjacentEntropyChange;
        private CellSuperPosition<T, A>[] _adjacentCSParray;
        private IAdjacentCell<CellSuperPosition<T, A>> _adjacentCSP;
        CellConstraintSet _combinedConstraints;
        private SuperPosition _intersection;


        private int _recursionCounter; // TMP


        // --- Setup --- //

        public CellSuperPosition(A address, CellFieldCollapse<T, A> wfc) 
        {
            _address = address;
            _wfc = wfc;

            // WFC Events
            _collapsedPosition = -1;
            _collapsedOrientation = -1;
            SuperPositions = wfc.ModuleSet.SuperPositions; // DO YOU EVEN NEED wfc?
        }

        public void SubscribeToCollapse(Action<CollapsedPosition> action) => _collapse += action;


        // --- Collapse --- //

        public void Collapse (int superPosIndex, int orientationIndex)
        {
            setCollapsedPosition(superPosIndex);
            setCollapsedOrientation(orientationIndex);
            _collapse?.Invoke(GetCollapsedPosition);
        }

        public void CollapseToModule (int moduleIndex, System.Random random)
        {
            for (int i = 0; i < SuperPositions.Count; i ++)
            {
                if(SuperPositions[i].ModuleIndex == moduleIndex)
                {
                    setCollapsedPosition(i);
                    setCollapsedOrientation(random.Next(0, SuperPositions[_collapsedPosition].Orientations.Count));
                    _collapse?.Invoke(GetCollapsedPosition);
                    return;
                }
            }
            CollapseRandom(random);
        }

        public void CollapseRandom(System.Random random)
        {
            setCollapsedPosition(random.Next(0, SuperPositions.Count));
            setCollapsedOrientation(random.Next(0, SuperPositions[_collapsedPosition].Orientations.Count));
            _collapse?.Invoke(GetCollapsedPosition);
        }

        public void SetModule (int moduleIndex, int orientationIndex)
        {
            // This method assumes the CSP doesn't have any constraints and the sequence
            // of all orientations and modules is therefore identical with their array positions 
            _collapsedPosition = moduleIndex;
            _collapsedOrientation = orientationIndex;
            _collapse?.Invoke(GetCollapsedPosition);
        }

        private void setCollapsedPosition(int index) => _collapsedPosition = index;
        private void setCollapsedOrientation(int index) => _collapsedOrientation = SuperPositions[_collapsedPosition].Orientations[index];

        private int getEntropy()
        {
            int entropy = 0;

            for (int i = 0; i < SuperPositions.Count; i ++)
                entropy += SuperPositions[i].Orientations.Count;

            return entropy;
        }


        // --- Collapsed Position --- //

        public CollapsedPosition GetCollapsedPosition
        {
            get
            {
                if ( _collapsedPosition == -1 || _collapsedOrientation == -1)
                    UnityEngine.Debug.LogError("These's no collapsed position at " + Address);

                SuperPosition pos = SuperPositions[_collapsedPosition];
                SuperOrientation orientations = new SuperOrientation((int)Mathf.Pow(2, _collapsedOrientation));
                return new CollapsedPosition(pos.ModuleIndex, orientations.First);
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
                    UnityEngine.Debug.LogError("No collapse possible at " + Address);
            }

            entropyChange = Entropy != previousEntropy;
        }

        public void ConstraintAdjacentCells(CellConstraintSet[] cellConstraintSets)
        {
            // The constraints of adjacent cells recursively
            _adjacentCSP = _wfc._cspField.GetAdjacentCSP(Address);
            _adjacentEntropyChange = new bool[_adjacentCSP.Length];
            _combinedConstraints = _wfc.CombinedConstraints(this, cellConstraintSets);
            
            // Constraint adjacent cells and check for changes in entropy
            for (byte side = 0; side < _adjacentCSP.Length; side ++)
            {                
                if (_adjacentCSP.IsValid(side))
                    _adjacentCSP.GetCell(side).addConstraint(_combinedConstraints[side], out _adjacentEntropyChange[side]);
                else
                    _adjacentEntropyChange[side] = false;
            }

            // Constraint adjacent cells of all adjacent cells, had a change in entropy
            for (byte side = 0; side < _adjacentEntropyChange.Length; side ++)
            {
                if(_adjacentEntropyChange[side])
                {
                    _wfc.Add2EntropyHeap(_adjacentCSP.GetCell(side));
                    _adjacentCSP.GetCell(side).ConstraintAdjacentCells(cellConstraintSets);
                }
            }
        }

        // TEMPORARARY HACK
        // public CellConstraintSet CombinedConstraints => _wfc.CombinedConstraints(this);


        // --- Heap --- //

        [SerializeField] int _heapIndex;
        public int HeapIndex { get => _heapIndex; set => _heapIndex = value; }
        public int CompareTo(CellSuperPosition<T, A> cspToCompare)
        {
            int compare = Entropy.CompareTo(cspToCompare.Entropy);
            return -compare;
        }
    }

    public struct CollapsedPosition
    {
        public int ModuleIndex, Orientation;

        public CollapsedPosition(int moduleIndex, int orientation)
        {
            ModuleIndex = moduleIndex;
            Orientation = orientation;
        }
    }

    public interface IAdjacentCell<T>
    {
        int Length { get; }
        bool IsValid(int i);
        T GetCell(int i);
        void SetCell(int i, T value);
    }
}