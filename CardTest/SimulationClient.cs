using CardLibrary.Manager;
using CardLibrary.Types;
using CardLibrary.Utils;
using CardTest.MonoSimulation;

namespace CardTest;

public class SimulationClient : MonoBehaviour
{
    private readonly GameManager _manager;


    public override void Start()
    {
        base.Start();
        _manager.StartNewGame();
    }

    public SimulationClient()
    {
        var players = new GamePlayerData[]
        {
            new("oscar.makala", "Oscar", 0, 1, 0, true),
            new("ai.albert", "AI1", 0, 1, 1, true),
            // new("2222-2222-2222", "AI 2", 0, 1, 2, true),
        };
        _manager = new GameManager(new GameData(players));
        _manager.OnNextTurnCallback += OnTableUpdate;
        _manager.OnGameFinishedCallback += OnGameFinished;
        _manager.OnNotificationCallback += Console.WriteLine;
    }

    protected virtual void OnGameFinished()
    {
        Stop();
    }
    

    protected virtual void OnTableUpdate(Board board)
    {
        Console.WriteLine($"GameCard  : {CardUtils.DisplayCard(board.GameCard)}");
        Console.WriteLine($"{board.UserId}: {CardUtils.DisplayCard(board.Hand)}");
        Console.WriteLine("================================================================");
        Console.WriteLine("\n");
    }

    protected override void Update(float deltaTime)
    {
    }
}