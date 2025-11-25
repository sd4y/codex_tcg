using System;
using UnityEngine;

[Serializable]
public class CardInstance
{
    public CardInstance(CardData data)
    {
        Data = data;
        ResetCost();
    }

    public CardData Data { get; }
    public int CurrentCost { get; private set; }
    public bool IsUpgraded { get; private set; }
    public bool IsExhausted { get; private set; }

    public void ResetCost()
    {
        CurrentCost = Data.Cost;
    }

    public void ModifyCost(int delta)
    {
        CurrentCost = Mathf.Max(0, CurrentCost + delta);
    }

    public void Upgrade()
    {
        IsUpgraded = true;
    }

    public void MarkExhausted()
    {
        IsExhausted = true;
    }
}
