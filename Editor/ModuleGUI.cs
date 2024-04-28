using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TribesAndTributes.WFC
{
    public class ModuleGUI<T> : Editor
        where T : Module<T>
    {
        T _baseTile;

        SerializedProperty _features, _featuresReflected, _featureFlagMask; // Features
        SerializedProperty _constraintSet, _constraints;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space(10);
            FindProperties();            
            DrawFeatures();
            EditorGUILayout.Space(20);
            DrawConstraints("Constraint Set");
            EditorGUILayout.Space(20);
            DrawUpdateButton("Update all Modules");

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }

        public virtual void FindProperties()
        {
            _baseTile          = (T) target;
            _features          = serializedObject.FindProperty("Features");
            _featuresReflected = serializedObject.FindProperty("FeaturesReflected");
            _featureFlagMask   = serializedObject.FindProperty("FeatureFlagMask");
            _constraintSet     = serializedObject.FindProperty("_constraints");
            _constraints       = _constraintSet.FindPropertyRelative("_set");
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
            SerializedProperty superPosition = _constraints.GetArrayElementAtIndex(i).FindPropertyRelative("SuperPositions");
            string hexSideFeature = _features.GetArrayElementAtIndex(i).intValue.ToString();
            
            EditorGUILayout.PropertyField(superPosition, new GUIContent("Constraints for side " + i.ToString() + " | Feature: " + hexSideFeature));
        }

        public virtual void DrawUpdateButton(string label)
        {
            if (GUILayout.Button(label))
            {
                _baseTile.UpdateAll();
                serializedObject.Update();
            }
        }
    }

    public class SuperPositionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the sub-properties of BaseTileNeighbour
            SerializedProperty orientationsProperty = property.FindPropertyRelative("Orientations");
            SerializedProperty neighbourProperty = property.FindPropertyRelative("Module");

            // Calculate label width
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect propertyRect = position;
            propertyRect.width -= labelWidth;
            propertyRect.x += labelWidth;

            // Draw the neighbour's name and orientation values as a label
            string orientationsString = "";

            for (int i = 0; i < orientationsProperty.arraySize; i++)
            {
                orientationsString += orientationsProperty.GetArrayElementAtIndex(i).intValue.ToString();
                if (i < orientationsProperty.arraySize - 1)
                    orientationsString += ", ";
            }

            EditorGUI.LabelField(position, neighbourProperty.objectReferenceValue != null ? neighbourProperty.objectReferenceValue.name + " [ " + orientationsString + " ]" : "None");
            EditorGUI.EndProperty();
        }
    }
}