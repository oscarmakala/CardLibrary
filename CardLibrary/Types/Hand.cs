using CardLibrary.Manager;
using CardLibrary.Utils;

namespace CardLibrary.Types;

public sealed class Hand : DropZone
{
    private readonly Player? _playerOfThisHand;


    private readonly GameManager _manager;


    public Hand(GameManager manager)
    {
        _manager = manager;
        _manager.DiscardPile.OnDiscardPileDrop += OnCardDiscard;
    }

    public Player? PlayerOfThisHand
    {
        get => _playerOfThisHand;
        init
        {
            _playerOfThisHand = value;
            _playerOfThisHand?.AssignHand(this);
        }
    }

    public void AddCard(Card card)
    {
        Cards.Add(card);
    }

    public List<Card> GetCard()
    {
        return Cards;
    }

    public List<Card> GetAvailableCardsFromZone()
    {
        var cards = GetCardsFromZone();
        Console.WriteLine($"GetAvailableCardsFromZone {CardUtils.DisplayCard(cards)}");
        return cards.Where(c => _manager.DiscardPile.CheckIfCardCanBePutOnDiscardPile(c)).ToList();
    }


    private void OnCardDiscard()
    {
        if (Cards.Count == 0)
        {
            _manager.TryFinishTheGame(_playerOfThisHand);
        }
    }

    public List<Card> GetAvailableToDefendCards()
    {
        var cards = GetCardsFromZone();
        return cards.Where(c => _manager.DiscardPile.CheckIfCardCardCanRescueFromPenalty(c)).ToList();
    }

    public bool TryHighlightNextSequenceCard()
    {
        var cards = GetAvailableSequencedCard();
        return cards.Count != 0;
    }

    private List<Card> GetAvailableSequencedCard()
    {
        var cards = GetCardsFromZone();
        var card = _manager.DiscardPile.GetTopCardFromDiscardPile();
        return cards.Where(c => c.Suit == card.Suit && _manager.DiscardPile.IsNextSequencedCard(c, _playerOfThisHand)).ToList();
    }
}