using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    public class ColumnProtrusion : MonoBehaviour
    {
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _lower;
        [SerializeField] private Transform _upper;
        [SerializeField] private MeshRenderer _lowerRenderer;
        [SerializeField] private MeshRenderer _upperRenderer;

        public Vector3 Apply(
            Vector3 lowerSize,
            Vector3 upperSize,
            float gap,
            float lowerOffset,
            float upperOffset,
            bool hideUpper,
            Material lowerMaterial,
            Material upperMaterial)
        {
            Vector3 origin = _origin.localPosition;
            Vector3 lowerTop = origin + Vector3.up * lowerOffset;
            _lower.localScale = lowerSize;
            _lower.localPosition = lowerTop + Vector3.down * (lowerSize.y * 0.5f);
            _lowerRenderer.sharedMaterial = lowerMaterial;

            if (hideUpper)
            {
                _upper.gameObject.SetActive(false);
            }
            else
            {
                _upper.gameObject.SetActive(true);
                Vector3 upperBottom = lowerTop + Vector3.up * (gap + upperOffset);
                _upper.localScale = upperSize;
                _upper.localPosition = upperBottom + Vector3.up * (upperSize.y * 0.5f);
                _upperRenderer.sharedMaterial = upperMaterial;
            }

            return lowerTop;
        }
    }
}
