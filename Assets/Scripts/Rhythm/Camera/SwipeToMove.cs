using UnityEngine;

namespace Rhythm.Camera {
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class SwipeToMove : MonoBehaviour {

#pragma warning disable 0649
        [SerializeField] private Bounds bounds;
        [SerializeField] private float speedMultiplier = .025f;
        [SerializeField] private float damping = 5;
#pragma warning restore 0649
        private Vector3 _target;
        private Vector3 _prevMousePos;
        private Vector3 _offset;
        private bool _moveEnabled;
        private Vector3 _halfScreenSize;
        private float _movementScale;
        private Vector3 _velocity = Vector3.zero;

        private void Start() {
            Vector3 curPos = transform.position;
            _prevMousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, curPos.z);
            _offset = _prevMousePos - curPos;
            _target = curPos + _offset;
            UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
            float aspect = Screen.width / (float) Screen.height;
            float cameraOrthographicSize = cam.orthographicSize;
            _halfScreenSize = new Vector2(cameraOrthographicSize * aspect, cameraOrthographicSize);
            _movementScale = 1280f / Screen.height;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
        }

        private void Update() {
            _moveEnabled = Input.GetMouseButton(0);

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
            if (_moveEnabled) {
                _velocity = (_prevMousePos - mousePosition) * speedMultiplier * _movementScale;
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