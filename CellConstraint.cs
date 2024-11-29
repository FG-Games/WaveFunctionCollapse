using System;
using Unity.Collections;

namespace WaveFunctionCollapse
{
    public struct CellConstraint : IDisposable
    {         
        private NativeArray<SuperPosition> _superPositions;

        public CellConstraint(NativeArray<SuperPosition> superPositions)
        {
            _superPositions = superPositions;
        }

        public CellConstraint Copy()
        {
            NativeArray<SuperPosition> superPositionsCopy = new NativeArray<SuperPosition>(_superPositions.Length, Allocator.TempJob);

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

        public void Dispose()
        {
            if(_superPositions.IsCreated)
                _superPositions.Dispose();
        }


        // --- Operations --- //

        public void Add(CellConstraint constraint)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
                _superPositions[i] = _superPositions[i].Union(constraint.GetSuperPosition(i));
        }

        public void Rotate(int rotation)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
                _superPositions[i] = _superPositions[i].Rotate(rotation);
        }

        public void Intersection(CellConstraint constraint)
        {
            for (int i = 0; i < _superPositions.Length; i ++)
               _superPositions[i] = _superPositions[i].Intersection(constraint.GetSuperPosition(i));
        }
    }
}