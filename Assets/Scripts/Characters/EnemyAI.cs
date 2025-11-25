using System;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyIntentType
{
    Attack,
    Defend,
    Buff,
    Debuff,
    Unknown
}

[Serializable]
public class EnemyMove
{
    [SerializeField] private string moveId = "attack";
    [SerializeField] private EnemyIntentType intent = EnemyIntentType.Attack;
    [SerializeField] private string description = "";
    [SerializeField] private int power = 5;
    [SerializeField] private CardTarget target = CardTarget.SingleEnemy;
    [SerializeField] private List<CardEffect> effects = new();
    [SerializeField] private Sprite icon;

    public string MoveId => moveId;
    public EnemyIntentType Intent => intent;
    public string Description => string.IsNullOrEmpty(description) ? intent.ToString() : description;
    public int Power => power;
    public CardTarget Target => target;
    public IReadOnlyList<CardEffect> Effects => effects;
    public Sprite Icon => icon;
}

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private List<EnemyMove> possibleMoves = new();
    [SerializeField] private bool chooseSequentially = false;

    private int moveIndex = 0;

    public EnemyMove NextMove { get; private set; }

    public void DetermineNextMove()
    {
        if (possibleMoves.Count == 0)
        {
            NextMove = null;
            return;
        }

        if (chooseSequentially)
        {
            NextMove = possibleMoves[moveIndex % possibleMoves.Count];
            moveIndex++;
        }
        else
        {
            NextMove = possibleMoves[UnityEngine.Random.Range(0, possibleMoves.Count)];
        }
    }

    public void ExecuteMove(CharacterCombatant self, CharacterCombatant player, IEnumerable<EnemyCharacter> allEnemies,
        DeckManager deckManager, TurnManager turnManager)
    {
        if (NextMove == null)
        {
            DetermineNextMove();
        }

        if (NextMove == null)
        {
            return;
        }

        var target = GetTargetForMove(NextMove, player, allEnemies);
        foreach (var effect in NextMove.Effects)
        {
            effect.Apply(self, target, allEnemies, deckManager, turnManager);
        }

        DetermineNextMove();
    }

    private CharacterCombatant GetTargetForMove(EnemyMove move, CharacterCombatant player, IEnumerable<EnemyCharacter> enemies)
    {
        switch (move.Target)
        {
            case CardTarget.Self:
                return GetComponent<CharacterCombatant>();
            case CardTarget.AllEnemies:
            case CardTarget.SingleEnemy:
            case CardTarget.RandomEnemy:
                return player;
            default:
                return player;
        }
    }
}
