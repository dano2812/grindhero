using System;
using System.Collections.Generic;
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
        private const float RespawnMarkerWidth = 280f;
        private const float RespawnMarkerHeight = 52f;
        private const float RespawnMarkerWorldOffset = 2.2f;
        private const float RespawnMarkerMinimumNameWidth = 90f;
        private const float RespawnCooldownTextWidth = 82f;
        private const float RespawnPipSize = 12f;
        private const float RespawnPipSpacing = 4f;
        private const float HealthBannerWidth = 210f;
        private const float HealthBannerHeight = 44f;
        private const float HealthBannerWorldOffset = 2.75f;
        private static readonly Color RemainingRespawnPipColor =
            new Color(0.98f, 0.72f, 0.24f, 1f);
        private static readonly Color UsedRespawnPipColor =
            new Color(0.16f, 0.2f, 0.26f, 1f);
        private static readonly Color HealthyHealthFillColor =
            new Color(0.27f, 0.76f, 0.4f, 1f);
        private static readonly Color WarningHealthFillColor =
            new Color(0.98f, 0.72f, 0.24f, 1f);
        private static readonly Color CriticalHealthFillColor =
            new Color(0.88f, 0.25f, 0.2f, 1f);

        private sealed class RespawnMarkerView
        {
            public RectTransform Root;
            public Text NameText;
            public Text CooldownText;
            public Image CooldownFill;
            public Image[] DailyRespawnPips;
            public int LastRemainingRespawns;
            public int LastVisibleFrame;
        }

        private sealed class HealthBannerView
        {
            public RectTransform Root;
            public Text HealthText;
            public Image HealthFill;
            public int LastUpdatedFrame;
        }

        private Canvas _canvas;
        private RectTransform _canvasRect;
        private RectTransform _safeAreaRect;
        private Camera _worldCamera;
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
        private EnemySpawnPoint[] _enemySpawnPoints = Array.Empty<EnemySpawnPoint>();
        private readonly Dictionary<string, RespawnMarkerView> _respawnMarkers =
            new Dictionary<string, RespawnMarkerView>(StringComparer.Ordinal);
        private readonly Dictionary<EntityId, HealthBannerView> _enemyHealthBanners =
            new Dictionary<EntityId, HealthBannerView>();
        private readonly List<EntityId> _staleEnemyHealthBannerIds = new List<EntityId>();
        private HealthBannerView _heroHealthBanner;
        private int _respawnUpdateFrame;
        private int _healthBannerUpdateFrame;
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
            Camera worldCamera,
            DynamicJoystickInput joystick,
            Action togglePause,
            EnemySpawnPoint[] enemySpawnPoints)
        {
            if (worldCamera == null)
            {
                throw new ArgumentNullException(nameof(worldCamera));
            }

            _combatSession = combatSession;
            _combatWorld = combatWorld;
            _hero = hero;
            _worldCamera = worldCamera;
            _togglePause = togglePause;
            _enemySpawnPoints = enemySpawnPoints ?? Array.Empty<EnemySpawnPoint>();
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

            UpdateRespawnIndicators();
            UpdateHealthBanners();

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

        private void UpdateRespawnIndicators()
        {
            if (_safeAreaRect == null || _worldCamera == null)
            {
                return;
            }

            var frame = ++_respawnUpdateFrame;
            foreach (var spawnPoint in _enemySpawnPoints)
            {
                if (spawnPoint == null || !spawnPoint.ShouldShowRespawnStatus)
                {
                    continue;
                }

                var markerWorldPosition =
                    spawnPoint.transform.position + Vector3.up * RespawnMarkerWorldOffset;
                if (!TryGetWorldScreenLocalPosition(markerWorldPosition, out var markerPosition))
                {
                    continue;
                }

                if (!_respawnMarkers.TryGetValue(spawnPoint.SpawnId, out var marker))
                {
                    marker = CreateRespawnMarker(spawnPoint);
                    _respawnMarkers.Add(spawnPoint.SpawnId, marker);
                }

                marker.LastVisibleFrame = frame;
                if (!marker.Root.gameObject.activeSelf)
                {
                    marker.Root.gameObject.SetActive(true);
                }

                if (marker.Root.anchoredPosition != markerPosition)
                {
                    marker.Root.anchoredPosition = markerPosition;
                }

                var cooldownText = FormatRespawnCooldown(spawnPoint.RespawnSecondsRemaining);
                if (marker.CooldownText.text != cooldownText)
                {
                    marker.CooldownText.text = cooldownText;
                }

                marker.CooldownFill.fillAmount = spawnPoint.RespawnCooldownProgress;
                var remainingRespawns = Mathf.Clamp(
                    spawnPoint.MaximumRespawnsPerDay - spawnPoint.RespawnsToday,
                    0,
                    marker.DailyRespawnPips.Length);

                if (marker.LastRemainingRespawns != remainingRespawns)
                {
                    for (var pipIndex = 0; pipIndex < marker.DailyRespawnPips.Length; pipIndex++)
                    {
                        marker.DailyRespawnPips[pipIndex].color = pipIndex < remainingRespawns
                            ? RemainingRespawnPipColor
                            : UsedRespawnPipColor;
                    }

                    marker.LastRemainingRespawns = remainingRespawns;
                }
            }

            foreach (var marker in _respawnMarkers.Values)
            {
                if (marker.LastVisibleFrame != frame && marker.Root.gameObject.activeSelf)
                {
                    marker.Root.gameObject.SetActive(false);
                }
            }
        }

        private void UpdateHealthBanners()
        {
            if (_safeAreaRect == null || _worldCamera == null)
            {
                return;
            }

            var frame = ++_healthBannerUpdateFrame;
            if (_hero != null)
            {
                if (_heroHealthBanner == null)
                {
                    _heroHealthBanner = CreateHealthBanner("Hero health banner");
                }

                UpdateHealthBanner(
                    _heroHealthBanner,
                    "Hero",
                    _combatSession.CurrentHealth,
                    _combatSession.MaximumHealth,
                    _hero.transform.position + Vector3.up * HealthBannerWorldOffset,
                    frame);
            }

            if (_combatWorld != null)
            {
                var enemies = _combatWorld.ActiveEnemies;
                for (var enemyIndex = 0; enemyIndex < enemies.Count; enemyIndex++)
                {
                    var enemy = enemies[enemyIndex];
                    if (enemy == null || !enemy.IsAlive)
                    {
                        continue;
                    }

                    var enemyId = enemy.GetEntityId();
                    if (!_enemyHealthBanners.TryGetValue(enemyId, out var banner))
                    {
                        banner = CreateHealthBanner($"Mob health banner {enemyId}");
                        _enemyHealthBanners.Add(enemyId, banner);
                    }

                    UpdateHealthBanner(
                        banner,
                        enemy.DisplayName,
                        enemy.CurrentHealth,
                        enemy.MaximumHealth,
                        enemy.transform.position + Vector3.up * HealthBannerWorldOffset,
                        frame);
                }
            }

            _staleEnemyHealthBannerIds.Clear();
            foreach (var entry in _enemyHealthBanners)
            {
                if (entry.Value.LastUpdatedFrame == frame)
                {
                    continue;
                }

                if (entry.Value.Root != null)
                {
                    Destroy(entry.Value.Root.gameObject);
                }

                _staleEnemyHealthBannerIds.Add(entry.Key);
            }

            foreach (var enemyId in _staleEnemyHealthBannerIds)
            {
                _enemyHealthBanners.Remove(enemyId);
            }
        }

        private void UpdateHealthBanner(
            HealthBannerView banner,
            string actorName,
            float currentHealth,
            float maximumHealth,
            Vector3 worldPosition,
            int frame)
        {
            banner.LastUpdatedFrame = frame;
            if (!TryGetWorldScreenLocalPosition(worldPosition, out var localPosition))
            {
                if (banner.Root.gameObject.activeSelf)
                {
                    banner.Root.gameObject.SetActive(false);
                }

                return;
            }

            if (!banner.Root.gameObject.activeSelf)
            {
                banner.Root.gameObject.SetActive(true);
            }

            if (banner.Root.anchoredPosition != localPosition)
            {
                banner.Root.anchoredPosition = localPosition;
            }

            var healthText = string.Format(
                CultureInfo.InvariantCulture,
                "{0}  {1:0.#} / {2:0.#} HP",
                actorName,
                currentHealth,
                maximumHealth);
            if (banner.HealthText.text != healthText)
            {
                banner.HealthText.text = healthText;
            }

            var healthFraction = maximumHealth > 0f
                ? Mathf.Clamp01(currentHealth / maximumHealth)
                : 0f;
            banner.HealthFill.fillAmount = healthFraction;
            banner.HealthFill.color = healthFraction > 0.5f
                ? HealthyHealthFillColor
                : healthFraction > 0.25f
                    ? WarningHealthFillColor
                    : CriticalHealthFillColor;
        }

        private bool TryGetWorldScreenLocalPosition(
            Vector3 worldPosition,
            out Vector2 localPosition)
        {
            var screenPosition = _worldCamera.WorldToScreenPoint(worldPosition);
            var screenPoint = new Vector2(screenPosition.x, screenPosition.y);
            if (screenPosition.z <= 0f || !Screen.safeArea.Contains(screenPoint))
            {
                localPosition = Vector2.zero;
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _safeAreaRect,
                screenPoint,
                null,
                out localPosition);
        }

        private RespawnMarkerView CreateRespawnMarker(EnemySpawnPoint spawnPoint)
        {
            var pipCount = spawnPoint.MaximumRespawnsPerDay;
            var pipGroupWidth = pipCount * RespawnPipSize +
                Mathf.Max(0, pipCount - 1) * RespawnPipSpacing;
            var cooldownRightOffset = 16f + pipGroupWidth;
            var markerWidth = Mathf.Max(
                RespawnMarkerWidth,
                cooldownRightOffset +
                RespawnCooldownTextWidth +
                RespawnMarkerMinimumNameWidth +
                16f);
            var nameWidth = markerWidth -
                cooldownRightOffset -
                RespawnCooldownTextWidth -
                16f;
            var root = CreateRect($"Respawn status {spawnPoint.SpawnId}", _safeAreaRect);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(markerWidth, RespawnMarkerHeight);

            var background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.07f, 0.1f, 0.78f);
            background.raycastTarget = false;
            var nameText = CreateText(
                $"Respawn mob name {spawnPoint.SpawnId}",
                root,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(nameWidth, 24f),
                new Vector2(8f, -4f),
                18,
                TextAnchor.UpperLeft);
            nameText.text = spawnPoint.Definition.displayName;
            var cooldownText = CreateText(
                $"Respawn cooldown {spawnPoint.SpawnId}",
                root,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(RespawnCooldownTextWidth, 24f),
                new Vector2(-cooldownRightOffset, -4f),
                18,
                TextAnchor.UpperRight);

            var pipGroup = CreateRect($"Daily respawn indicators {spawnPoint.SpawnId}", root);
            pipGroup.anchorMin = new Vector2(1f, 1f);
            pipGroup.anchorMax = new Vector2(1f, 1f);
            pipGroup.pivot = new Vector2(1f, 1f);
            pipGroup.sizeDelta = new Vector2(pipGroupWidth, RespawnPipSize + 2f);
            pipGroup.anchoredPosition = new Vector2(-8f, -7f);

            var pips = new Image[pipCount];
            for (var pipIndex = 0; pipIndex < pips.Length; pipIndex++)
            {
                var pip = CreateImage($"Daily respawn indicator {pipIndex + 1}", pipGroup);
                pip.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                pip.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                pip.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                pip.rectTransform.sizeDelta = new Vector2(RespawnPipSize, RespawnPipSize);
                var pipX = -pipGroupWidth + RespawnPipSize * 0.5f +
                    pipIndex * (RespawnPipSize + RespawnPipSpacing);
                pip.rectTransform.anchoredPosition = new Vector2(pipX, 0f);

                var outline = pip.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(0.72f, 0.79f, 0.87f, 0.8f);
                outline.effectDistance = new Vector2(1f, -1f);
                outline.useGraphicAlpha = false;
                pips[pipIndex] = pip;
            }

            var cooldownTrack = CreateImage("Respawn cooldown track", root);
            cooldownTrack.color = new Color(0.12f, 0.16f, 0.2f, 0.95f);
            cooldownTrack.rectTransform.anchorMin = new Vector2(0f, 0f);
            cooldownTrack.rectTransform.anchorMax = new Vector2(1f, 0f);
            cooldownTrack.rectTransform.pivot = new Vector2(0.5f, 0f);
            cooldownTrack.rectTransform.sizeDelta = new Vector2(-12f, 6f);
            cooldownTrack.rectTransform.anchoredPosition = new Vector2(0f, 5f);

            var cooldownFill = CreateImage("Respawn cooldown progress", cooldownTrack.transform);
            cooldownFill.rectTransform.anchorMin = Vector2.zero;
            cooldownFill.rectTransform.anchorMax = Vector2.one;
            cooldownFill.rectTransform.offsetMin = Vector2.zero;
            cooldownFill.rectTransform.offsetMax = Vector2.zero;
            cooldownFill.type = Image.Type.Filled;
            cooldownFill.fillMethod = Image.FillMethod.Horizontal;
            cooldownFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            cooldownFill.fillAmount = 0f;
            cooldownFill.color = new Color(0.27f, 0.76f, 0.68f, 1f);

            return new RespawnMarkerView
            {
                Root = root,
                NameText = nameText,
                CooldownText = cooldownText,
                CooldownFill = cooldownFill,
                DailyRespawnPips = pips,
                LastRemainingRespawns = int.MinValue
            };
        }

        private HealthBannerView CreateHealthBanner(string objectName)
        {
            var root = CreateRect(objectName, _safeAreaRect);
            root.anchorMin = new Vector2(0.5f, 0.5f);
            root.anchorMax = new Vector2(0.5f, 0.5f);
            root.pivot = new Vector2(0.5f, 0.5f);
            root.sizeDelta = new Vector2(HealthBannerWidth, HealthBannerHeight);

            var background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.07f, 0.1f, 0.78f);
            background.raycastTarget = false;
            var healthText = CreateText(
                "Health values",
                root,
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(-12f, 22f),
                new Vector2(0f, -2f),
                16,
                TextAnchor.MiddleCenter);

            var healthTrack = CreateImage("Health bar track", root);
            healthTrack.color = new Color(0.12f, 0.16f, 0.2f, 0.95f);
            healthTrack.rectTransform.anchorMin = new Vector2(0f, 0f);
            healthTrack.rectTransform.anchorMax = new Vector2(1f, 0f);
            healthTrack.rectTransform.pivot = new Vector2(0.5f, 0f);
            healthTrack.rectTransform.sizeDelta = new Vector2(-12f, 8f);
            healthTrack.rectTransform.anchoredPosition = new Vector2(0f, 4f);

            var healthFill = CreateImage("Health bar fill", healthTrack.transform);
            healthFill.rectTransform.anchorMin = Vector2.zero;
            healthFill.rectTransform.anchorMax = Vector2.one;
            healthFill.rectTransform.offsetMin = Vector2.zero;
            healthFill.rectTransform.offsetMax = Vector2.zero;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;
            healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
            healthFill.fillAmount = 1f;
            healthFill.color = HealthyHealthFillColor;

            root.gameObject.SetActive(false);
            return new HealthBannerView
            {
                Root = root,
                HealthText = healthText,
                HealthFill = healthFill
            };
        }

        private static string FormatRespawnCooldown(float remainingSeconds)
        {
            var totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds));
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:00}:{1:00}",
                totalSeconds / 60,
                totalSeconds % 60);
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
