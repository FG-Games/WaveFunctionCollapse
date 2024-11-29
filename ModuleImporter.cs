using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Collections;

namespace WaveFunctionCollapse
{
    public static class ModuleImporter<T>
        where T : Module<T>
    {
        public static void UpdateModules(ModuleSet<T> set)
        {
            generateConstraints(set);
        }

        static void generateConstraints(ModuleSet<T> set)
        {
            T[] allModules = set.Modules;
            List<SuperModule<T>> states = new List<SuperModule<T>>();
            SuperModuleOrientations superOrientation;

            // Make sure all Module's features are updated
            for (int i = 0; i < allModules.Length; i ++)
                allModules[i].SetFeatures();

            // Run through modules and test the features of each modules's side against the features of other modules sides
            for (byte i = 0; i < allModules.Length; i ++)
            {
                allModules[i].Index = i;

                // Set adjacent SuperModules for module
                SuperModuleArray<T>[] constraints = new SuperModuleArray<T>[allModules[i].Features.Length];

                // Run through each side of a module
                for (byte side = 0; side < allModules[i].Sides; side ++)
                {
                    evaluateSideFeature(side, allModules[i]);
                    constraints[side] = new SuperModuleArray<T>(states.ToArray(), set.Modules.Length);
                }

                allModules[i].UpdateConstraints(constraints);                
                EditorUtility.SetDirty(allModules[i]);
            }

            void evaluateSideFeature(int side, T module)
            {
                int exclusionMask = module.FeatureFlagMask.Mask(side);
                states = new List<SuperModule<T>>();

                // test other modules features against the feature, of the current side
                for (int i = 0; i < allModules.Length; i ++)
                {
                    // Exclude features
                    if((exclusionMask & allModules[i].FeatureFlags) != 0)
                        continue;

                    // find fitting orientations
                    evaluateAdjacentSideFeature(side, module, allModules[i]);
                }
            }

            void evaluateAdjacentSideFeature(int side, T module, T adjacentModule)
            {
                byte feature = module.Features[side];
                superOrientation = new SuperModuleOrientations();

                for (byte adjacentSide = 0; adjacentSide < adjacentModule.Sides; adjacentSide++)
                {
                    if (feature == adjacentModule.FeaturesReflected[adjacentSide])
                    {
                        int adjacentOrientation = (adjacentSide - side) % adjacentModule.Sides;

                        if (adjacentOrientation < 0)
                            adjacentOrientation += adjacentModule.Sides;

                        superOrientation.SetOrientation(adjacentOrientation);
                    }
                }

                if(superOrientation.isValid)
                    states.Add(new SuperModule<T>(adjacentModule, superOrientation));
            }
        }
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
            NativeArray<SuperPosition> superPositions = new NativeArray<SuperPosition>(_setLength, Allocator.TempJob);

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