using GrindHero.Domain.Combat;
using UnityEngine;

namespace GrindHero.Presentation.Definitions
{
    public enum ArmorSlot
    {
        Head,
        Torso
    }

    [CreateAssetMenu(menuName = "GrindHero/Armor Definition")]
    public sealed class ArmorDefinition : ScriptableObject
    {
        public string stableId = "armor.head.basic";
        public ArmorSlot slot;
        public DamageType protectsAgainst = DamageType.Axe;
        [Min(0f)] public float defense;
        public Color grayboxColor = new Color(0.62f, 0.68f, 0.76f);

        public string ValidateDefinition()
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return "Armor stable ID is required.";
            }

            if (defense < 0f || float.IsNaN(defense) || float.IsInfinity(defense))
            {
                return $"Armor '{stableId}' needs finite non-negative defense.";
            }

            return null;
        }
    }
}
