using CardLibrary.Manager;

namespace CardLibrary.Types;

public class Player
{
    public string UserId { get; set; }
    private readonly CheckTakenCardDelegate _checkTakenCardDelegate;
    private readonly DiscardSelectedCard _discardSelectedCard;
    public Action OnFinishMoveCb;
    public Hand MyHand;
    public string Name;
    private Card? _cardOnAction;
    public bool InAction;
    private readonly Deck _deck;
    protected readonly DiscardPile DiscardPile;
    private readonly TurnTimer _turnTimer;
    public int TurnOrder { get; set; }

    public Card? CardOnAction
    {
        get => _cardOnAction;
        protected set
        {
            _cardOnAction = value;
            InAction = true;
        }
    }

    

    public Player(Deck deck,
        DiscardPile discardPile,
        CheckTakenCardDelegate checkTakenCardDelegate,
        DiscardSelectedCard discardSelectedCard,
        TurnTimer turnTimer)
    {
        _deck = deck;
        _turnTimer = turnTimer;
        DiscardPile = discardPile;
        _checkTakenCardDelegate = checkTakenCardDelegate;
        _discardSelectedCard = discardSelectedCard;
        
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

    public void TakePenaltyCards(int penaltyCardsToTake)
    {
        for (var i = 0; i < penaltyCardsToTake; i++)
        {
            if (i == penaltyCardsToTake - 1)
            {
                CardOnAction = _deck.TakeCard(MyHand);
                if (CardOnAction == null) continue;
                _checkTakenCardDelegate(CardOnAction);
                FinishMove();
            }
            else
                _deck.TakeCard(MyHand);
        }
    }

    private void FinishMove()
    {
        InAction = false;
        OnFinishMoveCb();
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
        FinishMove();
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
        return MyHand.GetAvailableCardsFromZone;
    }

    protected void DiscardSelectedCard()
    {
        if (CardOnAction != null)
        {
            DiscardPile.OnDrop(CardOnAction);
            MyHand.OnCardDiscard();
            FinishMove();
            MyHand.OnCardDiscard();
        }
        else
        {
            _discardSelectedCard();
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
        return _deck;
    }

    public bool TimeIsOver()
    {
        return _turnTimer.TimeIsOver();
    }
}