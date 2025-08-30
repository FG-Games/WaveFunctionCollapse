/// <summary>
/// A SuperOrientation represents a set of orientations. 
/// Since any 2D or 3D tessellation has at most six possible orientations,
/// and SuperOrientation operations are limited to boolean logic, orientations are stored as bitmasks.
/// It is recommended to provide a mapping from integers to actual rotation representations,
/// such as Quaternions, to interpret the encoded orientations.
/// </summary>

using System;

namespace WaveFunctionCollapse
{
    public struct SuperOrientation
    {
        public int Bitmask;

        public SuperOrientation(int bitmask)
        {
            Bitmask = bitmask;
        }


        // --- Basic Access --- //
        
        public int First() // Returns the index of the first set bit (trailing zero count)
        {
            if (Bitmask == 0) return -1; // No bits set, return sentinel value
                return CountTrailingZeros(Bitmask);
        }

        public bool Valid()
        {
            return Bitmask > 0;
        }

        public int Count()
        {
            int orientations = Bitmask;
            int count = 0;

            while (orientations > 0)
            {
                orientations &= (orientations - 1); // Clear the lowest set bit
                count++;
            }

            return count;
        }

        public int GetOrientation(int index)
        {
            int orientations = Bitmask;
            int count = 0;

            while (orientations > 0)
            {
                // Get the position of the lowest set bit

                int orientation = CountTrailingZeros(orientations);

                if (count == index)
                    return orientation;

                orientations &= (orientations - 1); // Clear the lowest set bit
                count++;
            }

            return -1; // Index out of range
        }

        private static int CountTrailingZeros(int value) // Helper method to count trailing zeros using bitwise operations
        {
            int count = 0;
            while ((value & 1) == 0)
            {
                value >>= 1;
                count++;
            }
            return count;
        }


        // --- Operations --- //

        public void Union(SuperOrientation superOrientation)
        {
            Bitmask |= superOrientation.Bitmask;
        }

        public void Intersection(SuperOrientation superOrientation)
        {
            Bitmask &= superOrientation.Bitmask;
        }

        public void Rotate(int rotation)
        {
            Bitmask = ((Bitmask << rotation) | (Bitmask >> (6 - rotation))) & 0x3F;
        }
    }
}