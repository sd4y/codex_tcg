using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatusTickPhase
{
    StartOfTurn,
    EndOfTurn
}

public class CharacterCombatant : MonoBehaviour
{
    [SerializeField] private string combatantId = "character";
    [SerializeField] private int maxHealth = 80;
    [SerializeField] private int startingBlock = 0;
    [SerializeField] private StatusLibrary statusLibrary;

    private readonly Dictionary<string, StatusInstance> statuses = new();

    public string CombatantId => combatantId;
    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public int Block { get; private set; }
    public IReadOnlyDictionary<string, StatusInstance> Statuses => statuses;

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
        var adjustedBlock = CombatMath.CalculateBlockGain(this, amount);
        Block += Mathf.Max(0, adjustedBlock);

        NotifyStatsChanged();
    }

    public void ApplyStatus(string statusId, int stackCount, int duration)
    {
        var definition = ResolveStatusDefinition(statusId);

        if (statuses.TryGetValue(statusId, out var currentStatus))
        {
            currentStatus.Stacks += stackCount;
            currentStatus.Stacks = Mathf.Max(0, currentStatus.Stacks);
            currentStatus.Duration = Mathf.Max(currentStatus.Duration, duration);
            statuses[statusId] = currentStatus;
        }
        else
        {
            statuses[statusId] = new StatusInstance(definition, Mathf.Max(0, stackCount), Mathf.Max(0, duration));
        }

        if (definition.RemoveWhenStacksDepleted && statuses.TryGetValue(statusId, out var checkStatus) && checkStatus.Stacks <= 0)
        {
            statuses.Remove(statusId);
            NotifyStatsChanged();
            return;
        }

        NotifyStatsChanged();
    }

    public void RemoveStatus(string statusId)
    {
        if (statuses.Remove(statusId))
        {
            NotifyStatsChanged();
        }
    }

    public bool HasStatus(string statusId)
    {
        if (!statuses.TryGetValue(statusId, out var status) || status.Stacks <= 0)
        {
            return false;
        }

        var definition = status.Definition ?? ResolveStatusDefinition(statusId);
        return !definition.ReduceDuration || status.Duration != 0;
    }

    public int GetStatusStacks(string statusId)
    {
        return statuses.TryGetValue(statusId, out var status) ? status.Stacks : 0;
    }

    public void TickStatuses(StatusTickPhase phase)
    {
        var expired = new List<string>();
        var changed = false;

        foreach (var kvp in new List<KeyValuePair<string, StatusInstance>>(statuses))
        {
            var status = kvp.Value;
            var definition = status.Definition ?? ResolveStatusDefinition(kvp.Key);

            if (definition.ShouldTick(phase) && status.Stacks > 0)
            {
                ApplyStatusTick(definition, status);
                changed = true;
            }

            if (status.Duration > 0 && definition.ShouldDecrementDuration(phase))
            {
                status.Duration -= 1;
                changed = true;
            }

            if ((definition.RemoveWhenStacksDepleted && status.Stacks <= 0) ||
                (status.Duration <= 0 && definition.ReduceDuration))
            {
                expired.Add(kvp.Key);
            }
            else
            {
                statuses[kvp.Key] = status;
            }
        }

        foreach (var statusId in expired)
        {
            statuses.Remove(statusId);
        }

        if (expired.Count > 0 || changed)
        {
            NotifyStatsChanged();
        }
    }

    private void NotifyStatsChanged()
    {
        StatsChanged?.Invoke(this);
    }

    private void ApplyStatusTick(StatusDefinition definition, StatusInstance instance)
    {
        switch (definition.TickEffect)
        {
            case StatusTickEffectType.DamagePerStack:
                ApplyUnblockedDamage(definition.TickValuePerStack * instance.Stacks);
                break;
        }
    }

    private void ApplyUnblockedDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        NotifyStatsChanged();
    }

    private StatusDefinition ResolveStatusDefinition(string statusId)
    {
        if (statusLibrary != null)
        {
            var definition = statusLibrary.GetDefinition(statusId);
            if (definition != null)
            {
                return definition;
            }
        }

        return StatusDefinition.CreateDefault(statusId);
    }
}

public struct StatusInstance
{
    public StatusInstance(StatusDefinition definition, int stacks, int duration)
    {
        Definition = definition;
        Stacks = stacks;
        Duration = duration;
    }

    public StatusDefinition Definition { get; }
    public int Stacks { get; set; }
    public int Duration { get; set; }
}
