using System;
using System.Collections.Generic;
using GrindHero.Application.Combat;
using GrindHero.Presentation.Definitions;
using GrindHero.Presentation.World;
using UnityEngine;

namespace GrindHero.Presentation
{
    public sealed class EnemyActor : MonoBehaviour
    {
        private enum BehaviorState
        {
            Idle,
            Chasing,
            Engaged,
            Returning
        }

        private readonly List<Material> _ownedMaterials = new List<Material>();
        private EnemyDefinition _definition;
        private Transform _hero;
        private Transform _spawnTransform;
        private ICombatSession _combatSession;
        private CombatWorld _combatWorld;
        private EnemySpawnPoint _spawnPoint;
        private Renderer _bodyRenderer;
        private Color _baseColor;
        private BehaviorState _state;
        private float _nextAttackStartTime;
        private float _windupRemaining;
        private float _chaseRemaining;
        private bool _attackInProgress;
        private float _flashRemaining;

        public bool IsAlive { get; private set; }
        public float CurrentHealth { get; private set; }
        public float MaximumHealth => _definition != null ? _definition.maximumHealth : 0f;
        public string DisplayName => _definition != null ? _definition.displayName : name;
        public EnemyDefinition Definition => _definition;

        public void Initialize(
            EnemyDefinition definition,
            Transform hero,
            Transform spawnTransform,
            ICombatSession combatSession,
            CombatWorld combatWorld,
            EnemySpawnPoint spawnPoint)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var validationError = definition.ValidateDefinition();
            if (validationError != null)
            {
                throw new InvalidOperationException(validationError);
            }

            _definition = definition;
            _hero = hero;
            _spawnTransform = spawnTransform;
            _combatSession = combatSession;
            _combatWorld = combatWorld;
            _spawnPoint = spawnPoint;
            CurrentHealth = definition.maximumHealth;
            IsAlive = true;
            _state = BehaviorState.Idle;
            _nextAttackStartTime = Time.time;
            _baseColor = definition.grayboxColor;
            CreateGrayboxModel();
            _combatWorld.Register(this);
        }

        public void TakeDamage(float damage)
        {
            if (!IsAlive || damage <= 0f || float.IsNaN(damage) || float.IsInfinity(damage))
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            _flashRemaining = 0.12f;
            SetBodyColor(Color.white);

            var distanceToHero = HorizontalDistance(transform.position, _hero.position);
            if (_state == BehaviorState.Chasing || _state == BehaviorState.Returning ||
                _state == BehaviorState.Idle)
            {
                _chaseRemaining = _definition.chaseTimeoutSeconds;
                _state = distanceToHero <= _definition.attackRange
                    ? BehaviorState.Engaged
                    : BehaviorState.Chasing;
            }

            if (CurrentHealth <= 0f)
            {
                Defeat();
            }
        }

        private void Update()
        {
            if (!IsAlive || _definition == null || _hero == null)
            {
                return;
            }

            var deltaTime = Time.deltaTime;
            if (_flashRemaining > 0f)
            {
                _flashRemaining -= deltaTime;
                if (_flashRemaining <= 0f && !_attackInProgress)
                {
                    SetBodyColor(_baseColor);
                }
            }

            var distanceToHero = HorizontalDistance(transform.position, _hero.position);
            switch (_state)
            {
                case BehaviorState.Idle:
                    UpdateIdle(distanceToHero);
                    break;
                case BehaviorState.Chasing:
                    UpdateChasing(distanceToHero, deltaTime);
                    break;
                case BehaviorState.Engaged:
                    UpdateEngaged(distanceToHero, deltaTime);
                    break;
                case BehaviorState.Returning:
                    UpdateReturning(deltaTime);
                    break;
            }

            if (_state != BehaviorState.Engaged && _attackInProgress)
            {
                CancelAttackTelegraph();
            }
        }

        private void UpdateIdle(float distanceToHero)
        {
            if (distanceToHero > _definition.aggroRange)
            {
                return;
            }

            if (distanceToHero <= _definition.attackRange)
            {
                _state = BehaviorState.Engaged;
                return;
            }

            BeginChase();
        }

        private void UpdateChasing(float distanceToHero, float deltaTime)
        {
            if (distanceToHero <= _definition.attackRange)
            {
                _state = BehaviorState.Engaged;
                return;
            }

            if (_definition.returnAndHeal)
            {
                _chaseRemaining -= deltaTime;
                if (_chaseRemaining <= 0f)
                {
                    _state = BehaviorState.Returning;
                    return;
                }
            }

            MoveToward(_hero.position, deltaTime);
        }

        private void UpdateEngaged(float distanceToHero, float deltaTime)
        {
            if (distanceToHero > _definition.attackRange)
            {
                BeginChase();
                return;
            }

            FacePosition(_hero.position);
            UpdateAttack(deltaTime);
        }

        private void UpdateReturning(float deltaTime)
        {
            MoveToward(_spawnTransform.position, deltaTime);
            CurrentHealth = Mathf.Min(
                _definition.maximumHealth,
                CurrentHealth +
                (_definition.maximumHealth *
                 _definition.returnHealFractionPerSecond *
                 deltaTime));

            if (HorizontalDistance(transform.position, _spawnTransform.position) <= 0.08f)
            {
                transform.position = _spawnTransform.position;
                _state = BehaviorState.Idle;
                SetBodyColor(_baseColor);
            }
        }

        private void UpdateAttack(float deltaTime)
        {
            if (_attackInProgress)
            {
                _windupRemaining -= deltaTime;
                if (_windupRemaining <= 0f)
                {
                    _attackInProgress = false;
                    SetBodyColor(_baseColor);

                    if (HorizontalDistance(transform.position, _hero.position) <= _definition.attackRange)
                    {
                        _combatSession.ReceiveHeroDamage(
                            _definition.attackDamage,
                            _definition.attackDamageType);
                    }
                }
            }

            if (_attackInProgress || Time.time < _nextAttackStartTime)
            {
                return;
            }

            _attackInProgress = true;
            _windupRemaining = _definition.windupSeconds;
            _nextAttackStartTime = Time.time + _definition.attackCycleSeconds;
            SetBodyColor(Color.Lerp(_baseColor, new Color(1f, 0.35f, 0.12f), 0.8f));

            if (_windupRemaining <= 0f)
            {
                UpdateAttack(0f);
            }
        }

        private void BeginChase()
        {
            _state = BehaviorState.Chasing;
            _chaseRemaining = _definition.chaseTimeoutSeconds;
        }

        private void MoveToward(Vector3 targetPosition, float deltaTime)
        {
            targetPosition.y = transform.position.y;
            var direction = targetPosition - transform.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            FacePosition(targetPosition);
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                _definition.movementSpeed * deltaTime);
        }

        private void FacePosition(Vector3 targetPosition)
        {
            var direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }

        private void CancelAttackTelegraph()
        {
            _attackInProgress = false;
            _windupRemaining = 0f;
            SetBodyColor(_baseColor);
        }

        private void Defeat()
        {
            IsAlive = false;
            _combatWorld.Unregister(this);
            CancelAttackTelegraph();
            _spawnPoint.HandleEnemyDefeated(this);
            Destroy(gameObject);
        }

        private void CreateGrayboxModel()
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Enemy body";
            body.transform.SetParent(transform, false);
            body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            body.transform.localScale = new Vector3(0.78f, 0.95f, 0.78f);
            RemoveCollider(body);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Enemy head";
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0f, 2.02f, 0f);
            head.transform.localScale = new Vector3(0.48f, 0.48f, 0.48f);
            RemoveCollider(head);

            _bodyRenderer = body.GetComponent<Renderer>();
            ApplyColor(_bodyRenderer, _baseColor);
            ApplyColor(head.GetComponent<Renderer>(), Color.Lerp(_baseColor, Color.black, 0.2f));
        }

        private void ApplyColor(Renderer renderer, Color color)
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

            if (shader == null)
            {
                return;
            }

            var material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
            _ownedMaterials.Add(material);
        }

        private void SetBodyColor(Color color)
        {
            if (_bodyRenderer != null && _bodyRenderer.sharedMaterial != null)
            {
                _bodyRenderer.sharedMaterial.color = color;
            }
        }

        private static void RemoveCollider(GameObject visual)
        {
            var collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
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
