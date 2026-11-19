using GrindHero.Application.Combat;
using GrindHero.Presentation.UI;
using UnityEngine;
using System.Collections.Generic;

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
        private readonly List<Material> _ownedMaterials = new List<Material>();
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
                    ? "The eastern road is open. Mosswatch is yours to explore."
                    : "The Hollow Guardian holds this gate. Defeat it to open the eastern road.",
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
            var blocker = new GameObject("Gate panel", typeof(BoxCollider));
            blocker.transform.SetParent(transform, false);
            blocker.transform.localPosition = new Vector3(0f, ClosedPanelHeight, 0f);
            _panelCollider = blocker.GetComponent<BoxCollider>();
            _panelCollider.size = new Vector3(3.1f, 2.6f, 0.28f);
            for (var i = 0; i < 7; i++)
            {
                var bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = "Forged gate bar";
                bar.transform.SetParent(blocker.transform, false);
                bar.transform.localPosition = new Vector3(-1.35f + i * 0.45f, 0f, 0f);
                bar.transform.localScale = new Vector3(0.13f, 2.6f, 0.16f);
                Destroy(bar.GetComponent<Collider>());
                ApplyMaterial(bar.GetComponent<Renderer>(), new Color(0.25f, 0.32f, 0.31f));
            }
            for (var i = 0; i < 2; i++)
            {
                var brace = GameObject.CreatePrimitive(PrimitiveType.Cube);
                brace.name = "Gate brace";
                brace.transform.SetParent(blocker.transform, false);
                brace.transform.localPosition = new Vector3(0f, i == 0 ? -0.7f : 0.7f, 0f);
                brace.transform.localScale = new Vector3(3f, 0.16f, 0.22f);
                Destroy(brace.GetComponent<Collider>());
                ApplyMaterial(brace.GetComponent<Renderer>(), new Color(0.83f, 0.57f, 0.25f));
            }
            return blocker.transform;
        }

        private void ApplyMaterial(Renderer renderer, Color color)
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
                var material = new Material(shader) { color = color };
                renderer.sharedMaterial = material;
                _ownedMaterials.Add(material);
            }
        }

        private void OnDestroy()
        {
            foreach (var material in _ownedMaterials) Destroy(material);
        }
    }
}
