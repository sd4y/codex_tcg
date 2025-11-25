using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [Header("Card UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image artworkImage;
    [SerializeField] private Button button;

    private CardInstance cardInstance;
    private BattleManager battleManager;
    private CharacterCombatant target;

    public void Initialize(CardInstance card, BattleManager manager, CharacterCombatant initialTarget)
    {
        cardInstance = card;
        battleManager = manager;
        target = initialTarget;

        if (cardInstance != null)
        {
            nameText?.SetText(cardInstance.Data.DisplayName);
            costText?.SetText(cardInstance.CurrentCost.ToString());
            descriptionText?.SetText(cardInstance.Data.Description);

            if (artworkImage != null)
            {
                artworkImage.sprite = cardInstance.Data.Artwork;
                artworkImage.enabled = artworkImage.sprite != null;
            }
        }

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickCard);
        }
    }

    public void SetTarget(CharacterCombatant newTarget)
    {
        target = newTarget;
    }

    private void OnClickCard()
    {
        if (cardInstance == null || battleManager == null)
        {
            return;
        }

        battleManager.OnPlayerUseCard(cardInstance, target);
    }
}
