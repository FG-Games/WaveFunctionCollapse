/// <summary>
/// A CellConstraintSet represents the characteristic constraints that a Module or CellSuperPosition 
/// imposes on its neighbors. It acts both as:
/// 
/// • The static definition of adjacency rules for a single Module (provided by its Features).  
/// • A dynamic aggregation of constraints during the CSPField collapse, where multiple possible 
///   Modules are combined into a single set using the boolean operations of CellConstraint.
/// 
/// The length of the internal CellConstraint array corresponds to the tessellation of the CSPField. 
/// During collapse, CellConstraintSets are derived from constrained CellSuperPositions 
/// to propagate valid possibilities throughout the field.
/// </summary>

using System;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraintSet
    {
        private CellConstraint[] _constraints;
        private int _orientation;

        public CellConstraintSet(CellConstraint[] cellConstraints)
        {
            _constraints = cellConstraints;
            _orientation = 0;
        }

        public CellConstraintSet Copy()
        {
            CellConstraint[] constraintsCopy = new CellConstraint[_constraints.Length];

            for (int i = 0; i < constraintsCopy.Length; i ++)
                constraintsCopy[i] = _constraints[i].Copy();

            return new CellConstraintSet(constraintsCopy);
        }


        // --- Basic Access --- //

        public int Length()
        {
            return _constraints.Length;
        }

        public CellConstraint GetCellConstraint(int index)
        {
            return _constraints[addRotations(_orientation, index)];
        }


        // --- Operations --- //

        public void Add(CellConstraintSet addition)
        {
            for(int i = 0; i < _constraints.Length; i++)
                GetCellConstraint(i).Add(addition.GetCellConstraint(i));
        }

        public void Rotate(int rotation)
        {
            _orientation = subtractRotations(_orientation, rotation);

            for (int i = 0; i < _constraints.Length; i ++)
                GetCellConstraint(i).Rotate(rotation);
        }

        private int addRotations(int rotationA, int rotationB)
        {
            return (rotationA + rotationB) % _constraints.Length;
        }

        private int subtractRotations(int rotationA, int rotationB)
        {
            int addedRotations = rotationA - rotationB;

            if (addedRotations < 0)
                addedRotations += _constraints.Length;

            return addedRotations;
        }
    }
}