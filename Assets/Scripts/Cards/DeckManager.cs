using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private List<CardData> starterDeck = new();
    [SerializeField] private int startingHandSize = 5;

    private readonly List<CardInstance> drawPile = new();
    private readonly List<CardInstance> discardPile = new();
    private readonly List<CardInstance> hand = new();
    private readonly List<CardInstance> exhaustPile = new();

    public IReadOnlyList<CardInstance> DrawPile => drawPile;
    public IReadOnlyList<CardInstance> DiscardPile => discardPile;
    public IReadOnlyList<CardInstance> Hand => hand;
    public IReadOnlyList<CardInstance> ExhaustPile => exhaustPile;
    public IEnumerable<CardData> StarterDeck => starterDeck;

    public void InitializeDeck(IEnumerable<CardData> deckList)
    {
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();
        exhaustPile.Clear();

        foreach (var card in deckList)
        {
            drawPile.Add(card.CreateInstance());
        }

        Shuffle(drawPile);
    }

    public void DrawStartingHand()
    {
        DrawCards(startingHandSize);
    }

    public List<CardInstance> DrawCards(int count)
    {
        var drawnCards = new List<CardInstance>();

        for (int i = 0; i < count; i++)
        {
            if (drawPile.Count == 0)
            {
                ShuffleDiscardIntoDraw();
                if (drawPile.Count == 0)
                {
                    break;
                }
            }

            var card = drawPile[^1];
            drawPile.RemoveAt(drawPile.Count - 1);
            hand.Add(card);
            drawnCards.Add(card);
        }

        return drawnCards;
    }

    public bool RemoveFromHand(CardInstance card)
    {
        return hand.Remove(card);
    }

    public void ReturnCardToHand(CardInstance card)
    {
        if (!hand.Contains(card))
        {
            hand.Add(card);
        }
    }

    public void Discard(CardInstance card)
    {
        discardPile.Add(card);
    }

    public void Exhaust(CardInstance card)
    {
        card.MarkExhausted();
        exhaustPile.Add(card);
    }

    public void ResetHandsBetweenTurns()
    {
        foreach (var card in hand.ToList())
        {
            discardPile.Add(card);
        }

        hand.Clear();
    }

    public void Shuffle(IList<CardInstance> pile)
    {
        for (int i = pile.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (pile[i], pile[swapIndex]) = (pile[swapIndex], pile[i]);
        }
    }

    private void ShuffleDiscardIntoDraw()
    {
        if (discardPile.Count == 0)
        {
            return;
        }

        foreach (var card in discardPile)
        {
            card.ResetCost();
            drawPile.Add(card);
        }

        discardPile.Clear();
        Shuffle(drawPile);
    }
}
