using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEditor;

namespace WaveFunctionCollapse
{
    [Serializable]
    public struct SuperPosition<T>
        where T : Module<T>
    {
        public SuperOrientation Orientations; // Bitmask
        public int ModuleIndex;
        private CellConstraintSet<T> _constraints;

        public SuperPosition(SuperOrientation orientations, CellConstraintSet<T> constraints, int moduleIndex)
        {
            Orientations = orientations;            
            ModuleIndex = moduleIndex;
            _constraints = constraints;
        }

        public SuperPosition(SuperOrientation orientations, SuperPosition<T> superPosition)
        {
            Orientations = orientations;            
            ModuleIndex = superPosition.ModuleIndex;
            _constraints = superPosition._constraints;
        }

        public SuperPosition(SuperOrientation orientations, T Module)
        {
            Orientations = orientations;            
            ModuleIndex = Module.Index;
            _constraints = Module.Constraints;
        }


        // --- Orientations --- //

        public bool Union(SuperPosition<T> reference, out SuperPosition<T> union)
        {
            union = reference;
            
            if(reference.ModuleIndex != ModuleIndex)
                return false;

            union = new SuperPosition<T>(Orientations.Union(reference.Orientations), _constraints, ModuleIndex);
            return true;
        }

        public bool Intersection(SuperPosition<T> reference, out SuperPosition<T> intersection)
        {
            intersection = reference;
            
            if(reference.ModuleIndex != ModuleIndex)
                return false;

            intersection = new SuperPosition<T>(Orientations.Intersection(reference.Orientations), _constraints, ModuleIndex);
            return intersection.Orientations.Bitmask > 0;
        }

        public SuperPosition<T> Rotate(int rotation)
        {
            return new SuperPosition<T> (Orientations.Rotate(rotation), _constraints, ModuleIndex);
        }


        // --- Constraints --- //

        public CellConstraintSet<T> RotatedContraints(int i) => _constraints * Orientations[i];

        public CellConstraintSet<T> SuperConstraints
        {
            get
            {
                // Combine module constraints of all possible orientations
                CellConstraintSet<T> superConstraints = RotatedContraints(0);

                for (int i = 1; i < Orientations.Count; i ++)
                    superConstraints += RotatedContraints(i);

                return superConstraints;
            }
        }


        // --- Operators --- //

        public static bool operator == (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            a.Orientations == b.Orientations &&
            a.ModuleIndex == b.ModuleIndex;
        }

        public static bool operator != (SuperPosition<T> a, SuperPosition<T> b)
        {
            return
            a.Orientations != b.Orientations ||
            a.ModuleIndex != b.ModuleIndex;
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
                hash = hash * 31 + (ModuleIndex != null ? ModuleIndex.GetHashCode() : 0);
                return hash;
            }
        }
    }

    [Serializable]
    public struct SuperPositions<T> : IDisposable
        where T : Module<T>
    {
        public int Count => _count;

        [SerializeField] private int _count;
        [SerializeField] private NativeArray<SuperPosition<T>> _superPositions;
        [SerializeField] private NativeArray<bool> _possiblePositions;

        public SuperPositions(ModuleSet<T> moduleSet)
        {
            _count = moduleSet.Modules.Length;
            _superPositions = new NativeArray<SuperPosition<T>>(_count, Allocator.Persistent);
            _possiblePositions = new NativeArray<bool>(_count, Allocator.Persistent);

            for (int i = 0; i < _count; i++)
            {
                _superPositions[i] = moduleSet.SuperPosition(i);
                _possiblePositions[i] = true;
            }
        }

        public void Remove(int index)
        {
            if (index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");

            _count--;
            _possiblePositions[index] = false;
        }

        public SuperPosition<T> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException("Index is out of range.");

                int validIndex = -1;

                for (int i = 0; i < _possiblePositions.Length; i++)
                {
                    if (_possiblePositions[i])
                    {
                        validIndex++;
                        if (validIndex == index)
                            return _superPositions[i];
                    }
                }

                throw new InvalidOperationException("No valid SuperPosition at the specified index.");
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException("Index is out of range.");

                int validIndex = -1;

                for (int i = 0; i < _possiblePositions.Length; i++)
                {
                    if (_possiblePositions[i])
                    {
                        validIndex++;
                        if (validIndex == index)
                        {
                            _superPositions[i] = value; // Set the new value
                            return;
                        }
                    }
                }

                throw new InvalidOperationException("No valid SuperPosition at the specified index to set.");
            }
        }

        public void Dispose()
        {
            if (_superPositions.IsCreated)
                _superPositions.Dispose();
            if (_possiblePositions.IsCreated)
                _possiblePositions.Dispose();
        }
    }
}