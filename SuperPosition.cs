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
            return new SuperPosition(ModuleIndex, Orientations.Union(reference.Orientations));
        }

        public SuperPosition Intersection(SuperPosition reference)
        {
            return new SuperPosition(ModuleIndex, Orientations.Intersection(reference.Orientations));
        }

        public SuperPosition Rotate(int rotation)
        {
            return new SuperPosition (ModuleIndex, Orientations.Rotate(rotation));
        }
    }
}