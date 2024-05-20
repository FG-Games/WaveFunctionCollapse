using System;
using System.Collections;
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

            for (int i = 0; i < constraint.SuperPositions.Length; i ++)
            {
                bool containedInConstraint = false;
                SuperPosition<T> merdedPosition = new SuperPosition<T>();

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
    public struct CellConstraintSet<T> // A set of constraints: one for each adjacent cell
        where T : Module<T>
    {
        public CellConstraint<T>[] Set { get => _set; }
        [SerializeField] private CellConstraint<T>[] _set;

        public static CellConstraintSet<T> operator +(CellConstraintSet<T> obj1, CellConstraintSet<T> obj2) => obj1.merge(obj2);
        public static CellConstraintSet<T> operator *(CellConstraintSet<T> obj1, int i) => obj1.rotate(i);

        public CellConstraintSet(CellConstraint<T>[] cellConstraints)
        {
            if( cellConstraints.Length == 0 ||
                cellConstraints.Length != cellConstraints[0].SuperPositions[0].Module.Sides) // HOLY CRAP THAT'S HACKY ... 
            {
                Debug.LogError("CellConstraintSet must be of size " + cellConstraints[0].SuperPositions[0].Module.Sides);
            }

            _set = cellConstraints;
        }

        private CellConstraintSet<T> merge(CellConstraintSet<T> additionalSet)
        {
            // Combine constraints
            CellConstraint<T>[] set = _set.ToArray();

            for (int i = 0; i < set.Length; i ++)
                set[i] += additionalSet.Set[i];

            return new CellConstraintSet<T>(set);
        }

        private CellConstraintSet<T> rotate(int rotation)
        {
            CellConstraint<T>[] set = _set.ToArray();

            for (int i = 0; i < set.Length; i ++)
                set[i] = _set[addRotations(rotation, i)] * rotation;

            return new CellConstraintSet<T>(set);
        }

        private byte addRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % _set.Length); // HOLY CRAP THAT'S HACKY
    }
}