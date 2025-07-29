/// <summary>
/// A Superposition contains a reference to a Module and a set of its possible rotations. 
/// Superpositions are used in CellConstraints, and consequently in CSPFields and Modules.
/// In a CSPField, a Superposition represents a potential Module and its possible orientations.
/// In a Module, it defines a possible neighboring Module along with its allowed orientations.
/// </summary>

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

        public void Union(SuperPosition reference)
        {
            Orientations.Union(reference.Orientations);
        }

        public void Rotate(int rotation)
        {
            Orientations.Rotate(rotation);
        }

        public void Intersection(SuperPosition reference)
        {
            Orientations.Intersection(reference.Orientations);
        }
    }
}