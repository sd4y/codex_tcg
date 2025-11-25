using UnityEngine;

public enum BattlePhase
{
    PlayerTurn,
    EnemyTurn,
    Victory,
    Defeat
}

public class TurnManager : MonoBehaviour
{
    [SerializeField] private int baseEnergy = 3;
    [SerializeField] private int cardsPerTurn = 5;

    public int CurrentEnergy { get; private set; }
    public BattlePhase CurrentPhase { get; private set; } = BattlePhase.PlayerTurn;

    public void BeginBattle(DeckManager deckManager)
    {
        CurrentPhase = BattlePhase.PlayerTurn;
        deckManager.DrawStartingHand();
        ResetEnergy();
    }

    public void StartPlayerTurn(DeckManager deckManager)
    {
        CurrentPhase = BattlePhase.PlayerTurn;
        ResetEnergy();
        deckManager.DrawCards(cardsPerTurn);
    }

    public void StartEnemyTurn()
    {
        CurrentPhase = BattlePhase.EnemyTurn;
    }

    public void SetBattleResult(BattlePhase result)
    {
        CurrentPhase = result;
        CurrentEnergy = 0;
    }

    public bool TrySpendEnergy(int cost)
    {
        if (CurrentEnergy < cost)
        {
            return false;
        }

        CurrentEnergy -= cost;
        return true;
    }

    public void GainEnergy(int amount)
    {
        CurrentEnergy += Mathf.Max(0, amount);
    }

    private void ResetEnergy()
    {
        CurrentEnergy = baseEnergy;
    }
}
