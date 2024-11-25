using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraint
    {
        public int Count => _count;
        public int Entropy => _entropy;
        [SerializeField] private SuperPosition[] _superPositions;
        [SerializeField] private int _count, _entropy;
        private int length => _superPositions.Length;


        // --- Setup --- //

        public CellConstraint(SuperPosition[] superPositions)
        {
            _superPositions = superPositions;
            _count = _entropy = 0;
            setCount();
            setEntropy();
        }

        private void setCount()
        {
            _count = 0;

            for (int i = 0; i < length; i ++)
                if(this[i].Possible)
                    _count ++;
        }

        private void setEntropy()
        {
            _entropy = 0;

            for (int i = 0; i < length; i ++)
                _entropy += this[i].Orientations.Count;
        }


        // --- Basic Access --- //

        public SuperPosition this[int moduleIndex]
        {
            get
            {
                if (moduleIndex < 0 || moduleIndex >= length)
                    throw new IndexOutOfRangeException($"Index {moduleIndex} is out of range for the length {length}.");

                return _superPositions[moduleIndex];                    
            }
            set
            {
                if (moduleIndex < 0 || moduleIndex >= length)
                    throw new IndexOutOfRangeException($"Index {moduleIndex} is out of range for the length {length}.");

                _superPositions[moduleIndex] = value;
            }
        }

        public SuperPosition GetPossiblePosition(int index)
        {
            if (index < 0 || index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range for the amount of possible positions {length}.");

            int counter = -1;

            for (int i = 0; i < length; i ++)
            {
                if(_superPositions[i].Possible)
                {
                    counter ++;

                    if(index == counter)
                        return _superPositions[i];
                }
            }

            throw new IndexOutOfRangeException($"Index {index} is out of range for the amount of possible positions {length}.");
        }

        public void Dispose()
        {
            Debug.Log("NEVER Disposed SuperPositions");

            /*if (_superPositions.IsCreated)
                _superPositions.Dispose();
            if (_possiblePositions.IsCreated)
                _possiblePositions.Dispose();*/
        }

        

        public bool Intersection(SuperPosition reference, out SuperPosition intersection)
        {
            intersection = reference;

            for (int i = 0; i < _superPositions.Length; i ++)
                if(_superPositions[i].Intersection(reference, out intersection))
                    return true;

            return false;
        }

        private CellConstraint merge(CellConstraint constraint) // Merge Contraints 
        {
            SuperPosition[] mergedConstraint = new SuperPosition[length];

            for (int i = 0; i < length; i ++)
                mergedConstraint[i] = _superPositions[i].Union(constraint[i]);

            return new CellConstraint(mergedConstraint);
        }

        /*private CellConstraint merge(CellConstraint constraint) // Merge Contraints 
        {
            List<SuperPosition> combinedSuperPositions = _superPositions.ToList();
            SuperPosition merdedPosition = new SuperPosition();

            for (int i = 0; i < constraint._superPositions.Length; i ++)
            {
                bool containedInConstraint = false;
                merdedPosition = new SuperPosition();

                for (int j = 0; j < _superPositions.Length; j ++)
                {
                    if(combinedSuperPositions[j].Union(constraint._superPositions[i], out merdedPosition))
                    {
                        combinedSuperPositions[j] = merdedPosition;
                        containedInConstraint = true;
                        break;
                    }
                }

                if(!containedInConstraint)
                    combinedSuperPositions.Add(constraint._superPositions[i]);
            }

            return new CellConstraint(combinedSuperPositions.ToArray());
        }*/

        private CellConstraint rotate(int rotation) // Rotate or offset all orientations of SuperPositions 
        {
            SuperPosition[] offsetSuperPositions = new SuperPosition[_superPositions.Length];

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
    public struct CellConstraintSet // A set of constraints: one for each adjacent cell // HEX ONLY BUT YOU COULD JUST MAKE IT A NATIVE ARRAY...
    {
        public const int Length = 6;

        [SerializeField] private CellConstraint _constraint0, _constraint1, _constraint2, _constraint3, _constraint4, _constraint5;

        public CellConstraintSet(CellConstraint[] cellConstraints)
        {
            _constraint0 = cellConstraints[0];
            _constraint1 = cellConstraints[1];
            _constraint2 = cellConstraints[2];
            _constraint3 = cellConstraints[3];
            _constraint4 = cellConstraints[4];
            _constraint5 = cellConstraints[5];
        }

        public CellConstraint this[int index]
        {
            get
            {
                return index switch
                {
                    0 => _constraint0, 
                    1 => _constraint1,
                    2 => _constraint2,
                    3 => _constraint3,
                    4 => _constraint4,
                    5 => _constraint5,
                    _ => throw new IndexOutOfRangeException("Index out of range")
                };
            }
            set
            {
                switch (index)
                {
                    case 0: _constraint0 = value; break;
                    case 1: _constraint1 = value; break;
                    case 2: _constraint2 = value; break;
                    case 3: _constraint3 = value; break;
                    case 4: _constraint4 = value; break;
                    case 5: _constraint5 = value; break;
                    default: throw new IndexOutOfRangeException("Index out of range");
                }
            }
        }

        private CellConstraintSet merge(CellConstraintSet addition)
        {
            CellConstraintSet addedSet = this;

            for(int i = 0; i < Length; i++)
                addedSet[i] += addition[i];

            return addedSet;
        }

        private CellConstraintSet rotate(int rotation)
        {
            CellConstraintSet rotatedSet = this;

            for (int i = 0; i < Length; i ++)
                rotatedSet[i] = this[addRotations(rotation, i)] * rotation;

            return rotatedSet;
        }

        private byte addRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % Length);

        public static CellConstraintSet operator +(CellConstraintSet obj1, CellConstraintSet obj2) => obj1.merge(obj2);

        public static CellConstraintSet operator *(CellConstraintSet obj1, int i) => obj1.rotate(i);
    }
}