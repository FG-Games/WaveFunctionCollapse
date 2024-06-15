using System;
using System.Collections.Generic;
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
            SerializedProperty superPosition = _constraints.GetArrayElementAtIndex(i).FindPropertyRelative("SuperPositions");
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

    public class SuperPositionDrawer : PropertyDrawer
    {
        private static Color s_gray = new Color(0.4f, 0.4f, 0.4f, 1);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Get the sub-properties of BaseTileNeighbour
            SerializedProperty orientationsProperty = property.FindPropertyRelative("Orientations");
            SerializedProperty constraintProperty = property.FindPropertyRelative("Module");
            SerializedObject moduleObject = new SerializedObject(constraintProperty.objectReferenceValue);
            SerializedProperty passiveProperty = moduleObject.FindProperty("Passive");

            // Calculate label width
            float labelWidth = EditorGUIUtility.labelWidth;
            Rect propertyRect = position;
            propertyRect.width -= labelWidth;
            propertyRect.x += labelWidth;

            // Draw the neighbour's name and constraint values as a label
            string orientationsString = "";

            for (int i = 0; i < orientationsProperty.arraySize; i++)
            {
                orientationsString += orientationsProperty.GetArrayElementAtIndex(i).intValue.ToString();
                if (i < orientationsProperty.arraySize - 1)
                    orientationsString += ", ";
            }

            // Create a new GUIStyle and set its text color to red
            GUIStyle colorStyle = new GUIStyle(EditorStyles.label);
            colorStyle.normal.textColor = passiveProperty.boolValue ? s_gray : Color.black;

            EditorGUI.LabelField(position, constraintProperty.objectReferenceValue != null ? constraintProperty.objectReferenceValue.name + " [ " + orientationsString + " ]" : "None", colorStyle);
            EditorGUI.EndProperty();
        }
    }
}