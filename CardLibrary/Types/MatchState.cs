using CardLibrary.Data;

namespace CardLibrary.Types;

public class MatchState
{
    public GameState State = GameState.Connecting;

    //Players
    public readonly PlayerData[] Players;
    private readonly string _matchId;
    public readonly Deck Deck;
    public int IndexOfCurrentPlayer { get; set; }
    public int TurnCount { get; set; }
    public int NumberOfCardsToDeal { get; set; }
    public DiscardPile DiscardPile { get; set; }
    public int PenaltyCardsToTake { get; set; }
    public GamePhase Phase { get; set; }

    public int PlayerTurnDirection = 1;

    public MatchState(string matchId, int numberOfPlayers)
    {
        _matchId = matchId;
        Players = new PlayerData[numberOfPlayers];
        for (var i = 0; i < numberOfPlayers; i++)
            Players[i] = new PlayerData(i);
        DiscardPile = new DiscardPile();
        Deck = new Deck(DiscardPile);
    }

    private PlayerData? GetPlayer(int id)
    {
        if (id >= 0 && id < Players.Length)
            return Players[id];
        return null;
    }

    public PlayerData? GetActivePlayer()
    {
        return GetPlayer(IndexOfCurrentPlayer);
    }
}

public enum GameState
{
    Connecting = 0, //Players are not connected
    Play = 20, //Game is being played
    GameEnded = 99,
}