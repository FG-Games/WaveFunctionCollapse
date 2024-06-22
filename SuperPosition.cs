using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition<T>
        where T : Module<T>
    {
        public SuperOrientation Orientations; // Bitmask
        public T Module;

        public SuperPosition(SuperOrientation orientations, T module)
        {
            Orientations = orientations;
            Module = module;
        }

        public SuperPosition(T module)
        {
            Orientations = module.Orientations;
            Module = module;
        }


        // --- Orientations --- //

        public bool Union(SuperPosition<T> reference, out SuperPosition<T> union)
        {
            union = reference;
            
            if(reference.Module != Module)
                return false;

            union = new SuperPosition<T>(Orientations.Union(reference.Orientations), Module);
            return true;
        }

        public bool Intersection(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;
            
            if(reference.Module != Module)
                return false;

            intersection = new SuperPosition<T>(Orientations.Intersection(reference.Orientations), Module);
            return intersection.Orientations.Bitmask > 0;
        }

        public SuperPosition<T> Rotate(int rotation)
        {
            return new SuperPosition<T> (Orientations.Rotate(rotation), Module);
        }


        // --- Constraints --- //

        public CellConstraintSet<T> RotatedContraints(int i) => Module.Constraints * Orientations[i];

        public CellConstraintSet<T> SuperConstraints
        {
            get
            {
                // Combine module constraints of all possible orientations
                CellConstraintSet<T> superConstraints = RotatedContraints(0);

                for (int i = 1; i < Orientations.Count; i ++)
                    superConstraints += RotatedContraints(i);

                return superConstraints;
            }
        }


        // --- Operators --- //

        public static bool operator == (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            a.Orientations == b.Orientations &&
            a.Module == b.Module;
        }

        public static bool operator != (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            a.Orientations != b.Orientations ||
            a.Module != b.Module;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SuperPosition<T>))
                return false;

            var other = (SuperPosition<T>)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Orientations.GetHashCode();
                hash = hash * 31 + (Module != null ? Module.GetHashCode() : 0);
                return hash;
            }
        }
    }
}