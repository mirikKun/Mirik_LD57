using Project.Scripts.Generation;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Editor
{
    [CustomEditor(typeof(LocationsGenerator))]
    public class LocationsGeneratorEditor : UnityEditor.Editor
    {
        private LocationsGenerator _locationsGenerator;

        private void OnEnable()
        {
            _locationsGenerator = (LocationsGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Locations Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space();

            if (GUILayout.Button("Generate Next Location", GUILayout.Height(30)))
            {
                _locationsGenerator.GenerateNextLocation();
            
                EditorUtility.SetDirty(_locationsGenerator);
            
                if (Application.isPlaying)
                {
                    SceneView.RepaintAll();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}