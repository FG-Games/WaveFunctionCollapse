using System;

namespace WaveFunctionCollapse
{
    public struct CellConstraint : IDisposable
    {         
        public SuperPosition[] SuperPositions;

        public CellConstraint(SuperPosition[] superPositions)
        {
            SuperPositions = superPositions;
        }


        // --- Basic Access --- //

        public int Length()
        {
            return SuperPositions.Length;
        }

        public int Count()
        {
            int count = 0;

            for (int i = 0; i < SuperPositions.Length; i ++)
                if(GetSuperPosition(i).Possible())
                    count ++;

            return count;
        }

        public int Entropy()
        {
            int entropy = 0;

            for (int i = 0; i < SuperPositions.Length; i ++)
                entropy += GetSuperPosition(i).Orientations.Count();

            return entropy;
        }


        // --- Basic Access --- //

        public SuperPosition GetSuperPosition(int index)
        {
            return SuperPositions[index];
        }

        public void SetSuperPosition(int index, SuperPosition superPosition)
        {
            SuperPositions[index] = superPosition;
        }        

        public SuperPosition GetPossiblePosition(int possibleIndex)
        {
            int counter = -1;

            for (int i = 0; i < SuperPositions.Length; i ++)
            {
                if(SuperPositions[i].Possible())
                {
                    counter ++;

                    if(possibleIndex == counter)
                        return SuperPositions[i];
                }
            }

            return default(SuperPosition);
        }

        public SuperOrientation Orientations(int index)
        {
            return GetPossiblePosition(index).Orientations;
        }

        public void Dispose()
        {
            // Dispose native array here
        }


        // --- Operations --- //

        public CellConstraint Add(CellConstraint constraint)
        {
            SuperPosition[] mergedConstraint = new SuperPosition[SuperPositions.Length];

            for (int i = 0; i < SuperPositions.Length; i ++)
                mergedConstraint[i] = SuperPositions[i].Union(constraint.GetSuperPosition(i));

            return new CellConstraint(mergedConstraint);
        }

        public CellConstraint Rotate(int rotation)
        {
            SuperPosition[] offsetSuperPositions = new SuperPosition[SuperPositions.Length];

            for (int i = 0; i < SuperPositions.Length; i ++)
                offsetSuperPositions[i] = SuperPositions[i].Rotate(rotation);

            return new CellConstraint(offsetSuperPositions);
        }
    }

    [Serializable]
    public struct CellConstraintSet : IDisposable
    {
        private CellConstraint[] _constraints;

        public CellConstraintSet(CellConstraint[] cellConstraints)
        {
            _constraints = cellConstraints;
        }


        // --- Basic Access --- //

        public int Length()
        {
            return _constraints.Length;
        }

        public CellConstraint GetCellConstraint(int index)
        {
            return _constraints[index];
        }

        public void SetCellConstraint(int index, CellConstraint cellConstraint)
        {
            _constraints[index] = cellConstraint;
        }

        public void Dispose()
        {
            // Dispose native array here
        }


        // --- Operations --- //

        public CellConstraintSet Add(CellConstraintSet addition)
        {
            CellConstraint[] addedSet = new CellConstraint[_constraints.Length];

            for(int i = 0; i < _constraints.Length; i++)
                addedSet[i] = _constraints[i].Add(addition.GetCellConstraint(i));

            return new CellConstraintSet(addedSet);
        }

        public CellConstraintSet Rotate(int rotation)
        {
            CellConstraint[] rotatedSet = new CellConstraint[_constraints.Length];

            for (int i = 0; i < _constraints.Length; i ++)
                rotatedSet[i] = _constraints[addRotations(rotation, i)].Rotate(rotation);

            return new CellConstraintSet(rotatedSet);
        }

        private int addRotations(int rotationA, int rotationB)
        {
            return (rotationA + rotationB) % _constraints.Length;
        }
    }
}