using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class CellSuperPosition<T, A> : IHeapItem<CellSuperPosition<T, A>>
        where T : Module<T>
    {
        //public Cell<T, A> Cell;
        public CellConstraint SuperPositions; // Multiple module options each in multiple orientations // TRY THIS AS A NATIVE ARRAY
        public A Address { get => _address; }
        public int Entropy { get => SuperPositions.Entropy; }
        public int CollapsedPosition { get => _collapsedPosition; }
        public int CollapsedOrientation { get => _collapsedOrientation; }
        public bool Collapsed { get => _collapsedPosition != -1; }
        [SerializeField] private A _address;        
        [SerializeField] private int _collapsedPosition;
        [SerializeField] private int _collapsedOrientation;
        private event Action<Vector2Int> _collapse;        


        // --- Setup --- //

        public CellSuperPosition(Cell<T, A> cell, CellConstraint superPositions)
        {         
            SuperPositions = superPositions;            
            _address = cell.Address;

            // WFC Events
            _heapIndex = -1;
            _collapsedPosition = -1;
            _collapsedOrientation = -1;
            _collapse = delegate { };
            _collapse += cell.OnCollapse;            
        }

        public void SubscribeToCollapse(Action<Vector2Int> action) => _collapse += action;


        // --- Constraints --- //

        public void AddConstraint(CellConstraint constraint, out bool  entropyChange, ICSPfield<T, A> DEBUGFIELD)
        {
            int previousEntropy = Entropy;
            SuperPositions.Intersection(constraint);
            entropyChange = previousEntropy != Entropy;
        }


        // --- Collapse --- //

        public void Collapse (int moduleIndex, int orientationIndex)
        {
            _collapsedPosition = moduleIndex;
            _collapsedOrientation = orientationIndex;
            _collapse?.Invoke(DefinedRotatedModule);
        }        

        public void CollapseRandom(System.Random random)
        {
            int moduleIndex = random.Next(0, SuperPositions.Count);
            int orientationIndex = random.Next(0, SuperPositions[_collapsedPosition].Orientations.Count);
            Collapse(moduleIndex, orientationIndex);
        }
        
        public void CollapseToModule (int moduleIndex, System.Random random)
        {
            for (int i = 0; i < SuperPositions.Count; i ++)
            {
                if(SuperPositions[i].ModuleIndex == moduleIndex)
                {
                    int orientation = random.Next(0, SuperPositions[i].Orientations.Count);
                    Collapse(i, orientation);
                    return;
                }
            }
            CollapseRandom(random);
        }


        // --- Collapsed Position --- //

        public Vector2Int DefinedRotatedModule
        {
            get
            {
                if ( _collapsedPosition == -1 || _collapsedOrientation == -1)
                    UnityEngine.Debug.LogError("These's no collapsed position at " + _address);

                return new Vector2Int(_collapsedPosition, _collapsedOrientation);
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
        CellSuperPosition<T, A> GetCSP(A a);
        IAdjacentType<A> AdjacentCSPaddresses(A a);
        void LoopThroughCells(Action<A> action);

        void CollapseAt(A a, int moduleIndex, int orientationIndex);
        void CollapseRandomAt(A a, System.Random random);
        void CollapseToModuleAt(A a, int moduleIndex, System.Random random);
        bool Collapsed(A a);
    }

    public interface IAdjacentType<T>
    {
        int Count { get; }
        bool isValid(int i);
        T get(int i);
        void set(int i, T value);
    }
}