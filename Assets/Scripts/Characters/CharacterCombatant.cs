using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombatant : MonoBehaviour
{
    [SerializeField] private string combatantId = "character";
    [SerializeField] private int maxHealth = 80;
    [SerializeField] private int startingBlock = 0;

    private readonly Dictionary<string, StatusEffect> statuses = new();

    public string CombatantId => combatantId;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public int Block { get; private set; }
    public IReadOnlyDictionary<string, StatusEffect> Statuses => statuses;

    public event Action<CharacterCombatant> StatsChanged;

    private void Awake()
    {
        ResetForBattle();
    }

    public void ResetForBattle()
    {
        CurrentHealth = maxHealth;
        Block = startingBlock;
        statuses.Clear();

        NotifyStatsChanged();
    }

    public void ApplyDamage(int amount)
    {
        var remainingDamage = Mathf.Max(0, amount - Block);
        Block = Mathf.Max(0, Block - amount);
        CurrentHealth = Mathf.Max(0, CurrentHealth - remainingDamage);

        NotifyStatsChanged();
    }

    public void GainBlock(int amount)
    {
        Block += Mathf.Max(0, amount);

        NotifyStatsChanged();
    }

    public void ApplyStatus(string statusId, int stackCount, int duration)
    {
        if (statuses.TryGetValue(statusId, out var currentStatus))
        {
            currentStatus.Stacks += stackCount;
            currentStatus.Duration = Mathf.Max(currentStatus.Duration, duration);
            statuses[statusId] = currentStatus;
        }
        else
        {
            statuses[statusId] = new StatusEffect(statusId, stackCount, duration);
        }

        NotifyStatsChanged();
    }

    private void NotifyStatsChanged()
    {
        StatsChanged?.Invoke(this);
    }
}

public struct StatusEffect
{
    public StatusEffect(string id, int stacks, int duration)
    {
        Id = id;
        Stacks = stacks;
        Duration = duration;
    }

    public string Id { get; }
    public int Stacks { get; set; }
    public int Duration { get; set; }
}
