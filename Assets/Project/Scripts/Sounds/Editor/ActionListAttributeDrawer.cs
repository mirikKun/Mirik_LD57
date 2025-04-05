using System;
using System.Linq;
using Project.Scripts.Sounds.Attributes;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Sounds.Editor
{
    [CustomPropertyDrawer(typeof(ActionListAttribute))]
    public class ActionListAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = property.managedReferenceFullTypename.Split(".").LastOrDefault();
            EditorGUI.PropertyField(position, property, label, true);

            if (string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                Rect addButtonRect = new(position.xMin, position.yMin, position.width, EditorGUIUtility.singleLineHeight);

                if (!GUI.Button(addButtonRect, "Select Action Type"))
                    return;

                GenericMenu menu = new();

                // Get all types in the current assembly that implement IInterface
                Type[] types = TypeCache.GetTypesDerivedFrom(typeof(ISoundSystemActionConfig)).ToArray();

                foreach (Type type in types)
                {
                    menu.AddItem(new GUIContent(type.Name.Replace("ActionConfig", "")), false, () => AddItem(property, type));
                }

                menu.ShowAsContext();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property);
        }

        // Add an item to the list property
        private static void AddItem(SerializedProperty property, Type type)
        {
            property.managedReferenceValue = Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}