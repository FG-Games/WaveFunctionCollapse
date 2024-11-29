using System;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraintSet : IDisposable
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

        public void Dispose()
        {
            // Dispose native array here and all nested native arrays
        }


        // --- Operations --- //

        public void Add(CellConstraintSet addition)
        {
            for(int i = 0; i < _constraints.Length; i++)
                GetCellConstraint(i).Add(addition.GetCellConstraint(i));
        }

        public void Rotate(int rotation)
        {
            _orientation = addRotations(_orientation, rotation);

            for (int i = 0; i < _constraints.Length; i ++)
                GetCellConstraint(i).Rotate(rotation);
        }

        private int addRotations(int rotationA, int rotationB)
        {
            return (rotationA + rotationB) % _constraints.Length;
        }
    }
}