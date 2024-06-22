using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct CellConstraint<T> // reduced the entropy of a CellSuperPosition / limits the possibible options of a cell  
        where T : Module<T>
    {
        public SuperPosition<T>[] SuperPositions; // limited range of permitted modules and their possible orientations

        public CellConstraint(SuperPosition<T>[] superPositions) => SuperPositions = superPositions;

        public bool Intersection(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;

            for (int i = 0; i < SuperPositions.Length; i ++)
                if(SuperPositions[i].Intersection(reference, out intersection))
                    return true;

            return false;
        }

        private CellConstraint<T> merge(CellConstraint<T> constraint) // Merge Contraints 
        {
            List<SuperPosition<T>> combinedSuperPositions = SuperPositions.ToList();
            SuperPosition<T> merdedPosition = new SuperPosition<T>();

            for (int i = 0; i < constraint.SuperPositions.Length; i ++)
            {
                bool containedInConstraint = false;
                merdedPosition = new SuperPosition<T>();

                for (int j = 0; j < SuperPositions.Length; j ++)
                {
                    if(combinedSuperPositions[j].Union(constraint.SuperPositions[i], out merdedPosition))
                    {
                        combinedSuperPositions[j] = merdedPosition;
                        containedInConstraint = true;
                        break;
                    }
                }

                if(!containedInConstraint)
                    combinedSuperPositions.Add(constraint.SuperPositions[i]);
            }

            return new CellConstraint<T>(combinedSuperPositions.ToArray());
        }

        private CellConstraint<T> rotate(int rotation) // Rotate or offset all orientations of SuperPositions 
        {
            SuperPosition<T>[] offsetSuperPositions = new SuperPosition<T>[SuperPositions.Length];

            for (int i = 0; i < SuperPositions.Length; i ++)
                offsetSuperPositions[i] = SuperPositions[i].Rotate(rotation);

            return new CellConstraint<T>(offsetSuperPositions);
        }
        

        // --- Operators --- //

        public static CellConstraint<T> operator + (CellConstraint<T> a, CellConstraint<T> b) => a.merge(b);

        public static CellConstraint<T> operator * (CellConstraint<T> a, int i) => a.rotate(i);

        public static bool operator == (CellConstraint<T> a, CellConstraint<T> b) =>  a.SuperPositions.SequenceEqual(b.SuperPositions);

        public static bool operator != (CellConstraint<T> a, CellConstraint<T> b) => !a.SuperPositions.SequenceEqual(b.SuperPositions);

        public override bool Equals(object obj)
        {
            if (!(obj is CellConstraint<T>))
                return false;

            var other = (CellConstraint<T>)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                if (SuperPositions != null)
                    for (int i = 1; i < SuperPositions.Length; i ++)
                        hash = hash * 31 + SuperPositions[i].GetHashCode();

                return hash;
            }
        }
    }

    [Serializable]
    public struct CellConstraintSet<T> // A set of constraints: one for each adjacent cell // HEX ONLY
        where T : Module<T>
    {
        public const int Length = 6;

        [SerializeField] private CellConstraint<T> _constraint0, _constraint1, _constraint2, _constraint3, _constraint4, _constraint5;

        public CellConstraintSet(CellConstraint<T>[] cellConstraints)
        {
            _constraint0 = cellConstraints[0];
            _constraint1 = cellConstraints[1];
            _constraint2 = cellConstraints[2];
            _constraint3 = cellConstraints[3];
            _constraint4 = cellConstraints[4];
            _constraint5 = cellConstraints[5];
        }

        public CellConstraint<T> this[int index]
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

        private CellConstraintSet<T> merge(CellConstraintSet<T> additionalSet)
        {
            CellConstraintSet<T> addedSet = this;

            // Combine constraints
            for(int i = 1; i < Length; i++)
                addedSet[i] += additionalSet[i];

            return additionalSet;
        }

        private CellConstraintSet<T> rotate(int rotation)
        {
            CellConstraintSet<T> rotatedSet = this;

            for (int i = 0; i < Length; i ++)
                rotatedSet[i] = this[addRotations(rotation, i)] * rotation;

            return rotatedSet;
        }

        private byte addRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % Length);

        public static CellConstraintSet<T> operator +(CellConstraintSet<T> obj1, CellConstraintSet<T> obj2) => obj1.merge(obj2);
        public static CellConstraintSet<T> operator *(CellConstraintSet<T> obj1, int i) => obj1.rotate(i);
    }
}