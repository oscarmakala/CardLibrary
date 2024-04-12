namespace CardLibrary.Types;

public enum SpecialCard
{
    Reverse = 8,
    Two = 2,
    Skip = 7
}

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public class Card
{
    private bool Equals(Card other)
    {
        return Suit == other.Suit && Rank == other.Rank;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((Card)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Suit, Rank);
    }

    public Suit Suit { get; }
    public int Rank { get; }

    public Card(Suit suit, int rank)
    {
        Suit = suit;
        Rank = rank;
    }


    // public override bool Equals(object other)
    // {
    //     if (other == null) return false;
    //     return Suit == other.Suit && Rank == other.Rank;
    // }


    public override string ToString()
    {
        return $"{Rank} of {Suit.ToString()}";
    }
}