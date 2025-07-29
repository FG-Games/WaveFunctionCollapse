/// <summary>
/// A CellSuperPosition (CSP) represents all possible Modules and their respective valid orientations
/// at a specific position(<A>) within a CSPField.
/// It is initialized from the MaxEntropyPosition of a ModuleSet and modified by applying CellConstraints.
/// The reduction in possible states is tracked via an Entropy value.
/// During the CSPField collapse process, the Entropy value is used to determine which CSP to collapse next.
/// When a CSP is collapsed, it resolves to a single Module index and a single Orientation index.
/// </summary>

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

        public CellSuperPosition(A address, CellConstraint maxEntropyPosition) 
        {
            _address = address;
            Constraint = maxEntropyPosition;
        }

        public CellSuperPosition(A address, CellConstraint superPositions, int superPosIndex, int superOrIndex) // Use this contructor to load partially collapsed CSP fields
        {
            _address = address;
            Constraint = superPositions;
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
            // This expects a MaxEntropyPosition or a the Superposition be uncontrainted,
            // as the moduleindex is based on the full set of modules

            if (Constraint.GetSuperPosition(moduleIndex).Possible())
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

        public CellConstraintSet GetConstraintSet(CellConstraintSet[] constraintSets)
        {
            // Creates CellConstraintSet from Superposition

            if(Collapsed)
            {
                CollapsedPosition collapsedPosition = GetCollapsedPosition;
                CellConstraintSet constraintSetCopy = constraintSets[collapsedPosition.ModuleIndex].Copy();
                constraintSetCopy.Rotate(collapsedPosition.Orientation);
                return constraintSetCopy;
            }
            else
            {
                CellConstraintSet combinedConstraints = getSuperConstraints(Constraint.GetPossiblePosition(0));

                for (int i = 1; i < Constraint.Count(); i++)
                {
                    CellConstraintSet superConstraints = getSuperConstraints(Constraint.GetPossiblePosition(i));
                    combinedConstraints.Add(superConstraints);
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
                }

                return superConstraints;
            }

            CellConstraintSet getRotatedContraints(SuperPosition superPosition, int i)
            {
                CellConstraintSet constraintSetCopy = constraintSets[superPosition.ModuleIndex].Copy();
                constraintSetCopy.Rotate(superPosition.Orientations.GetOrientation(i));
                return constraintSetCopy;
            }
        }

        public void AddConstraint(CellConstraint constraint, out bool entropyChange)
        {
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

    public interface IAdjacentCell<A>
    {
        int Length { get; }
        bool IsCollapsed(int i);
        CellSuperPosition<A> GetCell(int i);
    }
}