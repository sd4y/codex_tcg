using System.Collections.Generic;
using UnityEngine;

public enum CardType
{
    Attack,
    Skill,
    Power,
    Status,
    Curse
}

public enum CardTarget
{
    Self,
    SingleEnemy,
    AllEnemies,
    None
}

public enum CardRarity
{
    Common,
    Uncommon,
    Rare
}

[CreateAssetMenu(menuName = "TCG/Card Data")]
public class CardData : ScriptableObject
{
    [SerializeField] private string cardId = "card_id";
    [SerializeField] private string displayName = "New Card";
    [TextArea]
    [SerializeField] private string description = "";
    [SerializeField] private int cost = 1;
    [SerializeField] private CardType cardType = CardType.Attack;
    [SerializeField] private CardTarget target = CardTarget.SingleEnemy;
    [SerializeField] private CardRarity rarity = CardRarity.Common;
    [SerializeField] private bool exhaustAfterPlay = false;
    [SerializeField] private List<CardEffect> effects = new();

    public string CardId => cardId;
    public string DisplayName => displayName;
    public string Description => description;
    public int Cost => cost;
    public CardType CardType => cardType;
    public CardTarget Target => target;
    public CardRarity Rarity => rarity;
    public bool ExhaustAfterPlay => exhaustAfterPlay;
    public IReadOnlyList<CardEffect> Effects => effects;

    public CardInstance CreateInstance()
    {
        return new CardInstance(this);
    }
}
