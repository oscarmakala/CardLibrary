using CardLibrary.Converstion;
using CardLibrary.Data;
using CardLibrary.Extension;

namespace CardLibrary.Types;

public class Deck : DropZone, ITakeCard
{
    private readonly DiscardPile? _discardPile;
    public Action? OnCardDealtCallback;


    public Deck()
    {
    }

    public Deck(DiscardPile discardPile, string? notation = null)
    {
        _discardPile = discardPile;
        if (notation != null)
        {
            var d = DeckString.LoadFromString(notation);
            Cards.AddRange(d.Cards);
        }
        else
        {
            CreateStack(1);
            Shuffle();
        }
    }

    private void CreateStack(int lowest)
    {
        AddSuit(Suit.Hearts, lowest);
        AddSuit(Suit.Diamonds, lowest);
        AddSuit(Suit.Clubs, lowest);
        AddSuit(Suit.Spades, lowest);
    }

    private void AddSuit(Suit suit, int lowest)
    {
        for (var i = lowest; i <= 13; i++)
        {
            GetCardsFromZone().Add(new Card(suit, i));
        }
    }

    /// <summary>
    /// Implements Fisher–Yates shuffle
    /// </summary>
    private void Shuffle()
    {
        var random = new Random();
        var n = Cards.Count;
        while (n > 1)
        {
            n--;
            var k = random.Next(n + 1);
            (Cards[k], Cards[n]) = (Cards[n], Cards[k]);
        }
    }

    private Card TopCard()
    {
        if (Cards.Count == 0)
        {
            CheckIfDeckIsEmpty();
        }

        return Cards.Last();
    }


    private Card DrawCard()
    {
        var card = TopCard();
        RemoveCard(card);
        return card;
    }


    private void RemoveCard(Card? card)
    {
        if (card == null) return;
        for (var i = 0; i < Cards.Count; ++i)
        {
            if (Cards[i].Equals(card))
            {
                Cards.RemoveAt(i);
            }
        }
    }


    public void DealCards(PlayerData[] players, int numberOfCards)
    {
        for (var i = 0; i < numberOfCards; i++)
        {
            foreach (var player in players)
            {
                var card = DrawCard();
                player.CardsInHand.Add(card);
            }
        }

        PutLastCardToDiscardPile();
    }

    public void DealCards(Hand[] hands)
    {
        for (var i = 0; i < 5; i++)
        {
            foreach (var hand in hands)
            {
                var card = DrawCard();
                hand.AddCard(card);
            }
        }
        PutLastCardToDiscardPile();
        OnCardDealtCallback?.Invoke();
        
    }

    private void PutLastCardToDiscardPile()
    {
        _discardPile?.DropCard(DrawCard());
    }

    public Card TakeCard(Hand hand)
    {
        var card = DrawCard();
        hand.AddCard(card);
        return card;
    }

    private void CheckIfDeckIsEmpty()
    {
        var remainingOnDeckCards = GetRemainingCardsNumber();
        if (remainingOnDeckCards == 0)
        {
            ReturnDiscardedCardsToDeck();
        }
    }

    private void ReturnDiscardedCardsToDeck()
    {
        var cards = _discardPile!.GetCardsFromZone();
        // Setting the last card aside to ensure it's not shuffled back to the deck 
        var lastCard = cards.Last();
        cards.Remove(lastCard);
        cards.Shuffle();

        // Add shuffled cards back to the deck
        Cards.AddRange(cards);
        foreach (var card in cards.ToList())
        {
            _discardPile.Cards.Remove(card);
        }

        // Adding the last card back to the discard pile
        _discardPile.Cards.Add(lastCard);
    }

    private int GetRemainingCardsNumber()
    {
        return Cards.Count;
    }
}