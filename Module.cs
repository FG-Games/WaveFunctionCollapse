/// <summary>
/// A Module represents a tileable unit in the Wave Function Collapse system, 
/// defining possible adjacent units and orientations.
/// 
/// The Module ScriptableObject is intended to store visual data, metadata, and logic that define your tile pattern.
/// In the context of Wave Function Collapse, each Module provides a CellConstraintSet,
/// which is necessary for initializing the collapse process in a CSPField.
/// 
/// A Module's CellConstraints are generated from Features, identified by string-based IDs.
/// The current setup encodes each corner of a Module with one digit, 
/// meaning each side is described by a pair of digits (e.g., 4 digits for a cube).
/// The resulting Features, which represent side characteristics, are stored as bytes.
/// 
/// The ModuleImporter handles side compatibility testing between Modules,
/// generates the corresponding CellConstraints, and assigns the resulting ConstraintSets 
/// to their respective Modules.
/// 
/// Serializable versions of CellConstraint, Superposition, and SuperOrientation
/// (used for storing data within the Module ScriptableObject)
/// can be found at the bottom of this script.
/// </summary>


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

        // Features
        public abstract string ID { get; }
        public abstract int Sides { get; }
        public virtual byte[] Features { get => _features; }
        public virtual byte[] FeaturesReflected { get => _featuresReflected; }
        public int FeatureFlags { get => _featureFlags; }

        [SerializeField] protected byte[] _features;
        [SerializeField] protected byte[] _featuresReflected;
        [SerializeField] protected int _featureFlags;
        

        // Constraints
        public CellConstraintSet Constraints
        { 
            get
            {
                CellConstraint[] constraints = new CellConstraint[_constraints.Length];

                for(int i = 0; i < constraints.Length; i++)
                    constraints[i] = _constraints[i].GetCellConstraint();

                return new CellConstraintSet(constraints);
            }
        }
        
        public virtual void UpdateConstraints(SuperModuleArray<T>[] constraints) => _constraints = constraints;
        [SerializeField] private SuperModuleArray<T>[] _constraints;

        
        public abstract SuperOrientation AllOrientations { get; }
        public int AddRotations(int rotationA, int rotationB) => ((rotationA + rotationB) % Sides);


        public virtual void SetFeatures()
        {
            Clear();
            _features = new byte[Sides]; 
            _featuresReflected = new byte[Sides]; 
            _featureFlags = 0;

            // Get features
            for (int i = 0; i < _features.Length; i ++)
            {
                int a = int.Parse(ID.Substring( i,                         1));
                int b = int.Parse(ID.Substring((i + 1) % _features.Length, 1));

                _features[i]                                          = (byte)(10 * a + b);
                _featuresReflected[(i + Sides / 2) % Features.Length] = (byte)(10 * b + a);
            }

            // Set flags
            for (int i = 0; i < ID.Length; i ++)
            {
                int flag = int.Parse(ID.Substring(i, 1));
                flag = flag > 1 ? 1 << (flag - 1) : flag;
                _featureFlags |= flag;
            }
        }

        public abstract void UpdateAll();

        public virtual void Clear()
        {
            _features = new byte[0];            
            _constraints = new SuperModuleArray<T>[0];
            _featureFlags = 0;
        }
    }

    [Serializable]
    public abstract class FeatureFlagMask
    {
        public abstract int Mask(int orienation);
    }

    [Serializable]
    public struct SuperModuleArray<T>
        where T : Module<T>
    {
        public SuperModule<T>[] SuperModules;
        [SerializeField] int _setLength;

        public SuperModuleArray(SuperModule<T>[] superModules, int setLengh)
        {
            SuperModules = superModules;
            _setLength = setLengh;
        }

        public CellConstraint GetCellConstraint()
        {
            SuperPosition[] superPositions = new SuperPosition[_setLength];

            for(int i = 0; i < _setLength; i++)
                superPositions[i] = new SuperPosition(i, new SuperOrientation(0));

            for(int i = 0; i < SuperModules.Length; i++)
                superPositions[SuperModules[i].Module.Index] = SuperModules[i].SuperPosition;

            return new CellConstraint(superPositions);
        }
    }

    [Serializable]
    public struct SuperModule<T>
        where T : Module<T>
    {
        public T Module;
        public SuperModuleOrientations Orientations;
        public SuperPosition SuperPosition => new SuperPosition(Module.Index, Orientations.SuperOrientation);

        public SuperModule(T module, SuperModuleOrientations orientations)
        {
            Module = module;
            Orientations = orientations;
        }
    }

    [Serializable]
    public struct SuperModuleOrientations
    {
        [SerializeField] private int _orientationBitmask;
        public SuperModuleOrientations(int bitmask) => _orientationBitmask = bitmask;
        public void SetOrientation (int index) => _orientationBitmask |= (1 << index);
        public bool isValid { get => _orientationBitmask > 0; }
        public SuperOrientation SuperOrientation => new SuperOrientation(_orientationBitmask);

        
        // --- Operators --- //

        public static bool operator == (SuperModuleOrientations a, SuperModuleOrientations b) => a._orientationBitmask == b._orientationBitmask;
        public static bool operator != (SuperModuleOrientations a, SuperModuleOrientations b) => a._orientationBitmask != b._orientationBitmask;

        public override bool Equals(object obj)
        {
            if (!(obj is SuperModuleOrientations))
                return false;

            var other = (SuperModuleOrientations)obj;
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