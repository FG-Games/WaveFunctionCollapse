using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class CellSuperPosition<T, A> : IHeapItem<CellSuperPosition<T, A>>
        where T : Module<T>
    {

        // --- CSP Field
        public A Address => _address;

        // --- WFC
        public CellConstraint SuperPositions;
        public int Entropy => SuperPositions.Entropy;
        public bool Collapsed => _collapsedPosition != -1;
        [SerializeField] private A _address;
        [SerializeField] private int _collapsedPosition = -1;
        [SerializeField] private int _collapsedOrientation = -1;
        private int maxOrientations(int index) => SuperPositions.Orientations(_collapsedPosition).Count;
        private event Action<CollapsedPosition> _collapse;


        // --- Setup --- //

        public CellSuperPosition(A address, CellConstraint superPositions) 
        {
            _address = address;

            // WFC Events
            _collapsedPosition = -1;
            _collapsedOrientation = -1;
            SuperPositions = superPositions;
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
            if(SuperPositions[moduleIndex].Possible)
            {
                setCollapsedPosition(moduleIndex);
                setCollapsedOrientation(random.Next(0, maxOrientations(_collapsedPosition)));
                _collapse?.Invoke(GetCollapsedPosition);
                return;
            }

            CollapseRandom(random);
        }

        public void CollapseRandom(System.Random random)
        {
            setCollapsedPosition(random.Next(0, SuperPositions.Count));
            setCollapsedOrientation(random.Next(0, maxOrientations(_collapsedPosition)));
            _collapse?.Invoke(GetCollapsedPosition);
        }

        private void setCollapsedPosition(int index) => _collapsedPosition = index;
        private void setCollapsedOrientation(int index) => _collapsedOrientation = SuperPositions.Orientations(_collapsedPosition)[index];


        // --- Collapsed Position --- //

        public CollapsedPosition GetCollapsedPosition
        {
            get
            {
                if ( _collapsedPosition == -1 || _collapsedOrientation == -1)
                    UnityEngine.Debug.LogError("These's no collapsed position at " + Address);

                SuperPosition pos = SuperPositions.GetPossiblePosition(_collapsedPosition);
                SuperOrientation orientations = new SuperOrientation((int)Mathf.Pow(2, _collapsedOrientation));
                return new CollapsedPosition(pos.ModuleIndex, orientations.First);
            }
        }


        // --- Constraints --- //
        
        public void AddConstraint(CellConstraint constraint, out bool entropyChange)
        {
            // Adds constraint to SuperPositions
            if(Collapsed)
            {
                entropyChange = false;
                return;
            }

            int previousEntropy = Entropy;

            // Go through SuperPosition and check for common States / intersections
            for (int i = 0; i < SuperPositions.Length; i ++)
                SuperPositions[i] = SuperPositions[i].Intersection(constraint[i]);

            if(SuperPositions.Count == 0)
                    UnityEngine.Debug.LogError("No collapse possible at " + Address);

            entropyChange = Entropy != previousEntropy;
        }

        public CellConstraintSet CombinedConstraints (CellConstraintSet[] constraintSets)
        {
            // Creates CellConstraintSet from Superposition

            if(Collapsed)
            {
                CollapsedPosition collapsedPosition = GetCollapsedPosition;
                CellConstraintSet cellConstraintSet = constraintSets[collapsedPosition.ModuleIndex];
                return cellConstraintSet * collapsedPosition.Orientation;
            }
            else
            {
                CellConstraintSet combinedConstraints = SuperConstraints(SuperPositions.GetPossiblePosition(0));

                for (int i = 1; i < SuperPositions.Count; i++)
                    combinedConstraints += SuperConstraints(SuperPositions.GetPossiblePosition(i));

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

    public interface IAdjacentCell<T> // THIS IS EXCLUSIVELY USED FOR CSPs.. 
    {
        int Length { get; }
        bool IsValid(int i); // THIS KINDA OBSCURES THE ANSWER ON WHETEHR OR NOT THE THING IS COLLAPSED
        T GetCell(int i);
        void SetCell(int i, T value);
    }
}