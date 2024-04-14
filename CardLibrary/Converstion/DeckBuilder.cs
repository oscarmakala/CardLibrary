using CardLibrary.Types;

namespace CardLibrary.Converstion;

/// <summary>
/// 
/// </summary>
public static class DeckBuilder
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="deck"></param>
    /// <returns></returns>
    public static (bool succeeded, GameException? exception) TryLoad(string data, out Deck deck)
    {
        deck = new Deck();
        var cardNotations = data.Split(" ");
        foreach (var cardNotation in cardNotations)
        {
            var notationSuitRank = cardNotation.Split("-");
            var suit = (Suit)Enum.Parse(typeof(Suit), notationSuitRank[0]);
            var rank = int.Parse(notationSuitRank[1]);
            var card = new Card(suit, rank);
            deck.GetCardsFromZone().Add(card);
        }

        return (true, null);
    }
}