using Assets.Scripts.General;
using UnityEngine;

namespace Assets.Scripts.Player.Controller
{
    public class WallDetector: MonoBehaviour
    {

        [SerializeField]private float _wallAngleLimit = 20;
        [SerializeField] private float _raycastDistance = 0.5f;
        [SerializeField] private Transform _cameraTransform;
        [SerializeField]private bool _isInDebugMode=true;
        [SerializeField] private LayerMask _layerMask;
        private CapsuleCollider _capsuleCollider;
        private bool _wallWasTouchedToTheLeft;
        private bool _wallWasTouchedToTheRight;
        private bool _wallWasTouchedToTheFront;
        private bool _wallWasTouchedToTheFrontBottom;
        private float _currentWallAngleLimit;

        private Vector3 _wallNormal;
        private RaycastSensor _dawnSensor;

        private RaycastSensor _topSensor;


        private void Start()
        {
            _capsuleCollider = GetComponent<CapsuleCollider>();
            _currentWallAngleLimit = _wallAngleLimit;
            int layerMask = _layerMask;
            _dawnSensor ??= new RaycastSensor(_cameraTransform);
            _topSensor ??= new RaycastSensor(_cameraTransform);
            
            _dawnSensor.layermask = layerMask;
            _topSensor.layermask = layerMask;
            
            float castLenght=_capsuleCollider.radius+_raycastDistance;
            _dawnSensor.castLength = castLenght;
            _topSensor.castLength = castLenght;

            Vector3 offset = new Vector3(0, _capsuleCollider.bounds.size.y/2, 0);
            _dawnSensor.SetCastOrigin(_capsuleCollider.bounds.center-offset);
            _topSensor.SetCastOrigin(_capsuleCollider.bounds.center+offset*2);

        }

        private void OnCollisionEnter(Collision collision) => CheckForContact(collision);

        private void OnCollisionStay(Collision collision) => CheckForContact(collision);
        private void LateUpdate() {
            if (_isInDebugMode) {
                _dawnSensor.DrawDebug();
                _topSensor.DrawDebug();
            }
        }
        private void CheckForContact(Collision collision)
        {
       
            if (collision.contacts.Length ==0) return;
            _wallNormal=collision.contacts[0].normal;
            float angleToLeft=Vector3.Angle(_wallNormal,_cameraTransform.right);
            float angleToRight=Vector3.Angle(_wallNormal,-_cameraTransform.right);
            float angleToFront=Vector3.Angle(_wallNormal,-_cameraTransform.forward);
            

            if(angleToLeft<_currentWallAngleLimit&&SideWallHit(RaycastSensor.CastDirection.Left))
                _wallWasTouchedToTheLeft = true;
            else if(angleToRight<_currentWallAngleLimit&&SideWallHit(RaycastSensor.CastDirection.Right))
                _wallWasTouchedToTheRight = true;
            else if (angleToFront < _currentWallAngleLimit)
            {
                if (SideWallHit(RaycastSensor.CastDirection.Forward))
                    _wallWasTouchedToTheFront = true;
                else if(ForwardBottomWallHit())
                {
                    _wallWasTouchedToTheFrontBottom = true;
                }
            }
            
        }
        public void SetWallAngleWithMultiplier(float multiplier) => _currentWallAngleLimit =_wallAngleLimit* multiplier;


        public Vector3 GetWallNormal() => _wallNormal;

        public bool HitForwardWall() => _wallWasTouchedToTheFront;
        public bool HitForwardBottomWall() => _wallWasTouchedToTheFrontBottom;

        public bool HitSidewaysWall() => _wallWasTouchedToTheLeft || _wallWasTouchedToTheRight;

        public void Reset()
        {
            _wallWasTouchedToTheLeft = false;
            _wallWasTouchedToTheRight = false;
            _wallWasTouchedToTheFront = false;
            _wallWasTouchedToTheFrontBottom = false;
        }

        private bool SideWallHit(RaycastSensor.CastDirection castDirection)
        {
            return _dawnSensor.CastInDirection(castDirection)&&_topSensor.CastInDirection(castDirection);
        }
        private bool ForwardBottomWallHit() => !_topSensor.CastInDirection(RaycastSensor.CastDirection.Forward)&&_dawnSensor.CastInDirection(RaycastSensor.CastDirection.Forward);
    }
}