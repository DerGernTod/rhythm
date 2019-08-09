using Rhythm.UI;
using Rhythm.Utils;
using UnityEngine;

namespace Rhythm.Camera {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class SwipeToMove : MonoBehaviour {

#pragma warning disable 0649
        [SerializeField] private Bounds bounds;
        [SerializeField] private float damping = 5;
#pragma warning restore 0649
        private Vector3 _target;
        private Vector3 _prevMousePos;
        private Vector3 _offset;
        private bool _moveEnabled;
        private bool _canStartMove;
        private Vector3 _halfScreenSize;
        private Vector3 _velocity = Vector3.zero;
        private Vector2 xyScale;
        private UnityEngine.Camera _cam;
        private int _touchableLayer;

        private void Start() {
            Vector3 curPos = transform.position;
            _offset = _prevMousePos - curPos;
            _target = curPos + _offset;
            _cam = GetComponent<UnityEngine.Camera>();
            float aspect = Screen.width / (float) Screen.height;
            float cameraOrthographicSize = _cam.orthographicSize;
            xyScale = 2f * new Vector2(aspect * cameraOrthographicSize / Screen.width, cameraOrthographicSize / Screen.height);
            _prevMousePos = new Vector3(Input.mousePosition.x * xyScale.x, Input.mousePosition.y * xyScale.y, curPos.z);
            _halfScreenSize = new Vector2(cameraOrthographicSize * aspect, cameraOrthographicSize);
            _touchableLayer = LayerMask.NameToLayer(Constants.LAYER_TOUCHABLES);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
        }

        private void Update() {
            RaycastHit2D hitInfo = Physics2D.Raycast(_cam.ScreenToWorldPoint(Input.mousePosition), Vector3.zero, 0, 1 << _touchableLayer);
            _canStartMove = !hitInfo.collider;
            Debug.Log("ray hit " + hitInfo.collider?.name);
            if (!_moveEnabled && _canStartMove) {
                _moveEnabled = Input.GetMouseButtonDown(0);
            }

            if (_moveEnabled) {
                _moveEnabled = !Input.GetMouseButtonUp(0);
            }

            Vector3 mousePosition = new Vector3(
                Input.mousePosition.x * xyScale.x,
                Input.mousePosition.y * xyScale.y,
                transform.position.z);
            
            if (_moveEnabled) {
                _velocity = _prevMousePos - mousePosition;
            } else {
                _velocity = Vector3.Lerp(_velocity, Vector3.zero, Time.deltaTime * damping);
            }

            Vector3 targetWithoutOffset = _target - _offset + _velocity;
            float clampedX = Mathf.Clamp(targetWithoutOffset.x, bounds.min.x + _halfScreenSize.x, bounds.max.x - _halfScreenSize.x);
            float clampedY = Mathf.Clamp(targetWithoutOffset.y, bounds.min.y + _halfScreenSize.y, bounds.max.y - _halfScreenSize.y);
            _target = new Vector3(clampedX, clampedY, targetWithoutOffset.z) + _offset;
            _prevMousePos = mousePosition;
            transform.position = _target - _offset;
        }
    }
}