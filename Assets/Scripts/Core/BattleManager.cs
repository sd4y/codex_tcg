using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private PlayerCharacter player;
    [SerializeField] private List<EnemyCharacter> enemies = new();
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private TurnManager turnManager;

    private void Start()
    {
        BeginBattle();
    }

    public void BeginBattle()
    {
        if (player != null)
        {
            player.ResetForBattle();
        }

        foreach (var enemy in enemies)
        {
            enemy.ResetForBattle();
        }

        deckManager.InitializeDeck(deckManager.StarterDeck);
        turnManager.BeginBattle(deckManager);
    }

    public void PlayCard(CardInstance card, CharacterCombatant target)
    {
        if (!deckManager.RemoveFromHand(card))
        {
            Debug.LogWarning("Tried to play a card that is not in the hand");
            return;
        }

        if (!turnManager.TrySpendEnergy(card.CurrentCost))
        {
            Debug.Log("Not enough energy to play card");
            deckManager.ReturnCardToHand(card);
            return;
        }

        ResolveCard(card, target);

        if (card.Data.ExhaustAfterPlay)
        {
            deckManager.Exhaust(card);
        }
        else
        {
            deckManager.Discard(card);
        }
    }

    private void ResolveCard(CardInstance card, CharacterCombatant target)
    {
        foreach (var effect in card.Data.Effects)
        {
            switch (card.Data.Target)
            {
                case CardTarget.Self:
                    effect.Apply(player, player, enemies, deckManager, turnManager);
                    break;
                case CardTarget.SingleEnemy:
                    effect.Apply(player, target, enemies, deckManager, turnManager);
                    break;
                case CardTarget.AllEnemies:
                    effect.Apply(player, target, enemies, deckManager, turnManager);
                    break;
                default:
                    effect.Apply(player, target, enemies, deckManager, turnManager);
                    break;
            }
        }
    }
}
