using System.Collections;
using ImprovedTimers;
using UnityEngine;

namespace Assets.Scripts.Player.PlayerEffects
{
    public class HookEffects:MonoBehaviour
    {
        [SerializeField] private LineRenderer _grappleLineRenderer;
        [SerializeField] private Transform _grappleStartPoint;
        private Coroutine _grapplingLineDrawingCoroutine;

        public void StartLineDrawing(Vector3 end,CountdownTimer preparingTimer)
        {
            _grapplingLineDrawingCoroutine = StartCoroutine(GrapplingLineDrawing(end, preparingTimer));
        }
        
        public void DrawGrappleLine(Vector3 end,float progress)
        {
            Vector3 start = _grappleStartPoint.position;
            _grappleLineRenderer.positionCount = 2;
            _grappleLineRenderer.SetPosition(0, start);
            _grappleLineRenderer.SetPosition(1, Vector3.Lerp(start, end, progress));
        }

        public void ClearGrappleLine()
        {
            if (_grapplingLineDrawingCoroutine != null)
            {
                StopCoroutine(_grapplingLineDrawingCoroutine);
            }
            _grappleLineRenderer.positionCount = 0;
        }

        private IEnumerator GrapplingLineDrawing(Vector3 end, CountdownTimer preparingTimer)
        {
            while (preparingTimer.IsRunning)
            {
                DrawGrappleLine(end,1- preparingTimer.Progress);
                yield return null;
            }
            DrawGrappleLine(end, 1);
        }
       
        
    }
}