using CardLibrary.Manager;
using CardLibrary.Types;
using CardLibrary.Utils;
using CardTest.MonoSimulation;
using Newtonsoft.Json;

namespace CardTest;

public class GameClient : MonoBehaviour
{
    private TurnTimer? _turnTimer;

    public override void Start()
    {
        base.Start();

        _turnTimer = new TurnTimer();
        var players = new GamePlayerData[]
        {
            new("0000-0000-0000", "Oscar", 0, 1, 0, false),
            new("1111-1111-1111", "AI 1", 0, 1, 1, true),
        };
        var manager = new GameManager(new GameData(players), _turnTimer);
        manager.OnNextTurn += OnTableUpdate;
        manager.StartNewGame();
    }

    protected virtual void OnTableUpdate(Board board)
    {
        Console.WriteLine($"GameCard  : {CardUtils.DisplayCard(board.GameCard)}");
        Console.WriteLine($"{board.UserId}: {CardUtils.DisplayCard(board.Hand)}");
    }


    protected override void Update(float deltaTime)
    {
        _turnTimer?.Update(deltaTime);
    }
}