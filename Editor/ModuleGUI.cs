using System;
using UnityEngine;
using UnityEditor;

namespace WaveFunctionCollapse
{
    public class ModuleGUI<T> : Editor
        where T : Module<T>
    {
        T _module;

        SerializedProperty _index, _features, _featuresReflected, _featureFlagMask; // Features
        SerializedProperty _constraintSet, _constraints;

        public override void OnInspectorGUI()
        {
            FindProperties();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(10);
            DrawIndex();
            EditorGUILayout.Space(10);
            DrawFeatures();
            EditorGUILayout.Space(20);
            DrawConstraints("Constraint Set");
            EditorGUILayout.Space(20);
            DrawUpdateButton("Update all Modules");

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        public void DrawIndex() => EditorGUILayout.LabelField(_index.intValue.ToString(), EditorStyles.boldLabel);

        public virtual void FindProperties()
        {
            _module            = (T) target;
            _index             = serializedObject.FindProperty("Index");
            _features          = serializedObject.FindProperty("_features");
            _featuresReflected = serializedObject.FindProperty("_featuresReflected");
            _featureFlagMask   = serializedObject.FindProperty("FeatureFlagMask");
            _constraints       = serializedObject.FindProperty("_constraints");
        }

        public virtual void DrawFeatures()
        {
            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);
            string features = "";
            string featuresReflected = "";

            for (int i = 0; i < _features.arraySize; i++)
            {
                features          += _features         .GetArrayElementAtIndex(i).intValue.ToString();
                featuresReflected += _featuresReflected.GetArrayElementAtIndex(i).intValue.ToString();

                if (i < _features.arraySize - 1)
                {
                    features          += " | ";
                    featuresReflected += " | ";
                }
            }

            EditorGUILayout.LabelField("Features: ", features);
            EditorGUILayout.LabelField("Features Reflected: ", featuresReflected);
        }

        public virtual void DrawConstraints(string header)
        {
            EditorGUILayout.LabelField(header, EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            for (int i = 0; i < _constraints.arraySize; i++)
            {
                DrawSuperPositions(i);
                EditorGUILayout.Space(20);
            }
        }

        public virtual void DrawSuperPositions(int i)
        {
            SerializedProperty superPosition = _constraints.GetArrayElementAtIndex(i).FindPropertyRelative("SuperModules");
            string hexSideFeature = _module.Features[i].ToString();
            
            EditorGUILayout.PropertyField(superPosition, new GUIContent("Constraints for side " + i.ToString() + " | Feature: " + hexSideFeature));
        }

        public virtual void DrawUpdateButton(string label)
        {
            if (GUILayout.Button(label))
            {
                _module.UpdateAll();
                serializedObject.Update();
            }
        }
    }

    [CustomPropertyDrawer(typeof(SuperModule<>))]
    public class SuperModuleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the sub-properties of BaseTileNeighbour
            SerializedProperty orientationsProperty = property.FindPropertyRelative("Orientations");
            SerializedProperty bitmaskProperty = orientationsProperty.FindPropertyRelative("_orientationBitmask");
            SerializedProperty constraintProperty = property.FindPropertyRelative("Module");
            SerializedObject moduleObject = new SerializedObject(constraintProperty.objectReferenceValue);

            // Calculate label width
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect propertyRect = position;
            propertyRect.width -= labelWidth;
            propertyRect.x += labelWidth;

            // Draw the neighbour's name and constraint values as a label
            string orientationsString = "";
            int orientations = bitmaskProperty.intValue;
            int orientation = 0;

            while (orientations > 0)
            {
                // Get smallest rotation / the position of the lowest set bit
                orientation = (int)Math.Log(orientations & -orientations, 2);
                orientationsString += orientation.ToString();

                // Clear the lowest set bit
                orientations &= (orientations - 1);

                if (orientations > 0)
                    orientationsString += ", ";
            }

            EditorGUI.LabelField(position, constraintProperty.objectReferenceValue != null ? constraintProperty.objectReferenceValue.name + " [ " + orientationsString + " ]" : "None");
            EditorGUI.EndProperty();
        }
    }
}