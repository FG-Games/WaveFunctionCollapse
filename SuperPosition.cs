using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition
    {
        public SuperOrientation Orientations; // Bitmask to store all possible orientations
        public int ModuleIndex;
        public bool Possible => Orientations.Valid;

        public SuperPosition(SuperOrientation orientations, int moduleIndex)
        {
            Orientations = orientations;
            ModuleIndex = moduleIndex;
        }


        // There are no safe guards for mismatching module indices!

        public SuperPosition Union(SuperPosition reference) => new SuperPosition(Orientations.Union(reference.Orientations), ModuleIndex);
        public SuperPosition Intersection(SuperPosition reference) => new SuperPosition(Orientations.Intersection(reference.Orientations), ModuleIndex);
        public SuperPosition Rotate(int rotation) => new SuperPosition (Orientations.Rotate(rotation), ModuleIndex);


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