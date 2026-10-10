using System;
using System.Globalization;
using GrindHero.Application.Combat;
using GrindHero.Presentation.Input;
using GrindHero.Presentation.World;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace GrindHero.Presentation.UI
{
    public sealed class HUDPresenter : MonoBehaviour
    {
        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _safeAreaRect;
        private Text _statsText;
        private Text _targetText;
        private Text _bossText;
        private Text _messageText;
        private Text _helpText;
        private Image _pauseButtonImage;
        private Text _pauseButtonText;
        private RectTransform _pauseButtonRect;
        private Image _joystickRing;
        private Image _joystickHandle;
        private ICombatSession _combatSession;
        private CombatWorld _combatWorld;
        private HeroActor _hero;
        private Action _togglePause;
        private float _messageUntil;
        private Rect _lastSafeArea;
        private bool _isPaused;
        private bool _hasFatalError;
        private bool _hasSaveError;
        private string _saveErrorMessage;

        private void Awake()
        {
            CreateCanvasIfNeeded();
            CreateHudElements();
        }

        public void Initialize(
            ICombatSession combatSession,
            CombatWorld combatWorld,
            HeroActor hero,
            DynamicJoystickInput joystick,
            Action togglePause)
        {
            _combatSession = combatSession;
            _combatWorld = combatWorld;
            _hero = hero;
            _togglePause = togglePause;
            if (joystick != null)
            {
                joystick.Configure(_canvasRect, _joystickRing, _joystickHandle, _pauseButtonRect);
            }

            ApplySafeArea();
        }

        public void ShowMessage(string message, Color color)
        {
            if (_messageText == null)
            {
                return;
            }

            _messageText.text = message;
            _messageText.color = color;
            _messageUntil = Time.unscaledTime + 3f;
        }

        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            if (_pauseButtonText != null)
            {
                _pauseButtonText.text = paused ? ">" : "II";
            }

            if (_pauseButtonImage != null)
            {
                _pauseButtonImage.color = paused
                    ? new Color(0.2f, 0.6f, 0.35f, 0.9f)
                    : new Color(0.14f, 0.17f, 0.21f, 0.8f);
            }

            if (!_hasFatalError && !_hasSaveError && _messageText != null)
            {
                _messageText.text = paused ? "PAUSED" : "RESUMED";
                _messageText.color = Color.white;
                _messageUntil = paused ? float.PositiveInfinity : Time.unscaledTime + 1f;
                _messageText.enabled = true;
            }
        }

        public void ShowFatalError(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _hasFatalError = true;
            _messageText.text = message;
            _messageText.color = new Color(1f, 0.42f, 0.32f);
            _messageUntil = float.PositiveInfinity;
            _messageText.enabled = true;
        }

        public void ShowSaveError(string message)
        {
            if (_messageText == null)
            {
                return;
            }

            _hasSaveError = true;
            _saveErrorMessage = message;
            _messageText.text = message;
            _messageText.color = new Color(1f, 0.42f, 0.32f);
            _messageUntil = float.PositiveInfinity;
            _messageText.enabled = true;
        }

        public void ClearSaveError()
        {
            _hasSaveError = false;
            _saveErrorMessage = null;
            if (_messageText != null && !_hasFatalError && !_isPaused)
            {
                _messageText.enabled = false;
            }
        }

        private void Update()
        {
            if (_canvas == null)
            {
                return;
            }

            HandlePauseInput();
            if (_lastSafeArea != Screen.safeArea)
            {
                ApplySafeArea();
            }

            if (_combatSession == null)
            {
                return;
            }

            var health = _combatSession.CurrentHealth.ToString("0.#", CultureInfo.InvariantCulture);
            var maximumHealth = _combatSession.MaximumHealth.ToString("0.#", CultureInfo.InvariantCulture);
            var axeDamage = _combatSession.AxeDamage.ToString("0.#", CultureInfo.InvariantCulture);
            var armor = (_combatSession.HeadAxeArmor + _combatSession.TorsoAxeArmor)
                .ToString("0.#", CultureInfo.InvariantCulture);
            _statsText.text =
                $"HP {health} / {maximumHealth}    Axe {axeDamage}    Axe armor {armor}    " +
                $"Dodge {_combatSession.DodgeChance:P0}    Block {_combatSession.BlockChance:P0}";

            if (_hero != null && _hero.CurrentTarget != null && _hero.CurrentTarget.IsAlive)
            {
                var target = _hero.CurrentTarget;
                _targetText.text =
                    $"{target.DisplayName}  {target.CurrentHealth:0.#} / {target.MaximumHealth:0.#} HP";
            }
            else
            {
                _targetText.text = $"Enemies nearby: {(_combatWorld != null ? _combatWorld.AliveEnemyCount : 0)}";
            }

            _bossText.text = _combatSession.BossDefeated
                ? "BOSS DEFEATED  ·  EXIT UNLOCKED"
                : "Defeat the boss to unlock the exit";
            if (_hasSaveError)
            {
                _messageText.text = _saveErrorMessage;
                _messageText.color = new Color(1f, 0.42f, 0.32f);
                _messageText.enabled = true;
            }
            else if (_isPaused)
            {
                _messageText.text = "PAUSED";
                _messageText.color = Color.white;
                _messageText.enabled = true;
            }
            else
            {
                _messageText.enabled = Time.unscaledTime < _messageUntil;
            }
        }

        private void CreateCanvasIfNeeded()
        {
            _canvas = GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.pixelPerfect = false;
            _canvasRect = _canvas.GetComponent<RectTransform>();

            var scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private void CreateHudElements()
        {
            _safeAreaRect = CreateRect("Safe area", _canvasRect);
            _safeAreaRect.anchorMin = Vector2.zero;
            _safeAreaRect.anchorMax = Vector2.one;
            _safeAreaRect.offsetMin = Vector2.zero;
            _safeAreaRect.offsetMax = Vector2.zero;

            _statsText = CreateText(
                "Hero stats",
                _safeAreaRect,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(1180f, 90f),
                new Vector2(28f, -24f),
                32,
                TextAnchor.UpperLeft);

            _targetText = CreateText(
                "Target status",
                _safeAreaRect,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(700f, 60f),
                new Vector2(28f, -94f),
                29,
                TextAnchor.UpperLeft);

            _bossText = CreateText(
                "Boss and exit status",
                _safeAreaRect,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(690f, 64f),
                new Vector2(-28f, -28f),
                29,
                TextAnchor.UpperRight);

            _messageText = CreateText(
                "Combat feedback",
                _safeAreaRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(1120f, 130f),
                Vector2.zero,
                42,
                TextAnchor.MiddleCenter);
            _messageText.enabled = false;

            _helpText = CreateText(
                "Controls",
                _safeAreaRect,
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(940f, 76f),
                new Vector2(0f, 34f),
                26,
                TextAnchor.MiddleCenter);
            _helpText.text = "Drag to move - auto-attack in range";

            _pauseButtonImage = CreateImage("Pause button", _safeAreaRect);
            _pauseButtonRect = _pauseButtonImage.rectTransform;
            _pauseButtonRect.anchorMin = new Vector2(1f, 0f);
            _pauseButtonRect.anchorMax = new Vector2(1f, 0f);
            _pauseButtonRect.pivot = new Vector2(1f, 0f);
            _pauseButtonRect.sizeDelta = new Vector2(126f, 82f);
            _pauseButtonRect.anchoredPosition = new Vector2(-32f, 28f);
            _pauseButtonImage.color = new Color(0.14f, 0.17f, 0.21f, 0.8f);
            _pauseButtonText = CreateText(
                "Pause button label",
                _pauseButtonRect,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                40,
                TextAnchor.MiddleCenter);
            _pauseButtonText.text = "II";

            _joystickRing = CreateImage("Dynamic joystick base", _canvasRect);
            _joystickRing.rectTransform.sizeDelta = new Vector2(220f, 220f);
            _joystickRing.color = new Color(0.12f, 0.14f, 0.18f, 0.45f);
            _joystickHandle = CreateImage("Dynamic joystick handle", _canvasRect);
            _joystickHandle.rectTransform.sizeDelta = new Vector2(86f, 86f);
            _joystickHandle.color = new Color(0.85f, 0.9f, 0.96f, 0.72f);
            _joystickRing.transform.SetAsLastSibling();
            _joystickHandle.transform.SetAsLastSibling();
        }

        private void HandlePauseInput()
        {
            if (_togglePause == null || _pauseButtonRect == null)
            {
                return;
            }

            var shouldToggle = false;
            foreach (var touch in Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began &&
                    RectTransformUtility.RectangleContainsScreenPoint(
                        _pauseButtonRect,
                        touch.screenPosition,
                        null))
                {
                    shouldToggle = true;
                    break;
                }
            }

            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame &&
                RectTransformUtility.RectangleContainsScreenPoint(
                    _pauseButtonRect,
                    Mouse.current.position.ReadValue(),
                    null))
            {
                shouldToggle = true;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                shouldToggle = true;
            }

            if (shouldToggle)
            {
                _togglePause.Invoke();
            }
        }

        private void ApplySafeArea()
        {
            _lastSafeArea = Screen.safeArea;
            var minimum = _lastSafeArea.position;
            var maximum = minimum + _lastSafeArea.size;
            _safeAreaRect.anchorMin = minimum / new Vector2(Screen.width, Screen.height);
            _safeAreaRect.anchorMax = maximum / new Vector2(Screen.width, Screen.height);
            _safeAreaRect.offsetMin = Vector2.zero;
            _safeAreaRect.offsetMax = Vector2.zero;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            var gameObject = new GameObject(objectName, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject.GetComponent<RectTransform>();
        }

        private static Text CreateText(
            string objectName,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 size,
            Vector2 position,
            int fontSize,
            TextAnchor alignment)
        {
            var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var text = gameObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string objectName, Transform parent)
        {
            var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            gameObject.transform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.raycastTarget = false;
            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return image;
        }
    }
}
