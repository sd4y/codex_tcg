using System;
using System.Collections.Generic;
using UnityEngine;

public enum CardEffectType
{
    Damage,
    Block,
    Draw,
    GainEnergy,
    ApplyStatus
}

[Serializable]
public class CardEffect
{
    [SerializeField] private CardEffectType effectType = CardEffectType.Damage;
    [SerializeField] private int value = 1;
    [SerializeField] private string statusId = string.Empty;
    [SerializeField] private int duration = 0;
    [SerializeField] private bool targetsAllEnemies = false;

    public CardEffectType EffectType => effectType;
    public int Value => value;
    public string StatusId => statusId;
    public int Duration => duration;
    public bool TargetsAllEnemies => targetsAllEnemies;

    public void Apply(CharacterCombatant source, CharacterCombatant primaryTarget, IEnumerable<CharacterCombatant> enemies,
        DeckManager deckManager, TurnManager turnManager)
    {
        switch (effectType)
        {
            case CardEffectType.Damage:
                if (targetsAllEnemies)
                {
                    foreach (var enemy in enemies)
                    {
                        enemy.ApplyDamage(value);
                    }
                }
                else
                {
                    primaryTarget.ApplyDamage(value);
                }
                break;
            case CardEffectType.Block:
                source.GainBlock(value);
                break;
            case CardEffectType.Draw:
                deckManager.DrawCards(value);
                break;
            case CardEffectType.GainEnergy:
                turnManager.GainEnergy(value);
                break;
            case CardEffectType.ApplyStatus:
                if (targetsAllEnemies)
                {
                    foreach (var enemy in enemies)
                    {
                        enemy.ApplyStatus(statusId, value, duration);
                    }
                }
                else
                {
                    primaryTarget.ApplyStatus(statusId, value, duration);
                }
                break;
            default:
                Debug.LogWarning($"Unhandled card effect type: {effectType}");
                break;
        }
    }
}
