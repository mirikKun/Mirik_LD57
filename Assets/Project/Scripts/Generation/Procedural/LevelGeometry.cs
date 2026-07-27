using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    /// <summary>
    /// Low-level helpers that turn scaled primitives into level pieces.
    /// Works both in play mode and in edit mode (editor preview).
    /// </summary>
    public static class LevelGeometry
    {
        public static Transform CreateBox(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 size, Material material, bool withCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            return Setup(go, parent, name, localPosition, localRotation, size, material, withCollider);
        }

        /// <summary>Cylinder oriented along local Y. Size = (diameter, full height, diameter).</summary>
        public static Transform CreateCylinder(Transform parent, string name, Vector3 localPosition, Quaternion localRotation, float diameter, float height, Material material, bool withCollider = true)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            // Unity's cylinder primitive is 2 units tall and 1 unit wide at scale 1.
            Vector3 size = new Vector3(diameter, height * 0.5f, diameter);
            return Setup(go, parent, name, localPosition, localRotation, size, material, withCollider);
        }

        private static Transform Setup(GameObject go, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, Vector3 scale, Material material, bool withCollider)
        {
            go.name = name;
            Transform tr = go.transform;
            tr.SetParent(parent, false);
            tr.localPosition = localPosition;
            tr.localRotation = localRotation;
            tr.localScale = scale;

            go.GetComponent<MeshRenderer>().sharedMaterial = material;

            if (!withCollider)
            {
                Collider collider = go.GetComponent<Collider>();
                if (Application.isPlaying)
                    Object.Destroy(collider);
                else
                    Object.DestroyImmediate(collider);
            }
            else if (go.TryGetComponent(out CapsuleCollider capsule))
            {
                // Cylinder primitives ship with a capsule collider whose rounded caps
                // are unpleasant to stand on; replace with a box.
                if (Application.isPlaying)
                    Object.Destroy(capsule);
                else
                    Object.DestroyImmediate(capsule);
                // The cylinder mesh is 2 units tall in local space.
                var box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(1f, 2f, 1f);
            }

            return tr;
        }

        public static Light CreatePointLight(Transform parent, string name, Vector3 localPosition, Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }
    }
}
