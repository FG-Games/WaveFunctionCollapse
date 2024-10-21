using System;

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

        public bool Union(SuperPosition other)
        {            
            if(ModuleIndex != other.ModuleIndex)
                return false;

            Orientations = Orientations.Union(other.Orientations);
            return true;
        }

        public bool Intersection(SuperPosition other)
        {            
            if(ModuleIndex != other.ModuleIndex)
                return false;

            Orientations = Orientations.Intersection(other.Orientations);
            return Orientations.Bitmask > 0;
        }

        public void Rotate(int rotation)
        {
            Orientations.Rotate(rotation);
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