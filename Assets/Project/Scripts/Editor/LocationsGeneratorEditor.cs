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
                SceneView.RepaintAll();
            }

            GUI.enabled = Application.isPlaying || _locationsGenerator.CanRegenerateLastLocation;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Regenerate Last (Same Seed)", GUILayout.Height(24)))
            {
                _locationsGenerator.RegenerateLastLocation(newSeed: false);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Regenerate Last (New Seed)", GUILayout.Height(24)))
            {
                _locationsGenerator.RegenerateLastLocation(newSeed: true);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Edit mode: Generate builds a single level from Start Location. Regenerate rebuilds that same level.",
                    MessageType.Info);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}