using System.Collections.Generic;
using GrindHero.Presentation.Definitions;
using UnityEngine;

namespace GrindHero.Presentation.World
{
    public sealed class CombatWorld : MonoBehaviour
    {
        private readonly List<EnemyActor> _enemies = new List<EnemyActor>();

        public int AliveEnemyCount => _enemies.Count;

        public void Register(EnemyActor enemy)
        {
            if (enemy != null && !_enemies.Contains(enemy))
            {
                _enemies.Add(enemy);
            }
        }

        public void Unregister(EnemyActor enemy)
        {
            _enemies.Remove(enemy);
        }

        public EnemyActor FindClosestTarget(Vector3 position, float range)
        {
            EnemyActor closest = null;
            var closestDistanceSquared = range * range;

            for (var i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    _enemies.RemoveAt(i);
                    continue;
                }

                var distanceSquared = HorizontalDistanceSquared(position, enemy.transform.position);
                if (distanceSquared <= closestDistanceSquared)
                {
                    closest = enemy;
                    closestDistanceSquared = distanceSquared;
                }
            }

            return closest;
        }

        public EnemyActor FindClosestTargetInRange(Vector3 position, float range)
        {
            return FindClosestTarget(position, range);
        }

        private static float HorizontalDistanceSquared(Vector3 left, Vector3 right)
        {
            var deltaX = left.x - right.x;
            var deltaZ = left.z - right.z;
            return (deltaX * deltaX) + (deltaZ * deltaZ);
        }
    }
}
