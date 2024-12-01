using System;

namespace WaveFunctionCollapse
{
    public struct CellConstraint
    {         
        private SuperPosition[] _superPositions;

        public CellConstraint(SuperPosition[] superPositions)
        {
            _superPositions = superPositions;
        }

        public CellConstraint Copy()
        {
            SuperPosition[] superPositionsCopy = new SuperPosition[_superPositions.Length];

            for (int i = 0; i < superPositionsCopy.Length; i ++)
                superPositionsCopy[i] = _superPositions[i];

            return new CellConstraint(superPositionsCopy);
        }


        // --- Basic Access --- //

        public int Length()
        {
            return _superPositions.Length;
        }

        public int Count()
        {
            int count = 0;

            for (int i = 0; i < _superPositions.Length; i ++)
                if(GetSuperPosition(i).Possible())
                    count ++;

            return count;
        }

        public int Entropy()
        {
            int entropy = 0;

            for (int i = 0; i < _superPositions.Length; i ++)
                entropy += GetSuperPosition(i).Orientations.Count();

            return entropy;
        }


        // --- Basic Access --- //

        public SuperPosition GetSuperPosition(int index)
        {
            return _superPositions[index];
        }

        public SuperPosition GetPossiblePosition(int possibleIndex)
        {
            int counter = -1;

            for (int i = 0; i < _superPositions.Length; i ++)
            {
                if(_superPositions[i].Possible())
                {
                    counter ++;

                    if(possibleIndex == counter)
                        return _superPositions[i];
                }
            }

            return default(SuperPosition);
        }

        public SuperOrientation Orientations(int possibleIndex)
        {
            return GetPossiblePosition(possibleIndex).Orientations;
        }


        // --- Operations --- //

        public void Add(CellConstraint constraint)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
                _superPositions[i].Union(constraint.GetSuperPosition(i));
        }

        public void Rotate(int rotation)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
                _superPositions[i].Rotate(rotation);
        }

        public void Intersection(CellConstraint constraint)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
               _superPositions[i].Intersection(constraint.GetSuperPosition(i));
        }
    }
}