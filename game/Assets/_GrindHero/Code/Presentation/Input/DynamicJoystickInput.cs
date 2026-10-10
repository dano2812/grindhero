using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GrindHero.Presentation.Input
{
    public sealed class DynamicJoystickInput : MonoBehaviour
    {
        [SerializeField, Min(16f)] private float radiusPixels = 110f;

        private RectTransform _canvasRect;
        private RectTransform _blockedRect;
        private RectTransform _ringRect;
        private RectTransform _handleRect;
        private bool _hasPointer;
        private bool _usingTouch;
        private bool _usingMouse;
        private int _activeTouchId;
        private Vector2 _pointerStart;

        public Vector2 MoveVector { get; private set; }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            EndPointer();
        }

        public void Configure(
            RectTransform canvasRect,
            Image ring,
            Image handle,
            RectTransform blockedRegion = null)
        {
            _canvasRect = canvasRect;
            _blockedRect = blockedRegion;
            _ringRect = ring != null ? ring.rectTransform : null;
            _handleRect = handle != null ? handle.rectTransform : null;

            if (_ringRect != null)
            {
                _ringRect.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            UpdateTouchInput();
            if (!_usingTouch)
            {
                UpdateMouseInput();
            }

            if (!_hasPointer && Keyboard.current != null)
            {
                var horizontal = 0f;
                var vertical = 0f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                {
                    horizontal -= 1f;
                }

                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                {
                    horizontal += 1f;
                }

                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                {
                    vertical -= 1f;
                }

                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                {
                    vertical += 1f;
                }

                MoveVector = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            }
        }

        private void UpdateTouchInput()
        {
            var foundPointer = false;
            var touches = Touch.activeTouches;
            for (var i = 0; i < touches.Count; i++)
            {
                var touch = touches[i];
                if (!_hasPointer && touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    BeginPointer(touch.screenPosition, true, touch.touchId);
                }

                if (!_usingTouch || touch.touchId != _activeTouchId)
                {
                    continue;
                }

                foundPointer = true;
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended ||
                    touch.phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    EndPointer();
                }
                else
                {
                    UpdatePointer(touch.screenPosition);
                }
            }

            if (_usingTouch && !foundPointer)
            {
                EndPointer();
            }
        }

        private void UpdateMouseInput()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (!_hasPointer && Mouse.current.leftButton.wasPressedThisFrame)
            {
                BeginPointer(Mouse.current.position.ReadValue(), false, 0);
            }

            if (_usingMouse && Mouse.current.leftButton.isPressed)
            {
                UpdatePointer(Mouse.current.position.ReadValue());
            }
            else if (_usingMouse && Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndPointer();
            }
        }

        private void BeginPointer(Vector2 screenPosition, bool isTouch, int touchId)
        {
            if (_blockedRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    _blockedRect,
                    screenPosition,
                    null))
            {
                return;
            }

            _hasPointer = true;
            _usingTouch = isTouch;
            _usingMouse = !isTouch;
            _activeTouchId = touchId;
            _pointerStart = screenPosition;
            MoveVector = Vector2.zero;

            if (_ringRect != null)
            {
                _ringRect.gameObject.SetActive(true);
                var localPosition = ScreenToCanvasPosition(screenPosition);
                _ringRect.anchoredPosition = localPosition;
                if (_handleRect != null)
                {
                    _handleRect.anchoredPosition = localPosition;
                }
            }
        }

        private void UpdatePointer(Vector2 screenPosition)
        {
            MoveVector = Vector2.ClampMagnitude(
                (screenPosition - _pointerStart) / radiusPixels,
                1f);

            if (_handleRect != null)
            {
                _handleRect.anchoredPosition =
                    _ringRect.anchoredPosition + (MoveVector * radiusPixels);
            }
        }

        private Vector2 ScreenToCanvasPosition(Vector2 screenPosition)
        {
            if (_canvasRect == null ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect,
                    screenPosition,
                    null,
                    out var localPosition))
            {
                return screenPosition - (new Vector2(Screen.width, Screen.height) * 0.5f);
            }

            return localPosition;
        }

        private void EndPointer()
        {
            _hasPointer = false;
            _usingTouch = false;
            _usingMouse = false;
            MoveVector = Vector2.zero;
            if (_ringRect != null)
            {
                _ringRect.gameObject.SetActive(false);
            }
        }
    }
}
