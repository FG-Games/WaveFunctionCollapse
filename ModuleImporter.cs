using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
            SuperOrientation superOrientation;

            // Make sure all Module's features are updated
            for (int i = 0; i < allModules.Length; i ++)
                allModules[i].SetFeatures();

            // Run through modules and test the features of each modules's side against the features of other modules sides
            for (byte i = 0; i < allModules.Length; i ++)
            {
                allModules[i].Index = i;

                // Set adjacent SuperModules for module
                SuperModuleArray<T>[] constraints = new SuperModuleArray<T>[allModules[i].Features.Length]; // THERE'S A CHANCE YOU CAN'T USE CONSTRAINTS WITH SUPERPOSITIONS IN BASETILES

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
                superOrientation = new SuperOrientation();

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

                if(superOrientation.Valid)
                    states.Add(new SuperModule<T>(superOrientation, adjacentModule));
            }
        }
    }

    [Serializable]
    public struct SuperModuleArray<T>
        where T : Module<T>
    {
        public SuperModule<T>[] SuperModules;
        [SerializeField] private int _setLength;

        public SuperModuleArray(SuperModule<T>[] superModules, int setLength)
        {
            SuperModules = superModules;
            _setLength = setLength;
        }

        public CellConstraint GetSuperPositions() => CreateUnmanaged<T>.CellConstraint(SuperModules, _setLength);
    }

    [Serializable]
    public struct SuperModule<T>
        where T : Module<T>
    {
        public SuperOrientation Orientations;
        public T Module;

        public SuperModule(SuperOrientation orientations, T module)
        {
            Orientations = orientations;
            Module = module;
        }

        public SuperPosition GetSuperPosition() => new SuperPosition(Orientations, Module.Index);
    }
}