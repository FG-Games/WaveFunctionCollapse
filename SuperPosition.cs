using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition
    {
        public SuperOrientation Orientations; // Bitmask
        public int ModuleIndex;

        public SuperPosition(SuperOrientation orientations, int moduleIndex)
        {
            Orientations = orientations;
            ModuleIndex = moduleIndex;
        }


        // --- Orientations --- //

        public bool Union(SuperPosition reference, out SuperPosition union)
        {
            union = reference;
            
            if(reference.ModuleIndex != ModuleIndex)
                return false;

            union = new SuperPosition(Orientations.Union(reference.Orientations), ModuleIndex);
            return true;
        }

        public bool Intersection(SuperPosition reference, out SuperPosition intersection)
        {
            intersection = reference;
            
            if(reference.ModuleIndex != ModuleIndex)
                return false;

            intersection = new SuperPosition(Orientations.Intersection(reference.Orientations), ModuleIndex);
            return intersection.Orientations.Bitmask > 0;
        }

        public SuperPosition Rotate(int rotation)
        {
            return new SuperPosition (Orientations.Rotate(rotation), ModuleIndex);
        }


        // --- Operators --- //

        public static bool operator == (SuperPosition a, SuperPosition b)
        {
            return
            a.Orientations == b.Orientations &&
            a.ModuleIndex == b.ModuleIndex;
        }

        public static bool operator != (SuperPosition a, SuperPosition b)
        {
            return
            a.Orientations != b.Orientations ||
            a.ModuleIndex != b.ModuleIndex;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SuperPosition))
                return false;

            var other = (SuperPosition)obj;
            return this == other;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Orientations.GetHashCode();
                hash = hash * 31 + ModuleIndex.GetHashCode();
                return hash;
            }
        }
    }
}