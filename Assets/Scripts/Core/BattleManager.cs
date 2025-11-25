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
    [SerializeField] private List<EnemyIntentView> enemyIntentViews = new();
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    private CardInstance pendingTargetedCard;
    private bool awaitingTarget;

    private void Start()
    {
        BeginBattle();
    }

    public void BeginBattle()
    {
        HideResultPanels();

        if (player != null)
        {
            player.ResetForBattle();
            playerStatusView?.Bind(player);
        }

        foreach (var enemy in enemies)
        {
            enemy.ResetForBattle();
            var ai = enemy.GetComponent<EnemyAI>();
            ai?.DetermineNextMove();
        }

        BindEnemyStatusViews();
        BindEnemyIntents();

        handView?.Initialize(deckManager, this, GetDefaultTarget());

        deckManager.InitializeDeck(deckManager.StarterDeck);
        turnManager.BeginBattle(deckManager);

        player.TickStatuses(StatusTickPhase.StartOfTurn);
    }

    public void OnPlayerUseCard(CardInstance card, CharacterCombatant target)
    {
        if (turnManager.CurrentPhase != BattlePhase.PlayerTurn)
        {
            return;
        }

        switch (card.Data.Target)
        {
            case CardTarget.Self:
                PlayCard(card, player);
                break;
            case CardTarget.AllEnemies:
                PlayCard(card, GetDefaultTarget());
                break;
            case CardTarget.RandomEnemy:
                var randomTarget = GetRandomTarget();
                if (randomTarget != null)
                {
                    PlayCard(card, randomTarget);
                }
                break;
            case CardTarget.SingleEnemy:
                var liveEnemies = enemies.Where(e => e.CurrentHealth > 0).ToList();
                if (liveEnemies.Count == 1)
                {
                    PlayCard(card, liveEnemies[0]);
                }
                else
                {
                    BeginTargetSelection(card);
                }
                break;
            default:
                PlayCard(card, target ?? GetDefaultTarget());
                break;
        }
    }

    public void PlayCard(CardInstance card, CharacterCombatant target)
    {
        if (!deckManager.RemoveFromHand(card))
        {
            Debug.LogWarning("Tried to play a card that is not in the hand");
            return;
        }

        ClearTargeting();

        if (!turnManager.TrySpendEnergy(card.CurrentCost))
        {
            Debug.Log("Not enough energy to play card");
            deckManager.ReturnCardToHand(card);
            return;
        }

        ResolveCard(card, target);

        if (CheckBattleEnd())
        {
            return;
        }

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
                case CardTarget.RandomEnemy:
                    var randomEnemy = GetRandomTarget();
                    effect.Apply(player, randomEnemy, enemies, deckManager, turnManager);
                    break;
                default:
                    effect.Apply(player, target, enemies, deckManager, turnManager);
                    break;
            }
        }
    }

    public void EndPlayerTurn()
    {
        if (turnManager.CurrentPhase != BattlePhase.PlayerTurn)
        {
            return;
        }

        ClearTargeting();
        player.TickStatuses(StatusTickPhase.EndOfTurn);
        deckManager.ResetHandsBetweenTurns();

        if (CheckBattleEnd())
        {
            return;
        }

        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        if (CheckBattleEnd())
        {
            return;
        }

        turnManager.StartEnemyTurn();

        foreach (var enemy in enemies)
        {
            if (enemy.CurrentHealth <= 0)
            {
                continue;
            }

            enemy.TickStatuses(StatusTickPhase.StartOfTurn);
            var ai = enemy.GetComponent<EnemyAI>();
            ai?.ExecuteMove(enemy, player, enemies, deckManager, turnManager);
            ai?.DetermineNextMove();
            RefreshIntentForEnemy(enemy);
            enemy.TickStatuses(StatusTickPhase.EndOfTurn);

            if (CheckBattleEnd())
            {
                break;
            }
        }

        if (!CheckBattleEnd())
        {
            StartPlayerTurn();
        }
    }

    private void StartPlayerTurn()
    {
        if (CheckBattleEnd())
        {
            return;
        }

        turnManager.StartPlayerTurn(deckManager);
        player.TickStatuses(StatusTickPhase.StartOfTurn);
        handView?.SetTarget(GetDefaultTarget());
        ClearTargeting();
        RefreshAllIntents();
    }

    private void BindEnemyStatusViews()
    {
        int count = Mathf.Min(enemyStatusViews.Count, enemies.Count);
        for (int i = 0; i < count; i++)
        {
            enemyStatusViews[i].Clicked -= OnEnemyStatusClicked;
            enemyStatusViews[i].Bind(enemies[i]);
            enemyStatusViews[i].Clicked += OnEnemyStatusClicked;
        }
    }

    private void BindEnemyIntents()
    {
        int count = Mathf.Min(enemyIntentViews.Count, enemies.Count);
        for (int i = 0; i < count; i++)
        {
            var intentView = enemyIntentViews[i];
            var ai = enemies[i].GetComponent<EnemyAI>();
            if (ai != null)
            {
                intentView?.Bind(ai);
            }
        }
    }

    private void RefreshAllIntents()
    {
        foreach (var enemy in enemies)
        {
            RefreshIntentForEnemy(enemy);
        }
    }

    private void RefreshIntentForEnemy(EnemyCharacter enemy)
    {
        int index = enemies.IndexOf(enemy);
        if (index < 0 || index >= enemyIntentViews.Count)
        {
            return;
        }

        enemyIntentViews[index]?.Refresh();
    }

    private CharacterCombatant GetDefaultTarget()
    {
        if (targetOverride != null)
        {
            return targetOverride;
        }

        return enemies.FirstOrDefault(enemy => enemy.CurrentHealth > 0);
    }

    private CharacterCombatant GetRandomTarget()
    {
        var alive = enemies.Where(e => e.CurrentHealth > 0).ToList();
        if (alive.Count == 0)
        {
            return null;
        }

        return alive[Random.Range(0, alive.Count)];
    }

    private void BeginTargetSelection(CardInstance card)
    {
        pendingTargetedCard = card;
        awaitingTarget = true;
        HighlightEnemies(true);
    }

    private void ClearTargeting()
    {
        awaitingTarget = false;
        pendingTargetedCard = null;
        HighlightEnemies(false);
    }

    private void HighlightEnemies(bool highlighted)
    {
        foreach (var view in enemyStatusViews)
        {
            if (view == null)
            {
                continue;
            }

            bool shouldHighlight = highlighted;
            if (highlighted && view.BoundCombatant != null)
            {
                shouldHighlight = view.BoundCombatant.CurrentHealth > 0;
            }

            view.SetHighlighted(shouldHighlight);
        }
    }

    private void OnEnemyStatusClicked(CharacterCombatant combatant)
    {
        if (!awaitingTarget || pendingTargetedCard == null)
        {
            return;
        }

        PlayCard(pendingTargetedCard, combatant);
    }

    private bool CheckBattleEnd()
    {
        if (turnManager.CurrentPhase == BattlePhase.Victory || turnManager.CurrentPhase == BattlePhase.Defeat)
        {
            return true;
        }

        if (player != null && player.CurrentHealth <= 0)
        {
            EndBattle(BattlePhase.Defeat);
            return true;
        }

        if (enemies.All(enemy => enemy.CurrentHealth <= 0))
        {
            EndBattle(BattlePhase.Victory);
            return true;
        }

        return false;
    }

    private void EndBattle(BattlePhase result)
    {
        ClearTargeting();
        turnManager.SetBattleResult(result);
        victoryPanel?.SetActive(result == BattlePhase.Victory);
        defeatPanel?.SetActive(result == BattlePhase.Defeat);
    }

    private void HideResultPanels()
    {
        victoryPanel?.SetActive(false);
        defeatPanel?.SetActive(false);
    }
}
