using CardLibrary.Manager;

namespace CardLibrary.Types;

public class Player
{
    public string UserId { get; set; }
    public Action? OnFinishMoveCb;
    public Hand MyHand;
    public string Name;
    private Card? _cardOnAction;
    private bool _inAction;
    protected readonly GameManager Manager;
    private readonly TurnTimer _turnTimer = new();

    public Player(GameManager gameManager)
    {
        Manager = gameManager;
        _turnTimer.OnTimerFinishedCb = PerformAction;
    }

    public int TurnOrder { get; set; }

    protected Card? CardOnAction
    {
        get => _cardOnAction;
        set
        {
            _cardOnAction = value;
            _inAction = true;
        }
    }


    public void AssignHand(Hand hand)
    {
        MyHand = hand;
    }

    public bool HasCardsToDefend()
    {
        var cards = MyHand.GetAvailableToDefendCards();
        return cards.Count != 0;
    }


    private void FinishMove()
    {
        _inAction = false;
        OnFinishMoveCb?.Invoke();
    }

    public void TakeCard()
    {
        TakeCard(GetPile());
    }

    public void Pass()
    {
        FinishMove();
    }

    private void TakeCard(ITakeCard cardPile)
    {
        CardOnAction = cardPile.TakeCard(MyHand);
        if (CardOnAction != null)
        {
            FinishMove();
        }
    }

    public virtual void Discard()
    {
    }

    public void Discard(Card card)
    {
        CardOnAction = card;
        DiscardSelectedCard();
    }

    protected Func<List<Card>> GetChooseCardRule()
    {
        if (Manager.GamePhase == GamePhase.OverbidOrTakePenalties)
            return MyHand.GetAvailableToDefendCards;
        return MyHand.GetAvailableCardsFromZone;
    }

    protected void DiscardSelectedCard()
    {
        if (CardOnAction != null)
        {
            MyHand.Cards.Remove(CardOnAction);
            Manager.DiscardPile.OnDrop(CardOnAction);
            FinishMove();
        }
        else
        {
            switch (Manager.GamePhase)
            {
                case GamePhase.TakeOrDiscard:
                    Console.WriteLine("Can't discard any card so I take one");
                    TakeCard();
                    break;
                case GamePhase.PassOrDiscard:
                    Console.WriteLine("Can't discard any card so I pass");
                    Pass();
                    break;
                case GamePhase.OverbidOrTakePenalties:
                    Console.WriteLine("OverbidOrTakePenalties on AI Discard");
                    break;
                case GamePhase.CardsDealing:
                case GamePhase.PassOrDiscardNextSequencedCard:
                case GamePhase.RoundEnded:
                case GamePhase.GameEnded:
                default:
                    break;
            }
        }
    }


    public void StartTimer()
    {
        _turnTimer.StartTimer();
    }

    public void ResetTimer()
    {
        _turnTimer.ResetTimer();
    }

    public void StopTimer()
    {
        _turnTimer.StopTimer();
    }


    private ITakeCard GetPile()
    {
        return Manager.Deck;
    }

    public bool TimeIsOver()
    {
        return _turnTimer.TimeIsOver();
    }

    public bool HasAnotherCardToPutInto()
    {
        var cards = MyHand.GetAvailableCardsFromZone();
        return cards.Count != 0;
    }

    public int GetScore()
    {
        var cards = MyHand.GetCardsFromZone();
        return cards.Sum(t => t.GetCardPointsValue());
    }

    public void PerformAction()
    {
        if (_inAction) return;
        var action = Manager.GetCurrentTurnTimerPassedProperAction(this);
        action?.Invoke();
    }

    public void TakePenaltyCards(int penaltyCardsToTake)
    {
        for (var i = 0; i < penaltyCardsToTake; i++)
        {
            CardOnAction = Manager.Deck.TakeCard(MyHand);
        }

        _inAction = false;
    }
}