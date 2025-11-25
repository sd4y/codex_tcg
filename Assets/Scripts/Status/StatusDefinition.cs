using System;
using UnityEngine;

public enum StatusTickEffectType
{
    None,
    DamagePerStack
}

[Serializable]
public class StatusDefinition
{
    [SerializeField] private string statusId = "status";
    [SerializeField] private string displayName = "Status";
    [SerializeField, TextArea] private string description = "";
    [SerializeField] private StatusTickPhase tickPhase = StatusTickPhase.EndOfTurn;
    [SerializeField] private bool tickEachTurn = false;
    [SerializeField] private StatusTickEffectType tickEffect = StatusTickEffectType.None;
    [SerializeField] private int tickValuePerStack = 0;
    [SerializeField] private bool reduceDuration = true;
    [SerializeField] private StatusTickPhase durationTickPhase = StatusTickPhase.EndOfTurn;
    [SerializeField] private bool removeWhenStacksDepleted = true;

    public string StatusId => statusId;
    public string DisplayName => displayName;
    public string Description => description;
    public StatusTickPhase TickPhase => tickPhase;
    public bool TickEachTurn => tickEachTurn;
    public StatusTickEffectType TickEffect => tickEffect;
    public int TickValuePerStack => tickValuePerStack;
    public bool ReduceDuration => reduceDuration;
    public StatusTickPhase DurationTickPhase => durationTickPhase;
    public bool RemoveWhenStacksDepleted => removeWhenStacksDepleted;

    public bool ShouldTick(StatusTickPhase phase)
    {
        return TickEachTurn && TickPhase == phase;
    }

    public bool ShouldDecrementDuration(StatusTickPhase phase)
    {
        return ReduceDuration && DurationTickPhase == phase;
    }

    public static StatusDefinition CreateDefault(string id)
    {
        switch (id)
        {
            case "strength":
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = "Strength",
                    description = "+1 damage per stack",
                    tickEachTurn = false,
                    reduceDuration = false,
                    removeWhenStacksDepleted = true
                };
            case "weak":
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = "Weak",
                    description = "Deal 25% less damage",
                    tickEachTurn = false,
                    reduceDuration = true,
                    durationTickPhase = StatusTickPhase.EndOfTurn,
                    removeWhenStacksDepleted = true
                };
            case "vulnerable":
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = "Vulnerable",
                    description = "Take 50% more damage",
                    tickEachTurn = false,
                    reduceDuration = true,
                    durationTickPhase = StatusTickPhase.EndOfTurn,
                    removeWhenStacksDepleted = true
                };
            case "frail":
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = "Frail",
                    description = "Gain 25% less block",
                    tickEachTurn = false,
                    reduceDuration = true,
                    durationTickPhase = StatusTickPhase.EndOfTurn,
                    removeWhenStacksDepleted = true
                };
            case "poison":
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = "Poison",
                    description = "Lose HP each turn per stack",
                    tickEachTurn = true,
                    tickPhase = StatusTickPhase.EndOfTurn,
                    tickEffect = StatusTickEffectType.DamagePerStack,
                    tickValuePerStack = 1,
                    reduceDuration = true,
                    durationTickPhase = StatusTickPhase.EndOfTurn,
                    removeWhenStacksDepleted = true
                };
            default:
                return new StatusDefinition
                {
                    statusId = id,
                    displayName = id,
                    description = id,
                    tickEachTurn = false,
                    reduceDuration = true,
                    durationTickPhase = StatusTickPhase.EndOfTurn,
                    removeWhenStacksDepleted = true
                };
        }
    }
}
