using UnityEngine;

public static class CombatMath
{
    private const string StrengthStatus = "strength";
    private const string WeakStatus = "weak";
    private const string VulnerableStatus = "vulnerable";
    private const string FrailStatus = "frail";

    public static int CalculateDamage(CharacterCombatant source, CharacterCombatant target, int baseDamage)
    {
        if (source != null)
        {
            baseDamage += source.GetStatusStacks(StrengthStatus);

            if (source.HasStatus(WeakStatus))
            {
                baseDamage = Mathf.FloorToInt(baseDamage * 0.75f);
            }
        }

        if (target != null && target.HasStatus(VulnerableStatus))
        {
            baseDamage = Mathf.CeilToInt(baseDamage * 1.5f);
        }

        return Mathf.Max(0, baseDamage);
    }

    public static int CalculateBlockGain(CharacterCombatant combatant, int baseBlock)
    {
        if (combatant != null && combatant.HasStatus(FrailStatus))
        {
            baseBlock = Mathf.FloorToInt(baseBlock * 0.75f);
        }

        return Mathf.Max(0, baseBlock);
    }
}
