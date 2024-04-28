using System;
using System.Linq;
using System.Collections.Generic;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition<T>
        where T : Module<T>
    {
        public byte[] Orientations;
        public T Module;

        public SuperPosition(byte[] orientations, T module)
        {
            Orientations = orientations;
            Module = module;
        }

        public SuperPosition(T module)
        {
            Orientations = module.Orientations;
            Module = module;
        }

        public bool Union(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;
            
            if(reference.Module != Module)
                return false;

            HashSet<byte> unionOrientations = Orientations.ToHashSet();
            unionOrientations.UnionWith(reference.Orientations.ToHashSet());

            intersection = new SuperPosition<T>(unionOrientations.ToArray(), Module);
            return true;
        }

        public bool Intersection(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;
            
            if(reference.Module != Module)
                return false;

            byte[] intersectingOrientations = Orientations.Intersect(reference.Orientations).ToArray();

            if (intersectingOrientations.Length == 0)
                return false;
                
            intersection = new SuperPosition<T>(intersectingOrientations, Module);
            return true;
        }

        public SuperPosition<T> Rotate(int rotation)
        {            
            byte[] orientations = new byte[Orientations.Length];

            // Add rotation offset to orientations
            for (int i = 0; i < Orientations.Length; i ++)
                orientations[i] = Module.AddRotations(rotation, Orientations[i]);

            return new SuperPosition<T> (orientations, Module);
        }


        // --- Constraints --- //

        public CellConstraintSet<T> RotatedContraints(int i) => Module.Constraints * Orientations[i];

        public CellConstraintSet<T> SuperConstraints
        {
            get
            {
                // Combine module constraints of all possible orientations
                CellConstraintSet<T> superConstraints = RotatedContraints(0);

                for (int i = 1; i < Orientations.Length; i ++)
                    superConstraints += RotatedContraints(i);

                return superConstraints;
            }
        }


        // --- Operators --- //

        public static bool operator == (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            a.Orientations.SequenceEqual(b.Orientations) &&
            a.Module == b.Module;
        }

        public static bool operator != (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            !a.Orientations.SequenceEqual(b.Orientations) ||
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

                if (Orientations != null)
                    for (int i = 1; i < Orientations.Length; i ++)
                        hash = hash * 31 + Orientations[i].GetHashCode();

                hash = hash * 31 + (Module != null ? Module.GetHashCode() : 0);
                return hash;
            }
        }
    }
}