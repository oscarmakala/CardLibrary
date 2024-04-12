namespace CardLibrary.Types;

public sealed class DiscardPile : DropZone
{
    public Action<Card, Card?> OnCardPutOnDiscardPile;

    public void DealCard(Card? card)
    {
        var prevCard = GetTopCardFromDiscardPile();
        if (card == null) return;
        GetCardsFromZone().Add(card);
        OnCardPutOnDiscardPile(card, prevCard);
    }

    public Card? GetTopCardFromDiscardPile()
    {
        var cards = GetCardsFromZone();
        return cards.Count > 0 ? cards[^1] : null;
    }

    public bool CheckIfCardCanBePutOnDiscardPile(Card card)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard != null) return card.Rank == topDeckCard.Rank || card.Suit == topDeckCard.Suit;
        return false;
    }

    public void OnDrop(Card card)
    {
        var prevCard = GetTopCardFromDiscardPile();
        //code to send on the server
        Cards.Add(card);
        OnCardPutOnDiscardPile(card, prevCard);
    }

    public bool CheckIfCardCardCanRescueFromPenalty(Card c)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard != null)
            return c.Rank switch
            {
                (int)SpecialCard.Reverse or (int)SpecialCard.Two or (int)SpecialCard.Skip => true,
                _ => false
            };
        Console.WriteLine("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
        return false;
    }

    public bool IsNextSequencedCard(Card card, Player playerOfThisHand)
    {
        var topDeckCard = GetTopCardFromDiscardPile();
        if (topDeckCard != null) return card.Suit != topDeckCard.Suit;
        Console.WriteLine("CheckIfCardCanBePutOnDiscardPile -- Card is null!");
        return false;
    }
}