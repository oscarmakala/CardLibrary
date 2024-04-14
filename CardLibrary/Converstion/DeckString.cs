using CardLibrary.Types;

namespace CardLibrary.Converstion;

/// <summary>
/// 
/// </summary>
public static class DeckString
{
    /// <summary>
    /// Tries to load Deck from cards space separated in format Suit-Rank
    /// </summary>
    /// <param name="data">deck string to load</param>
    /// <returns>Result with loaded deck</returns>
    public static Deck LoadFromString(string data)
    {
        var (succeeded, exception) = DeckBuilder.TryLoad(data, out var deck);

        if (!succeeded && exception is not null)
            throw exception;

        return deck!;
    }
}