using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private PlayerCharacter player;
    [SerializeField] private List<EnemyCharacter> enemies = new();
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private HandView handView;
    [SerializeField] private CharacterCombatant targetOverride;
    [Header("UI")]
    [SerializeField] private CombatantStatusView playerStatusView;
    [SerializeField] private List<CombatantStatusView> enemyStatusViews = new();

    private void Start()
    {
        BeginBattle();
    }

    public void BeginBattle()
    {
        if (player != null)
        {
            player.ResetForBattle();
            playerStatusView?.Bind(player);
        }

        foreach (var enemy in enemies)
        {
            enemy.ResetForBattle();
        }

        BindEnemyStatusViews();

        deckManager.InitializeDeck(deckManager.StarterDeck);
        turnManager.BeginBattle(deckManager);

        handView?.Initialize(deckManager, this, GetDefaultTarget());
    }

    public void OnPlayerUseCard(CardInstance card, CharacterCombatant target)
    {
        if (turnManager.CurrentPhase != BattlePhase.PlayerTurn)
        {
            return;
        }

        var chosenTarget = target != null ? target : GetDefaultTarget();
        if (chosenTarget == null)
        {
            Debug.LogWarning("No valid target to play the card on.");
            return;
        }

        PlayCard(card, chosenTarget);
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

    private void BindEnemyStatusViews()
    {
        int count = Mathf.Min(enemyStatusViews.Count, enemies.Count);
        for (int i = 0; i < count; i++)
        {
            enemyStatusViews[i].Bind(enemies[i]);
        }
    }

    private CharacterCombatant GetDefaultTarget()
    {
        if (targetOverride != null)
        {
            return targetOverride;
        }

        return enemies.FirstOrDefault(enemy => enemy.CurrentHealth > 0);
    }
}
