using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperOrientation
    {
        [SerializeField] private int _orientationBitmask;

        public SuperOrientation(int bitmask) => _orientationBitmask = bitmask;
        public int Bitmask { get => _orientationBitmask; }

        public int this[int index] { get => getOrientation(index); }
        public int First { get => (int)Math.Log(_orientationBitmask & -_orientationBitmask, 2); } 
        public int Count { get => count; }
        public bool Valid { get => _orientationBitmask > 0; }


        public void SetOrientation (int index) => _orientationBitmask |= (1 << index);
        public SuperOrientation Union (SuperOrientation superOrientation) => new SuperOrientation(_orientationBitmask | superOrientation.Bitmask);
        public SuperOrientation Intersection (SuperOrientation superOrientation) => new SuperOrientation(_orientationBitmask & superOrientation.Bitmask);
        public SuperOrientation Rotate (int rotation) => new SuperOrientation((_orientationBitmask << rotation) | (_orientationBitmask >> (6 - rotation)) & 0x3F);

        private int count
        {
            get
            {
                int orientations = _orientationBitmask;
                int count = 0;

                while (orientations > 0)
                {
                    orientations &= (orientations - 1); // Clear the lowest set bit
                    count++;
                }

                return count;
            }
        }

        private int getOrientation(int index)
        {
            int orientations = _orientationBitmask;
            int orientation = 0;
            int count = 0;

            while (orientations > 0)
            {
                orientation = (int)Math.Log(orientations & -orientations, 2); // Get lowest set orientation

                if (count == index)
                    return (int)orientation;

                orientations &= (orientations - 1); // Clear the lowest set bit
                count ++;
            }

            throw new ArgumentOutOfRangeException(nameof(index), $"Index is {index}, but must be between 0 and {Count}. ");
        }


        // --- Operators --- //

        public static bool operator == (SuperOrientation a, SuperOrientation b) => a._orientationBitmask == b._orientationBitmask;
        public static bool operator != (SuperOrientation a, SuperOrientation b) => a._orientationBitmask != b._orientationBitmask;
        public override bool Equals(object obj)
        {
            if (!(obj is SuperOrientation))
                return false;

            var other = (SuperOrientation)obj;
            return this == other;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _orientationBitmask.GetHashCode();
                return hash;
            }
        }
    }
}