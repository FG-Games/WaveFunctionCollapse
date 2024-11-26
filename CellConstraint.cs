using System;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraint
    {
        public static void SetCellConstraintLength(int lenth) => s_CellConstraintLength = lenth;
        private static int s_CellConstraintLength;

        public SuperPosition[] SuperPositions => _superPositions;
        public int Count => getCount();
        public int Length => s_CellConstraintLength;
        public int Entropy => getEntropy();
        [SerializeField] private SuperPosition[] _superPositions;


        // --- Setup --- //

        public CellConstraint(SuperPosition[] superPositions)
        {
            _superPositions = superPositions;
        }

        private int getCount()
        {
            int count = 0;

            for (int i = 0; i < Length; i ++)
                if(this[i].Possible)
                    count ++;

            return count;
        }

        private int getEntropy()
        {
            int entropy = 0;

            for (int i = 0; i < Length; i ++)
                entropy += this[i].Orientations.Count;

            return entropy;
        }


        // --- Basic Access --- //

        public SuperPosition this[int moduleIndex] { get => _superPositions[moduleIndex]; set => _superPositions[moduleIndex] = value; }

        public SuperOrientation Orientations(int index) => GetPossiblePosition(index).Orientations;

        public SuperPosition GetPossiblePosition(int index)
        {
            int counter = -1;

            for (int i = 0; i < Length; i ++)
            {
                if(_superPositions[i].Possible)
                {
                    counter ++;

                    if(index == counter)
                        return _superPositions[i];
                }
            }

            throw new IndexOutOfRangeException($"Index {index} is out of range for the amount of possible positions {Count}.");
        }

        

        public void Dispose()
        {
            Debug.Log("NEVER Disposed SuperPositions");

            /*if (_superPositions.IsCreated)
                _superPositions.Dispose();
            if (_possiblePositions.IsCreated)
                _possiblePositions.Dispose();*/
        }

        private CellConstraint merge(CellConstraint constraint) // Merge Contraints 
        {
            SuperPosition[] mergedConstraint = new SuperPosition[Length];

            for (int i = 0; i < Length; i ++)
                mergedConstraint[i] = _superPositions[i].Union(constraint[i]);

            return new CellConstraint(mergedConstraint);
        }

        private CellConstraint rotate(int rotation) // Rotate or offset all orientations of SuperPositions 
        {
            SuperPosition[] offsetSuperPositions = new SuperPosition[Length];

            for (int i = 0; i < _superPositions.Length; i ++)
                offsetSuperPositions[i] = _superPositions[i].Rotate(rotation);

            return new CellConstraint(offsetSuperPositions);
        }
        

        // --- Operators --- //

        public static CellConstraint operator + (CellConstraint a, CellConstraint b) => a.merge(b);

        public static CellConstraint operator * (CellConstraint a, int i) => a.rotate(i);

        public static bool operator == (CellConstraint a, CellConstraint b) =>  a._superPositions.SequenceEqual(b._superPositions);

        public static bool operator != (CellConstraint a, CellConstraint b) => !a._superPositions.SequenceEqual(b._superPositions);

        public override bool Equals(object obj)
        {
            if (!(obj is CellConstraint))
                return false;

            var other = (CellConstraint)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                if (_superPositions != null)
                    for (int i = 1; i < _superPositions.Length; i ++)
                        hash = hash * 31 + _superPositions[i].GetHashCode();

                return hash;
            }
        }
    }

    [Serializable]
    public struct CellConstraintSet
    {
        public static void SetCellConstraintSetLength(int lenth) => s_CellConstraintSetLength = lenth;
        private static int s_CellConstraintSetLength;
        public int Length => s_CellConstraintSetLength;
        [SerializeField] private CellConstraint[] _constraints;

        public CellConstraintSet(CellConstraint[] cellConstraints) => _constraints = cellConstraints;

        public CellConstraint this[int index] { get => _constraints[index]; set => _constraints[index] = value; }

        private CellConstraintSet merge(CellConstraintSet addition)
        {
            CellConstraint[] addedSet = new CellConstraint[Length];

            for(int i = 0; i < Length; i++)
                addedSet[i] = _constraints[i] + addition[i];

            return new CellConstraintSet(addedSet);
        }

        private CellConstraintSet rotate(int rotation)
        {
            CellConstraint[] rotatedSet = new CellConstraint[Length];

            for (int i = 0; i < Length; i ++)
                rotatedSet[i] = _constraints[addRotations(rotation, i)] * rotation;

            return new CellConstraintSet(rotatedSet);
        }

        private byte addRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % Length);

        public static CellConstraintSet operator +(CellConstraintSet obj1, CellConstraintSet obj2) => obj1.merge(obj2);

        public static CellConstraintSet operator *(CellConstraintSet obj1, int i) => obj1.rotate(i);
    }
}