using System;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition
    {
        public int ModuleIndex;
        public SuperOrientation Orientations; // Bitmask to store all possible orientations
        public bool Possible => Orientations.Valid;

        public SuperPosition(int moduleIndex, SuperOrientation orientations)
        {            
            ModuleIndex = moduleIndex;
            Orientations = orientations;
        }


        // There are no safe guards for mismatching module indices!

        public SuperPosition Union(SuperPosition reference) => new SuperPosition(ModuleIndex, Orientations.Union(reference.Orientations));
        public SuperPosition Intersection(SuperPosition reference) => new SuperPosition(ModuleIndex, Orientations.Intersection(reference.Orientations));
        public SuperPosition Rotate(int rotation) => new SuperPosition (ModuleIndex, Orientations.Rotate(rotation));


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