namespace CardLibrary.Types;

public class Deck : DropZone, ITakeCard
{
    private readonly DiscardPile _discardPile;
    public Action? OnCardDealtCallback;

    public Deck(DiscardPile discardPile)
    {
        _discardPile = discardPile;
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

    private Card? TopCard()
    {
        return Cards.Count > 0 ? Cards[^1] : null;
    }


    public void Deal(int nbCards, ICollection<Card> cards)
    {
        for (var i = 0; i < nbCards; i++)
        {
            var card = DrawCard();
            if (card != null) cards.Add(card);
        }
    }

    private Card? DrawCard()
    {
        if (Cards.Count <= 0)
        {
            Refill();
        }

        var card = TopCard();
        RemoveCard(card);
        return card;
    }

    private void Refill()
    {
        foreach (var discardPileCard in _discardPile.Cards.ToList())
        {
            Cards.Add(discardPileCard);
            _discardPile.Cards.Remove(discardPileCard);
        }
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

    public void ResetState()
    {
        CreateStack(1);
        Shuffle();
    }

    public void DealCards(Hand[] hands)
    {
        for (var i = 0; i < 5; i++)
        {
            foreach (var hand in hands)
            {
                var card = DrawCard();
                if (card != null) hand.AddCard(card);
            }
        }

        PutLastCardToDiscardPile();
        OnCardDealtCallback?.Invoke();
    }

    private void PutLastCardToDiscardPile()
    {
        _discardPile.DealCard(DrawCard());
    }

    public Card TakeCard(Hand hand)
    {
        var card = DrawCard();
        if (card != null)
        {
            hand.AddCard(card);
        }

        return card;
    }
}