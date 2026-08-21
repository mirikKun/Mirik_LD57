using Scripts.Player.DescentContorller;
using UnityEngine;

namespace Project.Scripts.Generation.Darkness
{
    public class DarknessFollowZone : MonoBehaviour
    {
        private DescentController _inside;

        public static DarknessFollowZone Ensure(Transform parent, DarknessFollowZone existing, float height, float diameter)
        {
            DarknessFollowZone zone = existing;
            if (zone == null && Application.isPlaying && parent.gameObject.scene.IsValid())
                zone = Create(parent);
            if (zone != null)
                zone.FitVerticalShaft(height, diameter);
            return zone;
        }

        private static DarknessFollowZone Create(Transform parent)
        {
            var go = new GameObject(nameof(DarknessFollowZone));
            go.transform.SetParent(parent, false);
            go.AddComponent<BoxCollider>().isTrigger = true;
            return go.AddComponent<DarknessFollowZone>();
        }

        public void FitVerticalShaft(float height, float diameter)
        {
            const float pad = 1f;
            var box = GetComponent<BoxCollider>();
            box.center = new Vector3(0f, -height * 0.5f, 0f);
            box.size = new Vector3(diameter, height + pad * 2f, diameter);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_inside != null)
                return;
            if (!other.TryGetComponent(out DescentController descent))
                return;
            _inside = descent;
            _inside.AddDarknessFollowZone();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent(out DescentController descent))
                return;
            if (_inside != descent)
                return;
            _inside.RemoveDarknessFollowZone();
            _inside = null;
        }

        private void OnDisable()
        {
            if (_inside == null)
                return;
            _inside.RemoveDarknessFollowZone();
            _inside = null;
        }
    }
}
