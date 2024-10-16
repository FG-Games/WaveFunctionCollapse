using System;
using UnityEngine;

namespace WaveFunctionCollapse
{
    public abstract class Module<T> : ScriptableObject
        where T : Module<T>
    {
        public byte Index;

        // Input
        public FeatureFlagMask FeatureFlagMask;

        // THERE SHOULD BE POSSIBILITY TO EXCLUDE "UNIQUE" TILES, 
        // LIKE PLAYER STARTS OR DECORATIVE / STORYTELLLING TILES

        // Features
        public abstract string ID { get; }
        public abstract int Sides { get; }
        public virtual byte[] Features { get => _features; }
        public virtual byte[] FeaturesReflected { get => _featuresReflected; }
        public int FeatureFlags;

        [SerializeField] protected byte[] _features;
        [SerializeField] protected byte[] _featuresReflected;

        // Constraints
        public CellConstraintSet<T> Constraints { get => new CellConstraintSet<T>(_constraints); } // Constraint adjacent cells
        public virtual void UpdateConstraints(CellConstraint<T>[] constraints) => _constraints = constraints;
        [SerializeField] private CellConstraint<T>[] _constraints; // MAKE THIS A CONSTRAINT SET!!!!!

        
        public abstract SuperOrientation Orientations { get; } // Bitmask // MAYBE MAKE THIS PART OF AN OVERALL HEX / QUAD / CUBE SETUP 
        public int AddRotations(int rotationA, int rotationB) => ((rotationA + rotationB) % Sides);


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
            _features = new byte[Sides]; 
            _featuresReflected = new byte[Sides]; 
            FeatureFlags = 0;

            // Get features
            for (int i = 0; i < _features.Length; i ++)
            {
                int a = int.Parse(meshName.Substring( i,                        1));
                int b = int.Parse(meshName.Substring((i + 1) % _features.Length, 1));

                _features[i]                                          = (byte)(10 * a + b);
                _featuresReflected[(i + Sides / 2) % Features.Length] = (byte)(10 * b + a);
            }

            // Set flags
            for (int i = 0; i < meshName.Length; i ++)
            {
                int flag = int.Parse(meshName.Substring(i, 1));
                flag = flag > 1 ? 1 << (flag - 1) : flag;
                FeatureFlags |= flag;
            }
        }

        public abstract void UpdateAll();

        public virtual void Clear()
        {
            _features = new byte[0];
            _constraints = new CellConstraint<T>[6];
            FeatureFlags = 0;
        }
    }

    [Serializable]
    public abstract class FeatureFlagMask
    {
        public abstract int Mask(int orienation);
    }
}