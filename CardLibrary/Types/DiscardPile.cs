namespace CardLibrary.Types;

public sealed class DiscardPile : DropZone
{
    public Action? OnDiscardPileDrop;
    public Action<Card, Card?>? OnCardPutOnDiscardPile;

    public void DropCard(Card? card)
    {
        var prevCard = GetTopCardFromDiscardPile();
        if (card == null) return;
        GetCardsFromZone().Add(card);
        OnCardPutOnDiscardPile?.Invoke(card, prevCard);
    }

    public Card? GetTopCardFromDiscardPile()
    {
        var cards = GetCardsFromZone();
        return cards.Count > 0 ? cards.Last() : null;
    }

    public bool CheckIfCardCanBePutOnDiscardPile(Card card)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard != null) return card.Rank == topDeckCard.Rank || card.Suit == topDeckCard.Suit;
        Console.WriteLine("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
        return false;
    }

    public void OnDrop(Card card)
    {
        var prevCard = GetTopCardFromDiscardPile();
        Cards.Add(card);
        OnCardPutOnDiscardPile?.Invoke(card, prevCard);
        OnDiscardPileDrop?.Invoke();
    }

    public bool CheckIfCardCardCanRescueFromPenalty(Card c)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard == null)
        {
            Console.WriteLine("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
            return false;
        }

        switch (topDeckCard.Rank)
        {
            case (int)SpecialCard.Two when c.Rank == (int)SpecialCard.Two:
            case (int)SpecialCard.Reverse when c.Rank == (int)SpecialCard.Reverse:
            case (int)SpecialCard.Skip when c.Rank == (int)SpecialCard.Skip:
                return true;
            default:
                return false;
        }
    }

    public bool IsNextSequencedCard(Card card, Player playerOfThisHand)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard != null) return card.Suit != topDeckCard.Suit;
        Console.WriteLine("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
        return false;
    }
}