using GrindHero.Domain.Combat;
using UnityEngine;

namespace GrindHero.Presentation.Definitions
{
    [CreateAssetMenu(menuName = "GrindHero/First Slice Tuning")]
    public sealed class FirstSliceTuning : ScriptableObject
    {
        [Header("Hero")]
        [Min(1)] public int startingMaximumHealth = 100;
        [Min(0)] public int healthPerPoint = 5;
        [Min(0f)] public float baseAxeDamage = 15f;
        [Min(0f)] public float axeDamagePerPoint = 1f;
        [Min(0f)] public float movementSpeed = 4.5f;
        [Range(0f, 1f)] public float startingDodgeChance;
        [Range(0f, 1f)] public float startingBlockChance;
        [Range(0f, 1f)] public float blockDamageMultiplier = 0.8f;
        [Range(0.1f, 1f)] public float respawnHealthFraction = 0.5f;

        [Header("Equipment")]
        public WeaponDefinition startingWeapon;
        public ArmorDefinition headArmor;
        public ArmorDefinition torsoArmor;

        [Header("Prototype Tuning")]
        [Tooltip("Prototype maximum respawns per spawn point per UTC day; review with the game owner.")]
        [Min(1)] public int maximumMobRespawnsPerDay = 3;
        [Min(1f)] public float saveIntervalSeconds = 5f;

        public string ValidateDefinition()
        {
            if (startingMaximumHealth <= 0 ||
                healthPerPoint < 0 ||
                !IsFinite(baseAxeDamage) ||
                baseAxeDamage < 0f ||
                !IsFinite(axeDamagePerPoint) ||
                axeDamagePerPoint < 0f ||
                !IsFinite(movementSpeed) ||
                movementSpeed <= 0f)
            {
                return "Hero health, damage, and movement tuning must be finite and valid.";
            }

            if (!IsFinite(startingDodgeChance) ||
                startingDodgeChance < 0f ||
                startingDodgeChance > 1f ||
                !IsFinite(startingBlockChance) ||
                startingBlockChance < 0f ||
                startingBlockChance > 1f ||
                !IsFinite(blockDamageMultiplier) ||
                blockDamageMultiplier < 0f ||
                blockDamageMultiplier > 1f)
            {
                return "Dodge, block, and block damage tuning must be in the range 0-1.";
            }

            if (!IsFinite(respawnHealthFraction) ||
                respawnHealthFraction < 0f ||
                respawnHealthFraction > 1f ||
                maximumMobRespawnsPerDay < 1 ||
                !IsFinite(saveIntervalSeconds) ||
                saveIntervalSeconds <= 0f)
            {
                return "Respawn health, daily respawns, and save interval must be valid.";
            }

            if (startingWeapon == null)
            {
                return "A starting weapon is required.";
            }

            var weaponError = startingWeapon.ValidateDefinition();
            if (weaponError != null || startingWeapon.damageType != DamageType.Axe)
            {
                return weaponError ?? "The first-slice starting weapon must use Axe damage.";
            }

            var headError = ValidateArmor(headArmor, ArmorSlot.Head);
            if (headError != null)
            {
                return headError;
            }

            return ValidateArmor(torsoArmor, ArmorSlot.Torso);
        }

        private static string ValidateArmor(ArmorDefinition armor, ArmorSlot expectedSlot)
        {
            if (armor == null)
            {
                return null;
            }

            var error = armor.ValidateDefinition();
            if (error != null)
            {
                return error;
            }

            return armor.slot == expectedSlot
                ? null
                : $"Armor '{armor.stableId}' is assigned to the wrong equipment slot.";
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
