using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatantStatusView : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text blockText;
    [SerializeField] private Slider healthSlider;

    private CharacterCombatant boundCombatant;

    public void Bind(CharacterCombatant combatant)
    {
        if (boundCombatant != null)
        {
            boundCombatant.StatsChanged -= Refresh;
        }

        boundCombatant = combatant;

        if (boundCombatant != null)
        {
            boundCombatant.StatsChanged += Refresh;
        }

        Refresh(boundCombatant);
    }

    private void OnDisable()
    {
        if (boundCombatant != null)
        {
            boundCombatant.StatsChanged -= Refresh;
        }
    }

    private void Refresh(CharacterCombatant combatant)
    {
        if (combatant == null)
        {
            return;
        }

        if (healthText != null)
        {
            healthText.SetText($"{combatant.CurrentHealth}/{combatant.MaxHealth}");
        }

        if (blockText != null)
        {
            blockText.SetText($"Block: {combatant.Block}");
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = combatant.MaxHealth;
            healthSlider.value = combatant.CurrentHealth;
        }
    }
}
