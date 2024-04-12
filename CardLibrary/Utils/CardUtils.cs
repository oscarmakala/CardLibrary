using CardLibrary.Types;

namespace CardLibrary.Utils;

public static class CardUtils
{
    public static string DisplayCard(IEnumerable<Card>? cards)
    {
        return cards == null ? "" : cards.Aggregate("", (current, card) => current + DisplayCard(card));
    }

    public static string DisplayCard(Card? card)
    {
        if (card == null)
        {
            return "";
        }

        var unicodeCard = "";
        unicodeCard += card.Rank switch
        {
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => $"{card.Rank}"
        };

        unicodeCard += card.Suit switch
        {
            Suit.Hearts => "\u2665",
            Suit.Spades => "\u2660",
            Suit.Clubs => "\u2663",
            Suit.Diamonds => "\u2666",
            _ => throw new ArgumentOutOfRangeException()
        };
        return unicodeCard + " ";
    }
}