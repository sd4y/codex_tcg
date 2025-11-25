using System.Collections.Generic;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private Transform cardContainer;

    private readonly List<CardView> spawnedCards = new();
    private DeckManager deckManager;
    private BattleManager battleManager;
    private CharacterCombatant currentTarget;

    public void Initialize(DeckManager deck, BattleManager manager, CharacterCombatant defaultTarget)
    {
        if (deckManager != null)
        {
            deckManager.HandChanged -= OnHandChanged;
        }

        deckManager = deck;
        battleManager = manager;
        currentTarget = defaultTarget;

        if (deckManager != null)
        {
            deckManager.HandChanged += OnHandChanged;
            OnHandChanged(deckManager.Hand);
        }
    }

    public void SetTarget(CharacterCombatant target)
    {
        currentTarget = target;
        RefreshTargets();
    }

    private void OnDestroy()
    {
        if (deckManager != null)
        {
            deckManager.HandChanged -= OnHandChanged;
        }
    }

    private void OnHandChanged(IReadOnlyList<CardInstance> hand)
    {
        ClearHand();

        if (hand == null || cardViewPrefab == null)
        {
            return;
        }

        foreach (var card in hand)
        {
            var cardView = Instantiate(cardViewPrefab, cardContainer != null ? cardContainer : transform);
            cardView.Initialize(card, battleManager, currentTarget);
            spawnedCards.Add(cardView);
        }
    }

    private void RefreshTargets()
    {
        foreach (var cardView in spawnedCards)
        {
            cardView.SetTarget(currentTarget);
        }
    }

    private void ClearHand()
    {
        foreach (var cardView in spawnedCards)
        {
            if (cardView != null)
            {
                Destroy(cardView.gameObject);
            }
        }

        spawnedCards.Clear();
    }
}
