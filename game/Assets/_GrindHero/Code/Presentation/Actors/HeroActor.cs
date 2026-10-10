using GrindHero.Application.Combat;
using GrindHero.Domain.Combat;
using GrindHero.Domain.Progression;
using GrindHero.Presentation.Definitions;
using GrindHero.Presentation.Input;
using GrindHero.Presentation.World;
using System.Collections.Generic;
using UnityEngine;

namespace GrindHero.Presentation
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class HeroActor : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float turnSpeedDegreesPerSecond = 720f;

        private CharacterController _controller;
        private FirstSliceTuning _tuning;
        private CombatWorld _combatWorld;
        private DynamicJoystickInput _movementInput;
        private ICombatSession _combatSession;
        private Transform _spawnPoint;
        private Transform _weaponPivot;
        private float _nextAttackStartTime;
        private float _windupRemaining;
        private bool _attackInProgress;
        private EnemyActor _attackTarget;
        private readonly List<Material> _ownedMaterials = new List<Material>();

        public bool IsInitialized { get; private set; }
        public EnemyActor CurrentTarget { get; private set; }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            CreateGrayboxModel();
        }

        public void Initialize(
            FirstSliceTuning tuning,
            CombatWorld combatWorld,
            DynamicJoystickInput movementInput,
            ICombatSession combatSession,
            Transform spawnPoint)
        {
            _tuning = tuning;
            _combatWorld = combatWorld;
            _movementInput = movementInput;
            _combatSession = combatSession;
            _spawnPoint = spawnPoint;
            _nextAttackStartTime = Time.time;
            IsInitialized = true;

            if (_tuning.startingWeapon != null)
            {
                _tuning.startingWeapon.ValidateDefinition();
            }

            CreateArmorVisuals();
        }

        public void RespawnAtSpawn()
        {
            _attackInProgress = false;
            _attackTarget = null;
            CurrentTarget = null;

            _controller.enabled = false;
            transform.position = _spawnPoint.position;
            transform.rotation = _spawnPoint.rotation;
            _controller.enabled = true;

            _nextAttackStartTime = Time.time;
        }

        private void Update()
        {
            if (!IsInitialized)
            {
                return;
            }

            MoveHero();
            UpdateAutomaticAttack();
        }

        private void MoveHero()
        {
            var input = _movementInput != null ? _movementInput.MoveVector : Vector2.zero;
            var cameraTransform = Camera.main != null ? Camera.main.transform : null;
            var forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
            var right = cameraTransform != null ? cameraTransform.right : Vector3.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            var direction = (right * input.x) + (forward * input.y);
            direction = Vector3.ClampMagnitude(direction, 1f);
            if (direction.sqrMagnitude > 0.0001f)
            {
                _controller.SimpleMove(direction * _tuning.movementSpeed);
                var desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    desiredRotation,
                    turnSpeedDegreesPerSecond * Time.deltaTime);
            }
            else
            {
                _controller.SimpleMove(Vector3.zero);
            }
        }

        private void UpdateAutomaticAttack()
        {
            var weapon = _tuning.startingWeapon;
            if (weapon == null)
            {
                CurrentTarget = null;
                return;
            }

            CurrentTarget = _combatWorld.FindClosestTarget(
                transform.position,
                weapon.attackRange);

            if (_attackInProgress)
            {
                _windupRemaining -= Time.deltaTime;
                if (_windupRemaining <= 0f)
                {
                    ResolveAttack();
                }
            }

            if (_attackInProgress ||
                CurrentTarget == null ||
                Time.time < _nextAttackStartTime)
            {
                return;
            }

            _attackTarget = CurrentTarget;
            _attackInProgress = true;
            _windupRemaining = weapon.windupSeconds;
            _nextAttackStartTime = Time.time + weapon.attackCycleSeconds;
            FaceTarget(_attackTarget.transform.position);
            if (_weaponPivot != null)
            {
                _weaponPivot.localRotation = Quaternion.Euler(0f, 0f, -55f);
            }

            if (_windupRemaining <= 0f)
            {
                ResolveAttack();
            }
        }

        private void ResolveAttack()
        {
            var weapon = _tuning.startingWeapon;
            _attackInProgress = false;
            if (_weaponPivot != null)
            {
                _weaponPivot.localRotation = Quaternion.identity;
            }

            if (_attackTarget != null &&
                _attackTarget.IsAlive &&
                HorizontalDistance(transform.position, _attackTarget.transform.position) <= weapon.attackRange)
            {
                var damage = (float)ProgressionRules.CalculateAxeDamage(
                    _tuning.baseAxeDamage,
                    weapon.weaponDamage,
                    GetAxeAttackPoints(),
                    _tuning.axeDamagePerPoint);
                _attackTarget.TakeDamage(damage);
            }

            _attackTarget = null;
        }

        private int GetAxeAttackPoints()
        {
            return _combatSession.AxeAttackPoints;
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void CreateGrayboxModel()
        {
            _controller.height = 2f;
            _controller.radius = 0.38f;
            _controller.center = new Vector3(0f, 1f, 0f);
            _controller.minMoveDistance = 0f;
            _controller.skinWidth = 0.04f;

            CreateVisualPrimitive(
                PrimitiveType.Capsule,
                "Hero body",
                new Vector3(0f, 1f, 0f),
                new Vector3(0.72f, 1f, 0.72f),
                new Color(0.18f, 0.38f, 0.76f));
            CreateVisualPrimitive(
                PrimitiveType.Sphere,
                "Hero head",
                new Vector3(0f, 2.12f, 0f),
                new Vector3(0.48f, 0.48f, 0.48f),
                new Color(0.92f, 0.72f, 0.55f));

            var weapon = CreateVisualPrimitive(
                PrimitiveType.Cube,
                "Basic axe",
                new Vector3(0.53f, 1.05f, 0.35f),
                new Vector3(0.12f, 0.12f, 0.75f),
                new Color(0.34f, 0.22f, 0.12f));
            _weaponPivot = weapon.transform;
            CreateVisualPrimitive(
                PrimitiveType.Cube,
                "Axe head",
                new Vector3(0f, 0.28f, 0f),
                new Vector3(0.48f, 0.22f, 0.2f),
                new Color(0.75f, 0.78f, 0.82f),
                weapon.transform);
        }

        private void CreateArmorVisuals()
        {
            if (_tuning == null)
            {
                return;
            }

            if (_tuning.headArmor != null)
            {
                CreateVisualPrimitive(
                    PrimitiveType.Sphere,
                    "Head armor",
                    new Vector3(0f, 2.27f, 0f),
                    new Vector3(0.52f, 0.24f, 0.52f),
                    _tuning.headArmor.grayboxColor);
            }

            if (_tuning.torsoArmor != null)
            {
                CreateVisualPrimitive(
                    PrimitiveType.Cube,
                    "Torso armor",
                    new Vector3(0f, 1.12f, 0.34f),
                    new Vector3(0.63f, 0.62f, 0.14f),
                    _tuning.torsoArmor.grayboxColor);
            }
        }

        private GameObject CreateVisualPrimitive(
            PrimitiveType primitiveType,
            string objectName,
            Vector3 localPosition,
            Vector3 localScale,
            Color color,
            Transform parent = null)
        {
            var visual = GameObject.CreatePrimitive(primitiveType);
            visual.name = objectName;
            visual.transform.SetParent(parent != null ? parent : transform, false);
            visual.transform.localPosition = localPosition;
            visual.transform.localScale = localScale;

            var collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            var renderer = visual.GetComponent<Renderer>();
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (renderer != null && shader != null)
            {
                var material = new Material(shader) { color = color };
                renderer.sharedMaterial = material;
                _ownedMaterials.Add(material);
            }

            return visual;
        }

        private static float HorizontalDistance(Vector3 left, Vector3 right)
        {
            var deltaX = left.x - right.x;
            var deltaZ = left.z - right.z;
            return Mathf.Sqrt((deltaX * deltaX) + (deltaZ * deltaZ));
        }

        private void OnDestroy()
        {
            foreach (var material in _ownedMaterials)
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }
    }
}
