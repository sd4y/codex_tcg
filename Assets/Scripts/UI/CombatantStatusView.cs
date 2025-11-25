using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatantStatusView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text blockText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject highlight;

    private CharacterCombatant boundCombatant;

    public event Action<CharacterCombatant> Clicked;

    public CharacterCombatant BoundCombatant => boundCombatant;

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
        SetHighlighted(false);
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

    public void SetHighlighted(bool isHighlighted)
    {
        if (highlight != null)
        {
            highlight.SetActive(isHighlighted);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (boundCombatant != null)
        {
            Clicked?.Invoke(boundCombatant);
        }
    }
}
