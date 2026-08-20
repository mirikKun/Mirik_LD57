using UnityEngine;

namespace Project.Scripts.Generation.Procedural
{
    public class CeilingProtrusion : MonoBehaviour
    {
        [SerializeField] private Transform _origin;
        [SerializeField] private Transform _column;
        [SerializeField] private Transform _platform;
        [SerializeField] private MeshRenderer _columnRenderer;
        [SerializeField] private MeshRenderer _platformRenderer;

        public Vector3 Apply(float height, Vector3 platformSize, Vector3 columnSize, Material columnMaterial, Material platformMaterial)
        {
            Vector3 origin = _origin.localPosition;
            float extraHeight = columnSize.y;
            float columnHeight = height + extraHeight;
            _column.localPosition = origin + Vector3.down * (columnHeight * 0.5f) ;
            _column.localScale = new Vector3(columnSize.x, columnHeight, columnSize.z);
            _columnRenderer.sharedMaterial = columnMaterial;

            _platform.localScale = platformSize;
            _platform.localPosition = origin + Vector3.down * (columnHeight + platformSize.y * 0.5f);
            _platformRenderer.sharedMaterial = platformMaterial;

            return origin + Vector3.down * columnHeight;
        }
    }
}
