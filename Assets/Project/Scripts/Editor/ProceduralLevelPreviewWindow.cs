using Project.Scripts.Generation.Procedural;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Editor
{
    /// <summary>
    /// Generates any archetype variant with a chosen seed in edit mode for quick
    /// visual inspection. Remember to clear the preview before entering play mode.
    /// </summary>
    public class ProceduralLevelPreviewWindow : EditorWindow
    {
        private const string PreviewRootName = "~ProceduralLevelPreview";

        private ProceduralLevelsConfig _config;
        private LevelArchetype _archetype;
        private int _seed = 12345;
        private float _difficulty;
        private bool _randomizeSeedEachTime = true;

        [MenuItem("Tools/Procedural Level Preview")]
        private static void Open()
        {
            GetWindow<ProceduralLevelPreviewWindow>("Level Preview");
        }

        private void OnEnable()
        {
            if (_config == null)
                _config = Resources.Load<ProceduralLevelsConfig>("ProceduralLevels/ProceduralLevelsConfig");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Procedural Level Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            _config = (ProceduralLevelsConfig)EditorGUILayout.ObjectField("Config", _config, typeof(ProceduralLevelsConfig), false);
            _archetype = (LevelArchetype)EditorGUILayout.ObjectField("Archetype Variant", _archetype, typeof(LevelArchetype), false);
            _seed = EditorGUILayout.IntField("Seed", _seed);
            _randomizeSeedEachTime = EditorGUILayout.Toggle("Randomize Seed", _randomizeSeedEachTime);
            _difficulty = EditorGUILayout.Slider("Difficulty", _difficulty, 0f, 1f);

            EditorGUILayout.Space();

            GUI.enabled = _config != null && _archetype != null;
            if (GUILayout.Button("Generate (New Seed)", GUILayout.Height(30)))
                Generate(newSeed: true);
            if (GUILayout.Button("Regenerate (Same Seed)", GUILayout.Height(24)))
                Generate(newSeed: false);
            GUI.enabled = true;

            if (GUILayout.Button("Clear Preview"))
                Clear();

            EditorGUILayout.HelpBox("Preview objects are generated under '" + PreviewRootName + "'. Clear them before entering play mode.", MessageType.Info);
        }

        private void Generate(bool newSeed)
        {
            Clear();

            if (newSeed && _randomizeSeedEachTime)
                _seed = Random.Range(1, int.MaxValue);

            var previewRoot = new GameObject(PreviewRootName);
            var location = ProceduralLevelBuilder.Build(_config, _archetype, Vector3.zero, Quaternion.identity, _seed, 0, _difficulty);
            location.transform.SetParent(previewRoot.transform, true);

            Selection.activeGameObject = location.gameObject;
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private static void Clear()
        {
            GameObject existing;
            while ((existing = GameObject.Find(PreviewRootName)) != null)
                DestroyImmediate(existing);
        }
    }
}
