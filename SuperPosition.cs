using System;

namespace WaveFunctionCollapse
{
    public struct SuperPosition
    {
        public int ModuleIndex;
        public SuperOrientation Orientations;
        public SuperPosition(int moduleIndex, SuperOrientation orientations)
        {            
            ModuleIndex = moduleIndex;
            Orientations = orientations;
        }


        // --- Basic Access --- //

        public bool Possible()
        {
            return Orientations.Valid();
        }


        // --- Operations --- //

        public SuperPosition Union(SuperPosition reference)
        {
            Orientations.Union(reference.Orientations);
            return this;
        }

        public SuperPosition Rotate(int rotation)
        {
            Orientations.Rotate(rotation);
            return this;
        }

        public SuperPosition Intersection(SuperPosition reference)
        {
            Orientations.Intersection(reference.Orientations);
            return this;
        }
    }
}