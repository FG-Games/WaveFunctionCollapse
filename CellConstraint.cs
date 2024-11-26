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
        public SuperPosition[] SuperPositions => _superPositions;
        public int Count => getCount();
        public int Length => _superPositions.Length;
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
                                                                                        // ACTUALLY CellConstraint SuperPositions AND CellConstraintSet HAVE THE SAME LENGTH THROUGH OUT THE 
                                                                                        // MAP GENERATION THINK ABOUT SOLVING THE ISSUE IN ONE WAY ... LIKE DEFINING THEM INI`TIALLY AND REFERENCING A CENTRAL LENGTH ALWAYS 
    {
        public int Length => 6;

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