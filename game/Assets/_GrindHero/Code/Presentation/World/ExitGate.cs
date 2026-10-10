using GrindHero.Application.Combat;
using GrindHero.Presentation.UI;
using UnityEngine;

namespace GrindHero.Presentation.World
{
    public sealed class ExitGate : MonoBehaviour
    {
        private const float ClosedPanelHeight = 1.5f;
        private const float OpenPanelHeight = 4f;

        private ICombatSession _combatSession;
        private HUDPresenter _hud;
        private Transform _panel;
        private BoxCollider _panelCollider;
        private Color _closedColor = new Color(0.72f, 0.18f, 0.12f);
        private Color _openColor = new Color(0.2f, 0.72f, 0.4f);

        public bool IsUnlocked { get; private set; }

        public void Initialize(ICombatSession combatSession, HUDPresenter hud, bool unlocked)
        {
            _combatSession = combatSession;
            _hud = hud;
            CreateGrayboxGate();
            SetUnlocked(unlocked);
        }

        public void SetUnlocked(bool unlocked)
        {
            IsUnlocked = unlocked;
            if (_panelCollider != null)
            {
                _panelCollider.enabled = !unlocked;
            }

            if (_panel != null && _panel.TryGetComponent<Renderer>(out var renderer))
            {
                if (renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.color = unlocked ? _openColor : _closedColor;
                }
            }
        }

        private void Update()
        {
            if (_panel == null)
            {
                return;
            }

            var targetHeight = IsUnlocked ? OpenPanelHeight : ClosedPanelHeight;
            var targetPosition = new Vector3(0f, targetHeight, 0f);
            _panel.localPosition = Vector3.MoveTowards(
                _panel.localPosition,
                targetPosition,
                4f * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<HeroActor>() == null || _combatSession == null || _hud == null)
            {
                return;
            }

            _hud.ShowMessage(
                IsUnlocked
                    ? "The next section is unlocked. It is outside this prototype."
                    : "Defeat the boss to unlock the exit.",
                IsUnlocked ? _openColor : _closedColor);
        }

        private void CreateGrayboxGate()
        {
            var trigger = GetComponent<BoxCollider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
            }

            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.2f, 0f);
            trigger.size = new Vector3(4f, 2.8f, 3f);

            CreatePost("Left gate post", new Vector3(-1.7f, 1.4f, 0f));
            CreatePost("Right gate post", new Vector3(1.7f, 1.4f, 0f));
            _panel = CreateBlocker();
        }

        private void CreatePost(string objectName, Vector3 localPosition)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = objectName;
            post.transform.SetParent(transform, false);
            post.transform.localPosition = localPosition;
            post.transform.localScale = new Vector3(0.3f, 2.8f, 0.5f);
            ApplyMaterial(post.GetComponent<Renderer>(), new Color(0.31f, 0.29f, 0.25f));
        }

        private Transform CreateBlocker()
        {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Gate panel";
            blocker.transform.SetParent(transform, false);
            blocker.transform.localPosition = new Vector3(0f, ClosedPanelHeight, 0f);
            blocker.transform.localScale = new Vector3(3.1f, 2.6f, 0.28f);
            _panelCollider = blocker.GetComponent<BoxCollider>();
            ApplyMaterial(blocker.GetComponent<Renderer>(), _closedColor);
            return blocker.transform;
        }

        private static void ApplyMaterial(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                renderer.sharedMaterial = new Material(shader) { color = color };
            }
        }
    }
}
