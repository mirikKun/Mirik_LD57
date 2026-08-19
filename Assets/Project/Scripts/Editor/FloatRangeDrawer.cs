using Project.Scripts.Generation.Procedural;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(FloatRange))]
    public class FloatRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeDrawer.Draw(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(IntRange))]
    public class IntRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RangeDrawer.Draw(position, property, label);
        }
    }

    [CustomPropertyDrawer(typeof(Vector3Range))]
    public class Vector3RangeDrawer : PropertyDrawer
    {
        private const float Spacing = 2f;
        private const float LabelWidth = 28f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2f + Spacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            float line = EditorGUIUtility.singleLineHeight;
            float old = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;
            EditorGUI.PropertyField(
                new Rect(position.x, position.y, position.width, line),
                property.FindPropertyRelative("min"),
                new GUIContent("Min"));
            EditorGUI.PropertyField(
                new Rect(position.x, position.y + line + Spacing, position.width, line),
                property.FindPropertyRelative("max"),
                new GUIContent("Max"));
            EditorGUIUtility.labelWidth = old;

            EditorGUI.EndProperty();
        }
    }

    internal static class RangeDrawer
    {
        private const float Spacing = 4f;
        private const float LabelWidth = 28f;

        public static void Draw(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            float fieldWidth = (position.width - Spacing) * 0.5f;
            DrawLabeledField(new Rect(position.x, position.y, fieldWidth, position.height), "Min", property.FindPropertyRelative("x"));
            DrawLabeledField(new Rect(position.x + fieldWidth + Spacing, position.y, fieldWidth, position.height), "Max", property.FindPropertyRelative("y"));

            EditorGUI.EndProperty();
        }

        private static void DrawLabeledField(Rect rect, string name, SerializedProperty property)
        {
            float old = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = LabelWidth;
            EditorGUI.PropertyField(rect, property, new GUIContent(name));
            EditorGUIUtility.labelWidth = old;
        }
    }
}
