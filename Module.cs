using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WaveFunctionCollapse
{
    [Serializable]
    public abstract class Module<T> : ScriptableObject
        where T : Module<T>
    {
        public byte Index;

        // Input
        public FeatureFlagMask FeatureFlagMask;

        // Features
        public abstract string ID { get; }
        public abstract int Sides { get; }
        public byte[] Features;
        public byte[] FeaturesReflected;
        public int FeatureFlags;

        // Constraints
        public CellConstraintSet<T> Constraints { get => _constraints; } // Constraint adjacent cells
        public void UpdateConstraints(CellConstraintSet<T> constraints) => _constraints = constraints;
        [SerializeField] private CellConstraintSet<T> _constraints;

        
        public abstract byte[] Orientations { get; } // MAYBE PUT THIS SOMEWHERE ELSE... IT IS MORE PART OF A TILE SYSTEM CONCEPT THAN A SINGLE TILE // In general orientation would be a sequence of numbers, but just in case it can be defined as any aray of bytes
        public byte AddRotations(int rotationA, int rotationB) => (byte)((rotationA + rotationB) % Sides);


        public virtual void SetFeatures()
        {
            // Each Module is expected to provide a FeatureSized digit number as an ID.
            // Each digit stands for a type of feature on each of the Module's corners,
            // starting at the top going counter clockwise around the n-gon.
            // As the program focusses on the sides of a n-gon and not its corners, 
            // values are stored as pairs to represent the feature of a side of the n-gon.
            // THERE SHOULD BE AN OPTION TO SET MULTIPLE DIGITS TO DEFINE A FEAUTIRE OTHERWISE YOU'RE LEFT WITH 10 

            Clear();
            string meshName = ID;   
            Features = new byte[Sides]; 
            FeaturesReflected = new byte[Sides]; 
            FeatureFlags = 0;

            // Get features
            for (int i = 0; i < Features.Length; i ++)
            {
                int a = int.Parse(meshName.Substring( i,                        1));
                int b = int.Parse(meshName.Substring((i + 1) % Features.Length, 1));

                Features[i]                                          = (byte)(10 * a + b);
                FeaturesReflected[(i + Sides / 2) % Features.Length] = (byte)(10 * b + a);
            }

            // Set flags
            for (int i = 0; i < meshName.Length; i ++)
            {
                int flag = int.Parse(meshName.Substring(i, 1));
                flag = flag > 1 ? 1 << (flag - 1) : flag;
                FeatureFlags |= flag;
            }
        }

        public virtual void UpdateAll()
        {
            ModuleImporter<T>.UpdateModules(this);
        }

        public virtual void Clear()
        {
            Features = new byte[0];
            FeatureFlags = 0;
            _constraints = new CellConstraintSet<T>();
        }
    }

    [Serializable]
    public abstract class FeatureFlagMask
    {
        public abstract int Mask(int orienation);
    }
}