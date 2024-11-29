using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public class CellSuperPosition<A> : IHeapItem<CellSuperPosition<A>>
    {

        // --- CSP Field
        public A Address => _address;

        // --- WFC
        public CellConstraint Constraint;
        public int Entropy => Constraint.Entropy();
        public bool Collapsed => _collapsedPosIndex != -1;
        [SerializeField] private A _address;
        [SerializeField] private int _collapsedPosIndex = -1; // possible SuperPosition Index
        [SerializeField] private int _collapsedOrBitmask = -1; // valid Orientation Index
        private int maxOrientations(int index) => Constraint.Orientations(_collapsedPosIndex).Count();
        private event Action<CollapsedPosition> _collapse;


        // --- Setup --- //

        public CellSuperPosition(A address, CellConstraint constraint) 
        {
            _address = address;
            Constraint = constraint;
        }

        public CellSuperPosition(A address, CellConstraint constraint, int superPosIndex, int superOrIndex) 
        {
            _address = address;
            Constraint = constraint;
            _collapsedPosIndex = superPosIndex;
            _collapsedOrBitmask = superOrIndex;
        }

        public void SubscribeToCollapse(Action<CollapsedPosition> action) => _collapse += action;


        // --- Collapse --- //

        public void Collapse (int superPosIndex, int superOrIndex)
        {
            setCollapsedPosition(superPosIndex);
            setCollapsedOrientation(superOrIndex);
            _collapse?.Invoke(GetCollapsedPosition);
        }

        public void CollapseToModule (int moduleIndex, System.Random random)
        {
            if(Constraint.GetSuperPosition(moduleIndex).Possible()) // THIS IS SUPER DANGEROUS AS THER INPUT IS A DIRECT INDEX AND NOT A POSSIBLE
            {
                setCollapsedPosition(moduleIndex);
                setCollapsedOrientation(random.Next(0, maxOrientations(_collapsedPosIndex)));
                _collapse?.Invoke(GetCollapsedPosition);
                return;
            }

            CollapseRandom(random);
        }

        public void CollapseRandom(System.Random random)
        {
            setCollapsedPosition(random.Next(0, Constraint.Count()));
            setCollapsedOrientation(random.Next(0, maxOrientations(_collapsedPosIndex)));
            _collapse?.Invoke(GetCollapsedPosition);
        }

        private void setCollapsedPosition(int index) => _collapsedPosIndex = index;
        private void setCollapsedOrientation(int index) => _collapsedOrBitmask = Constraint.Orientations(_collapsedPosIndex).GetOrientation(index);


        // --- Collapsed Position --- //

        public CollapsedPosition GetCollapsedPosition
        {
            get
            {
                if ( _collapsedPosIndex == -1 || _collapsedOrBitmask == -1)
                    UnityEngine.Debug.LogError("These's no collapsed position at " + Address);

                SuperPosition pos = Constraint.GetPossiblePosition(_collapsedPosIndex);
                SuperOrientation orientations = new SuperOrientation((int)Mathf.Pow(2, _collapsedOrBitmask));
                return new CollapsedPosition(pos.ModuleIndex, orientations.First());
            }
        }


        // --- Constraints --- //

        public CellConstraintSet CombinedConstraints (CellConstraintSet[] constraintSets)
        {
            // Creates CellConstraintSet from Superposition

            if(Collapsed)
            {
                CollapsedPosition collapsedPosition = GetCollapsedPosition;
                CellConstraintSet cellConstraintSet = constraintSets[collapsedPosition.ModuleIndex].Copy();
                cellConstraintSet.Rotate(collapsedPosition.Orientation);
                return cellConstraintSet;
            }
            else
            {
                CellConstraintSet combinedConstraints = getSuperConstraints(Constraint.GetPossiblePosition(0));

                for (int i = 1; i < Constraint.Count(); i++)
                {
                    CellConstraintSet superConstraints = getSuperConstraints(Constraint.GetPossiblePosition(i));
                    combinedConstraints.Add(superConstraints);
                    superConstraints.Dispose();
                }

                return combinedConstraints;
            }

            CellConstraintSet getSuperConstraints(SuperPosition superPosition)
            {
                // Combine module constraints of all possible orientations
                CellConstraintSet superConstraints = getRotatedContraints(superPosition, 0);

                for (int i = 1; i < superPosition.Orientations.Count(); i ++)
                {
                    CellConstraintSet rotatedConstrainst = getRotatedContraints(superPosition, i);
                    superConstraints.Add(rotatedConstrainst);
                    rotatedConstrainst.Dispose();
                }

                return superConstraints;
            }

            CellConstraintSet getRotatedContraints(SuperPosition superPosition, int i)
            {
                CellConstraintSet cellConstraintSet = constraintSets[superPosition.ModuleIndex].Copy();
                cellConstraintSet.Rotate(superPosition.Orientations.GetOrientation(i));
                return cellConstraintSet;
            }
        }

        public void AddConstraint(CellConstraint constraint, out bool entropyChange)
        {
            // Adds constraint to SuperPositions

            int previousEntropy = Entropy;
            Constraint.Intersection(constraint);

            if(Constraint.Count() == 0)
                    UnityEngine.Debug.LogError("No collapse possible at " + Address);

            entropyChange = Entropy != previousEntropy;
        }


        // --- Heap --- //

        [SerializeField] int _heapIndex;
        public int HeapIndex { get => _heapIndex; set => _heapIndex = value; }
        public int CompareTo(CellSuperPosition<A> cspToCompare)
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