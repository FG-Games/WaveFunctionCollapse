using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition<T>
        where T : Module<T>
    {
        public int Orientations; // Bitmask
        public T Module;

        public SuperPosition(int orientations, T module)
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

        public int GetFirstOrientation { get => (int)Math.Log(Orientations & -Orientations, 2); } // Get smallest rotation / the position of the lowest set bit

        public int OrientationCount
        {
            get
            {
                int orientations = Orientations;
                int count = 0;                

                while (orientations > 0)
                {
                    orientations &= (orientations - 1); // Clear the lowest set bit
                    count++;
                }
                return count;
            }
        }

        public int GetOrientation(int index)
        {
            int orientations = Orientations;
            int orientation = 0;
            int count = 0;

            while (orientations > 0)
            {
                orientation = (int)Math.Log(orientations & -orientations, 2);

                if (count == index)
                    return (int)orientation;

                orientations &= (orientations - 1);
                count ++;
            }

            throw new ArgumentOutOfRangeException(nameof(index), $"Index is {index}, but must be between 0 and {OrientationCount}. Orientations is {Orientations} / {Convert.ToString(Orientations, 2)}");
        }

        public bool Union(SuperPosition<T> reference, out SuperPosition<T> union)
        {
            union = reference;
            
            if(reference.Module != Module)
                return false;

            int unionOrientations = Orientations | reference.Orientations;
            union = new SuperPosition<T>(Orientations | reference.Orientations, Module);
            return true;
        }

        public bool Intersection(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;
            
            if(reference.Module != Module)
                return false;

            int intersectingOrientations = Orientations & reference.Orientations;                
            intersection = new SuperPosition<T>(intersectingOrientations, Module);
            return intersectingOrientations > 0;
        }

        public SuperPosition<T> Rotate(int rotation)
        {
            int rotatedOrientations = ((Orientations << rotation) | (Orientations >> (6 - rotation))) & 0x3F;
            return new SuperPosition<T> (rotatedOrientations, Module);
        }


        // --- Constraints --- //

        public CellConstraintSet<T> FirstOrientedContraints { get => Module.Constraints * GetFirstOrientation; }

        public CellConstraintSet<T> SuperConstraints
        {
            get
            {
                // Combine module constraints of all possible orientations                
                CellConstraintSet<T> superConstraints = FirstOrientedContraints;

                int rotation = 0;
                int orientations = Orientations;
                orientations &= (orientations - 1);

                while (orientations > 0)
                {
                    // Get smallest rotation / the position of the lowest set bit
                    rotation = (int)Math.Log(orientations & -orientations, 2);
                    superConstraints += Module.Constraints * rotation;

                    // Clear the lowest set bit
                    orientations &= (orientations - 1);
                }
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