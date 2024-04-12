namespace CardLibrary.Types;

/// <summary>
/// Opponent data model
/// </summary>
public class GamePlayerData
{
    public string Id { get; private set; }
    public string Name { get; set; }
    public int AvatarId { get; set; }
    public int Level { get; private set; }
    public int TurnOrder { get; private set; }
    public bool IsBot { get; private set; }


    public GamePlayerData(
        string id,
        string name,
        int avatarId,
        int level,
        int turnOrder,
        bool isBot)
    {
        Id = id;
        Name = name;
        AvatarId = avatarId;
        Level = level;
        TurnOrder = turnOrder;
        IsBot = isBot;
    }
}

public class GameData
{
    public GamePlayerData[] Players { get; private set; }

    public GameData(GamePlayerData[] players)
    {
        Players = players;
    }
}