using Noname.Application.Ports;
using Noname.Core.Primitives;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Noname.Infrastructure.Input
{
    /// <summary>
    /// Unity 입력 시스템을 통해 플레이어 입력을 읽습니다.
    /// </summary>
    public sealed class DefenseInputAdapter : MonoBehaviour, IGameInputReader
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float bombardmentPlaneZ = 0f;

        private InputAction _moveAction;

        private void Awake()
        {
            BuildActions();
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
        }

        private void OnDestroy()
        {
            _moveAction?.Dispose();
            _moveAction = null;
        }

        /// <summary>
        /// 현재 수평 입력 값을 읽습니다.
        /// </summary>
        public float ReadMovement()
        {
            if (InputBlocker.IsBlocked)
            {
                return 0f;
            }

            return _moveAction.ReadValue<float>();
        }

        /// <summary>
        /// 마우스/터치 위치를 월드 좌표로 변환해 폭격 지점으로 사용합니다.
        /// </summary>
        public bool TryReadBombardmentPoint(out Float2 worldPosition)
        {
            worldPosition = Float2.Zero;

            var cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null)
            {
                return false;
            }

            var (hasInput, screenPoint) = TryGetScreenPoint();
            if (!hasInput)
            {
                return false;
            }

            var plane = new Plane(Vector3.forward, new Vector3(0f, 0f, bombardmentPlaneZ));
            var ray = cam.ScreenPointToRay(screenPoint);
            if (!plane.Raycast(ray, out var enter))
            {
                return false;
            }

            var worldPoint = ray.GetPoint(enter);
            worldPosition = new Float2(worldPoint.x, worldPoint.y);
            return true;
        }

        private static (bool, Vector2) TryGetScreenPoint()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return (true, Mouse.current.position.ReadValue());
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return (true, Touchscreen.current.primaryTouch.position.ReadValue());
            }

            return (false, Vector2.zero);
        }

        private void BuildActions()
        {
            _moveAction = new InputAction("MoveHorizontal", InputActionType.Value);
            var composite = _moveAction.AddCompositeBinding("1DAxis");
            composite.With("Negative", "<Keyboard>/a");
            composite.With("Negative", "<Keyboard>/leftArrow");
            composite.With("Positive", "<Keyboard>/d");
            composite.With("Positive", "<Keyboard>/rightArrow");
            _moveAction.AddBinding("<Gamepad>/leftStick/x");
        }
    }
}
