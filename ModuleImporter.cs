using System.Collections.Generic;
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
            List<SuperPosition<T>> states = new List<SuperPosition<T>>();
            SuperOrientation superOrientation;

            // Make sure all Module's features are updated
            for (int i = 0; i < allModules.Length; i ++)
                allModules[i].SetFeatures();

            // Run through modules and test the features of each modules's side against the features of other modules sides
            for (byte i = 0; i < allModules.Length; i ++)
            {
                allModules[i].Index = i;

                // Set adjacent SuperModules for module
                CellConstraint<T>[] constraints = new CellConstraint<T>[allModules[i].Features.Length];

                // Run through each side of a module
                for (byte side = 0; side < allModules[i].Sides; side ++)
                {
                    evaluateSideFeature(side, allModules[i]);
                    constraints[side] = new CellConstraint<T>(states.ToArray());
                }

                allModules[i].UpdateConstraints(constraints);                
                EditorUtility.SetDirty(allModules[i]);
            }

            void evaluateSideFeature(int side, T module)
            {
                int exclusionMask = module.FeatureFlagMask.Mask(side);
                states = new List<SuperPosition<T>>();

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
                    states.Add(new SuperPosition<T>(superOrientation, adjacentModule));
            }
        }


        // --- Get BaseTiles --- //

        /*static List<T> getAllModules(string moduleFolderPath)
        {
            // Get the folder path where this BaseTile resides
            string folderPath = AssetDatabase.GetAssetPath(baseTile.GetInstanceID());
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));

            // Load all BaseTile from that folder
            string[] guids = AssetDatabase.FindAssets("t:BaseTile", new[] { moduleFolderPath });
            List<T> modules = new List<T>();

            for (int i = 0; i < guids.Length; i ++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                modules.Add(AssetDatabase.LoadAssetAtPath<T>(assetPath));
            }

            // Debug.Log(modules.Count + " Modules were found by the ModuleImporter.");

            return modules;
        }*/
    }
}