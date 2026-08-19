using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    [CreateAssetMenu(menuName = "Procedural Levels/Module Palette", fileName = "ModulePalette")]
    public class ModulePalette : ScriptableObject
    {
        [Header("Materials")]
        public Material StructureMaterial;
        public Material PlatformMaterial;
        public Material AccentMaterial;
        public Material GlowMaterial;

        [Header("Lighting")]
        public Color GuideLightColor = new Color(1f, 0.63f, 0.2f);
    }
}
