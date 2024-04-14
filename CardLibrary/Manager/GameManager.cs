using CardLibrary.AI;
using CardLibrary.Types;

namespace CardLibrary.Manager;

/// <summary>
/// Game board logic
/// </summary>
public class GameManager
{
    public Action<string>? OnNotificationCallback;
    public Action? OnGameFinishedCallback;
    public Action<Board>? OnNextTurnCallback;
    public GamePhase GamePhase;
    private readonly Queue<GamePhase> _turnPhases = new();
    public readonly Deck Deck;
    private readonly Hand[] _playersHands;
    public readonly DiscardPile DiscardPile = new();
    private int _indexOfCurrentPlayer = -1;
    private int _playerTurnDirection = 1;
    private int _penaltyCardsToTake;
    private List<Player> _playerList = new();
    private readonly GameData _gameData;
    private Action? _botAction;
    private int _turnCount;
    private Player? CurrentPlayer { get; set; }


    public GameManager(GameData gameData)
    {
        _gameData = gameData;
        Deck = new Deck(DiscardPile, MatchConstants.TestDeck);
        _playersHands = new Hand[_gameData.Players.Length];

        //callbacks
        Deck.OnCardDealtCallback += StartGame;
        DiscardPile.OnCardPutOnDiscardPile += CheckForCardSpecialPower;
    }


    private void CheckForCardSpecialPower(Card card, Card? prevCard)
    {
        if (!IsValidTimeToCheckCardSpecialPower() && _turnCount != 0) return;
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
        OnNotificationCallback?.Invoke("Reverse turn for next player");
        _playerTurnDirection = -_playerTurnDirection;
    }


    private void SkipNextPlayer()
    {
        IncreasePlayerIndex();
        OnNotificationCallback?.Invoke($"Skip turn for {CurrentPlayer?.Name} ");
    }

    private bool IsOnlyTwoPlayersGame()
    {
        return _playerList.Count == 2;
    }

    private bool IsValidTimeToCheckCardSpecialPower()
    {
        return GamePhase is GamePhase.TakeOrDiscard or GamePhase.PassOrDiscard
            or GamePhase.PassOrDiscardNextSequencedCard or GamePhase.OverbidOrTakePenalties;
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
        if (GamePhase == GamePhase.GameEnded)
            return;

        if (_turnPhases.Count > 0)
        {
            _turnCount++;
            OnNextTurnCallback?.Invoke(new Board
            {
                UserId = CurrentPlayer?.UserId,
                Hand = CurrentPlayer?.MyHand.GetCardsFromZone(),
                GameCard = DiscardPile.GetTopCardFromDiscardPile()
            });
            //there are moves so output.
            CurrentPlayer?.ResetTimer();
            GamePhase = _turnPhases.Dequeue();
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
        {
            TakePenaltyCards();
        }
    }

    private bool IsValidGamePhase(GamePhase gPhase)
    {
        return GamePhase == gPhase && IsThisGamePlayerAndTimeIsNotOver();
    }

    private void TakePenaltyCards()
    {
        CurrentPlayer?.TakePenaltyCards(_penaltyCardsToTake);
        OnNotificationCallback?.Invoke($"{CurrentPlayer?.Name} Take {_penaltyCardsToTake} cards!");
        _penaltyCardsToTake = 0;
        StartNextPhase();
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
            CurrentPlayer?.PerformAction();
        }
        else if (CurrentPlayer is AiPlayer bot)
        {
            _botAction = GetCurrentTurnProperAction(bot);
            DelayedBotAction();
        }
    }


    public Action? GetCurrentTurnTimerPassedProperAction(Player player)
    {
        Action? action = null;
        switch (GamePhase)
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
        return GamePhase == GamePhase.RoundEnded;
    }


    private bool IsGameEnded()
    {
        return GamePhase == GamePhase.GameEnded;
    }

    private bool IsGameDealingCards()
    {
        return GamePhase == GamePhase.CardsDealing;
    }


    private Action? GetCurrentTurnProperAction(Player player)
    {
        Action? action = null;
        switch (GamePhase)
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
        if (CurrentPlayer?.HasCardsToDefend() == true && CurrentPlayer.TimeIsOver() == false)
            CurrentPlayer?.Discard();
        else
            TakePenaltyCards();
    }

    private void ShowTurnDesc()
    {
        OnNotificationCallback?.Invoke($"{CurrentPlayer?.Name} turn.");
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

    public void StartNewGame()
    {
        if (!IsGameEnded())
        {
            CloseThisGame();
        }

        GamePhase = GamePhase.CardsDealing;
        ResetPlayers();
        InitPlayers();
        StartNewMatch();
    }

    private void ResetPlayers()
    {
        _penaltyCardsToTake = 0;
        _indexOfCurrentPlayer = -1;
        _playerTurnDirection = 1;
    }


    private void InitPlayers()
    {
        for (var index = 0; index < _gameData.Players.Length; index++)
        {
            var playerData = _gameData.Players[index];
            var player = playerData.IsBot
                ? new AiPlayer(this)
                : new Player(this);


            player.OnFinishMoveCb += StartNextPhase;
            player.Name = playerData.Name;
            player.TurnOrder = playerData.TurnOrder;
            player.UserId = playerData.Id;


            _playersHands[index] = new Hand(this)
            {
                PlayerOfThisHand = player
            };
            _playerList.Add(player);
        }

        SetupPlayersOrder();
    }

    public void TryFinishTheGame(Player? winner)
    {
        if (winner == null) return;
        if (!IsProperPhaseToPerformGameMove() || !IsThisPlayerTurn(winner)) return;
        CloseThisGame();
        ShowEndGameWindow(winner);
    }

    protected virtual void ShowEndGameWindow(Player winner)
    {
        var score = _playerList.Where(t => t != winner).Sum(t => t.GetScore());
        OnNotificationCallback?.Invoke("The winner is: " + winner.Name + "\n\nTotal score: " + score);
    }

    private void CloseThisGame()
    {
        foreach (var player in _playerList)
        {
            player.StopTimer();
        }

        GamePhase = GamePhase.GameEnded;
        OnGameFinishedCallback?.Invoke();
    }


    private bool IsThisPlayerTurn(Player player)
    {
        return CurrentPlayer == player;
    }


    private void SetupPlayersOrder()
    {
        _playerList = _playerList.OrderBy(p => p.TurnOrder).ToList();
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
        Deck.DealCards(dealOrder);
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