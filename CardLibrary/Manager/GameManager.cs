using CardLibrary.AI;
using CardLibrary.Types;

namespace CardLibrary.Manager;

public delegate void DiscardSelectedCard();

public delegate void CheckTakenCardDelegate(Card card);

public delegate void TryFinishTheGameDelegate(Player player);

/// <summary>
/// Game board logic
/// </summary>
public sealed class GameManager
{
    public Action? OnGamePhaseChangeCallback;
    public Action<Board>? OnNextTurn;
    private GamePhase _gamePhase;
    private readonly Queue<GamePhase> _turnPhases = new();
    private readonly Deck _deck;
    private readonly Hand[] _playersHands;
    private readonly DiscardPile _discardPile;
    private int _indexOfCurrentPlayer = -1;
    private int _playerTurnDirection = 1;
    private int _penaltyCardsToTake;
    private List<Player> _playerList = new();
    private readonly GameData _gameData;
    private Action? _botAction;
    private readonly TurnTimer _turnTimer;
    private Player? CurrentPlayer { get; set; }


    public GameManager(GameData gameData, TurnTimer turnTimer)
    {
        _gameData = gameData;
        _discardPile = new DiscardPile();
        _deck = new Deck(_discardPile);
        _turnTimer = turnTimer;
        _playersHands = new Hand[_gameData.Players.Length];

        _turnTimer.OnTimerFinishedCb = PerformAction;
        _deck.OnCardDealtCallback += StartGame;
        _discardPile.OnCardPutOnDiscardPile += CheckForCardSpecialPower;
    }

    private void CheckForCardSpecialPower(Card card, Card? prevCard)
    {
        if (!IsValidTimeToCheckCardSpecialPower()) return;
        switch (card.Rank)
        {
            case (int)SpecialCard.Reverse:
                if (IsOnlyTwoPlayersGame())
                    SkipNextPlayer();
                else
                    ReversePlayersOrder();
                break;
            case (int)SpecialCard.Skip:
                SkipNextPlayer();
                break;
            case (int)SpecialCard.Two:
                AddPenaltyCards(2);
                break;
        }
    }

    private void AddPenaltyCards(int penaltyCards)
    {
        _penaltyCardsToTake += penaltyCards;
    }

    private void ReversePlayersOrder()
    {
        _playerTurnDirection = -_playerTurnDirection;
    }


    private void SkipNextPlayer()
    {
        IncreasePlayerIndex();
    }

    private bool IsOnlyTwoPlayersGame()
    {
        return _playerList.Count == 2;
    }

    private bool IsValidTimeToCheckCardSpecialPower()
    {
        return _gamePhase is GamePhase.TakeOrDiscard or GamePhase.PassOrDiscard
                   or GamePhase.PassOrDiscardNextSequencedCard or GamePhase.OverbidOrTakePenalties &&
               CurrentPlayer?.TimeIsOver() == false;
    }

    private void StartGame()
    {
        NextTurn();
    }

    private void NextTurn()
    {
        //print the data for the next turn.
        IncreasePlayerIndex();
        AssignNewPlayer();
        ShowTurnDesc();
        RecreatePhasesQueue();
        StartNextPhase();
    }

    private void StartNextPhase()
    {
        if (_gamePhase == GamePhase.GameEnded)
            return;

        if (_turnPhases.Count > 0)
        {
            OnNextTurn?.Invoke(new Board
            {
                UserId = CurrentPlayer?.UserId,
                Hand = CurrentPlayer?.MyHand.GetCardsFromZone(),
                GameCard = _discardPile.GetTopCardFromDiscardPile()
            });
            //there are moves so output.
            CurrentPlayer?.ResetTimer();
            _gamePhase = _turnPhases.Dequeue();
            CheckIfNextPlayerIsBot();
            CheckIfNextPlayerIsForcedToTakeCards();
        }
        else
        {
            NextTurn();
        }
    }

    private void CheckIfNextPlayerIsForcedToTakeCards()
    {
        if (IsValidGamePhase(GamePhase.OverbidOrTakePenalties) && CurrentPlayer?.HasCardsToDefend() == false)
            TakePenaltyCards();
    }

    private bool IsValidGamePhase(GamePhase gPhase)
    {
        return _gamePhase == gPhase && IsThisGamePlayerAndTimeIsNotOver();
    }

    private void TakePenaltyCards()
    {
        CurrentPlayer?.TakePenaltyCards(_penaltyCardsToTake);
        Console.WriteLine("Take " + _penaltyCardsToTake + " cards!");
        _penaltyCardsToTake = 0;
    }

    private bool IsThisGamePlayerAndTimeIsNotOver()
    {
        return CurrentPlayer != null && IsThisGamePlayer() && CurrentPlayer.TimeIsOver() == false;
    }

    private bool IsThisGamePlayer()
    {
        return CurrentPlayer is not AiPlayer;
    }

    private void CheckIfNextPlayerIsBot()
    {
        if (CurrentPlayer?.TimeIsOver() == true)
        {
            PerformAction();
        }
        else if (CurrentPlayer is AiPlayer bot)
        {
            _botAction = GetCurrentTurnProperAction(bot);
            DelayedBotAction();
        }
    }

    private void PerformAction()
    {
        if (CurrentPlayer is not { InAction: false }) return;
        var action = GetCurrentTurnTimerPassedProperAction(CurrentPlayer);
        action?.Invoke();
    }

    private Action? GetCurrentTurnTimerPassedProperAction(Player player)
    {
        Action? action = null;
        switch (_gamePhase)
        {
            case GamePhase.TakeOrDiscard:
                action = player.TakeCard;
                break;
            case GamePhase.PassOrDiscard:
                action = player.Pass;
                break;
            case GamePhase.PassOrDiscardNextSequencedCard:
                action = player.Pass;
                break;
            case GamePhase.OverbidOrTakePenalties:
                action = DiscardOrTakePenaltyCards;
                break;
            case GamePhase.CardsDealing:
            case GamePhase.RoundEnded:
            case GamePhase.GameEnded:
            default:
                break;
        }

        return action;
    }

    private void DelayedBotAction()
    {
        if (IsProperPhaseToPerformGameMove())
            _botAction?.Invoke();
    }

    private bool IsProperPhaseToPerformGameMove()
    {
        return !(IsGameEnded() || IsGameDealingCards() || IsRoundEnded());
    }


    private bool IsRoundEnded()
    {
        return _gamePhase == GamePhase.RoundEnded;
    }


    private bool IsGameEnded()
    {
        return _gamePhase == GamePhase.GameEnded;
    }

    private bool IsGameDealingCards()
    {
        return _gamePhase == GamePhase.CardsDealing;
    }


    private Action? GetCurrentTurnProperAction(Player player)
    {
        Action? action = null;
        switch (_gamePhase)
        {
            case GamePhase.TakeOrDiscard:
                action = player.Discard;
                break;
            case GamePhase.PassOrDiscard:
                action = player.Discard;
                break;
            case GamePhase.PassOrDiscardNextSequencedCard:
                action = player.Discard;
                break;
            case GamePhase.OverbidOrTakePenalties:
                action = DiscardOrTakePenaltyCards;
                break;
            case GamePhase.CardsDealing:
            case GamePhase.RoundEnded:
            case GamePhase.GameEnded:
            default:
                break;
        }

        return action;
    }

    private void DiscardOrTakePenaltyCards()
    {
        if (CurrentPlayer?.HasCardsToDefend() == true)
            CurrentPlayer?.Discard();
        else
            TakePenaltyCards();
    }

    private void ShowTurnDesc()
    {
        Console.WriteLine($"{CurrentPlayer?.Name} turn.");
    }

    private void AssignNewPlayer()
    {
        CurrentPlayer?.StopTimer();
        CurrentPlayer = _playerList[_indexOfCurrentPlayer];
        CurrentPlayer?.StartTimer();
    }

    private void IncreasePlayerIndex()
    {
        _indexOfCurrentPlayer = (_indexOfCurrentPlayer + _playerTurnDirection) % _playerList.Count;
        if (_indexOfCurrentPlayer < 0)
            _indexOfCurrentPlayer += _playerList.Count;
    }

    ~GameManager()
    {
        _deck.OnCardDealtCallback -= StartGame;
    }

    public void StartNewGame()
    {
        _gamePhase = GamePhase.CardsDealing;

        _deck.ResetState();
        InitPlayers();
        StartNewMatch();
    }


    private void InitPlayers()
    {
        for (var index = 0; index < _gameData.Players.Length; index++)
        {
            var playerData = _gameData.Players[index];
            var player = playerData.IsBot
                ? new AiPlayer(_deck, _discardPile, CheckTakenCard, DiscardSelected, _turnTimer)
                : new Player(_deck, _discardPile, CheckTakenCard, DiscardSelected, _turnTimer);
            player.OnFinishMoveCb += StartNextPhase;
            player.Name = playerData.Name;
            player.TurnOrder = playerData.TurnOrder;
            player.UserId = playerData.Id;
            _playersHands[index] = new Hand(_discardPile, TryFinishTheGame)
            {
                PlayerOfThisHand = player
            };
            _playerList.Add(player);
        }

        SetupPlayersOrder();
    }

    private void TryFinishTheGame(Player winner)
    {
        if (IsProperPhaseToPerformGameMove() && IsThisPlayerTurn(winner))
        {
            CloseThisGame();
        }
    }

    private void CloseThisGame()
    {
        _turnTimer.StopTimer();
        _gamePhase = GamePhase.GameEnded;
    }


    private bool IsThisPlayerTurn(Player player)
    {
        return CurrentPlayer == player;
    }


    private void SetupPlayersOrder()
    {
        _playerList = _playerList.OrderBy(p => p.TurnOrder).ToList();
    }

    private void DiscardSelected()
    {
        switch (_gamePhase)
        {
            case GamePhase.TakeOrDiscard:
                Console.WriteLine("Can't discard any card so I take one");
                CurrentPlayer?.TakeCard();
                break;
            case GamePhase.PassOrDiscard:
                Console.WriteLine("Can't discard any card so I pass");
                CurrentPlayer?.Pass();
                break;
            case GamePhase.CardsDealing:
            case GamePhase.PassOrDiscardNextSequencedCard:
            case GamePhase.OverbidOrTakePenalties:
            case GamePhase.RoundEnded:
            case GamePhase.GameEnded:
            default:
                break;
        }
    }

    private void CheckTakenCard(Card card)
    {
        if (_gamePhase == GamePhase.TakeOrDiscard && CurrentPlayer?.TimeIsOver() == false &&
            _discardPile.CheckIfCardCanBePutOnDiscardPile(card))
            AddNewMoveToQueue(GamePhase.PassOrDiscard);
    }

    private void AddNewMoveToQueue(GamePhase gamePhase)
    {
        _turnPhases.Enqueue(gamePhase);
    }

    private Hand[] CreateHandDealOrder()
    {
        var handOrder = _playersHands.Where(h => h.PlayerOfThisHand != null).ToArray();
        return handOrder.OrderBy(h => h.PlayerOfThisHand?.TurnOrder).ToArray();
    }

    private void StartNewMatch()
    {
        RecreatePhasesQueue();
        var dealOrder = CreateHandDealOrder();
        _deck.DealCards(dealOrder);
    }

    private void RecreatePhasesQueue()
    {
        _turnPhases.Clear();
        _turnPhases.Enqueue(ThereArePenaltiesToTake() ? GamePhase.OverbidOrTakePenalties : GamePhase.TakeOrDiscard);
    }


    private bool ThereArePenaltiesToTake()
    {
        return _penaltyCardsToTake > 0;
    }
}