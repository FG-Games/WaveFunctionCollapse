using System.Collections.Generic;
using UnityEditor;

namespace WaveFunctionCollapse
{
    public static class ModuleImporter<T>
        where T : Module<T>
    {
        public static void UpdateModules(Module<T> module)
        {
            generateConstraints(module);
        }

        static void generateConstraints(Module<T> module)
        {
            List<T> allModules = GetAllModules();
            List<SuperPosition<T>> states = new List<SuperPosition<T>>();
            List<int> orientations = new List<int>();

            // Make sure all Module's features are updated
            for (int i = 0; i < allModules.Count; i ++)
                allModules[i].SetFeatures();

            // Run through modules and test the features of each modules's side against the features of other modules sides
            for (byte i = 0; i < allModules.Count; i ++)
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
                for (int i = 0; i < allModules.Count; i ++)
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
                orientations = new List<int>();

                for (byte adjacentSide = 0; adjacentSide < adjacentModule.Sides; adjacentSide ++)
                {
                    if (feature == adjacentModule.FeaturesReflected[adjacentSide])
                    {
                        int adjacentOrientation = (adjacentSide - side) % adjacentModule.Sides;

                        if(adjacentOrientation < 0)
                            adjacentOrientation += adjacentModule.Sides;
                            
                        orientations.Add((byte)adjacentOrientation);
                    }
                }

                if(orientations.Count > 0)
                    states.Add(new SuperPosition<T>(orientations.ToArray(), adjacentModule));
            }
        }


        // --- Get BaseTiles --- //

        public static List<T> GetAllModules()
        {
            // Get the folder path where this BaseTile resides
            /*string folderPath = AssetDatabase.GetAssetPath(baseTile.GetInstanceID());
            folderPath = folderPath.Substring(0, folderPath.LastIndexOf("/"));*/

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
        }

        static string moduleFolderPath = "Assets/GameAssets/BaseTiles"; // UPDATE THIS!
    }
}