using CardLibrary.Manager;

namespace CardLibrary.Types;

public sealed class Hand : DropZone
{
    private readonly Player? _playerOfThisHand;
    private readonly DiscardPile _discardPile;
    private readonly TryFinishTheGameDelegate _tryFinishTheGameDelegate;

    public Hand(DiscardPile discardPile, TryFinishTheGameDelegate tryFinishTheGameDelegate)
    {
        _discardPile = discardPile;
        _tryFinishTheGameDelegate = tryFinishTheGameDelegate;
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
        return cards.Where(c => _discardPile.CheckIfCardCanBePutOnDiscardPile(c)).ToList();
    }

    public Card? AnalyzeHandAndDiscardPile(List<Card> cards)
    {
        var topCard = _discardPile.GetTopCardFromDiscardPile();
        // Initialize variables to store potential playable cards and score
        var playableCards = new List<Card>();
        var cardScores = new Dictionary<Card, int>();

        // Debug.Log($"AI Cards {Hand.ToJson()}");
        // Analyze hand cards
        foreach (var card in cards)
        {
            var score = CalculateCardScore(card, topCard);
            if (score <= 0) continue;
            playableCards.Add(card);
            cardScores.Add(card, score);
        }

        // Choose the card with the highest score based on your AI strategy
        // when null , No playable cards found
        return playableCards.Count > 0 && topCard != null
            ? ChooseBestCard(playableCards, cardScores, topCard)
            : null;
    }

    //todo implement logic for joker
    private int GetBaseScore(Card card)
    {
        return card.Rank switch
        {
            11 => 25, //if rank is J
            2 => 20, // if rank is 2
            12 => 2, //if rank is Q
            13 => 4, //if rank is K
            _ => card.Rank
        };
    }


    //todo add logic to accept J as an option
    private int CalculateCardScore(Card card, Card? currentCard)
    {
        var baseScore = 0;
        if (currentCard == null)
        {
            return baseScore;
        }

        if (card.Suit != currentCard.Suit && card.Rank != currentCard.Rank) return baseScore;

        baseScore = GetBaseScore(card);

        // Bonus for playing a Wild card like J
        if (card.Rank == 25)
        {
            baseScore += 8;
            // Bonus for changing to a favorable color based on hand analysis
            // int bestColorCount = GetMostCardsColor(handColors);
            // if (card.color == bestColorCount.Key)
            // {
            //     baseScore += 2;
            // }
        }
        // Penalty for discarding a matching suit (adjusted based on hand size)
        // if (card.Suit == currentCard.Suit && handColors[card.color] > 1)
        // {
        //     int discardPenalty = GetDiscardPenalty(handColors.Count);
        //     baseScore -= discardPenalty;
        // }

        // Penalty for holding onto specific cards (e.g., DrawTwo, SkipTurn)
        if (card.Rank != 2 && card.Rank != 7 && card.Rank != 8)
        {
            baseScore -= 1; // Adjust penalty based on desired AI risk tolerance
        }

        // Additional considerations based on game state and strategy
        // ... (e.g., opponent hand size analysis, remaining cards in deck)
        return baseScore;
    }


    private Card? ChooseBestCard(IEnumerable<Card> playableCards, Dictionary<Card, int> cardScores,
        Card currentCard)
    {
        // Initialize variables
        Card? bestCard = null;
        var bestScore = int.MinValue;

        // Implement your chosen selection strategy:

        // 1. Weighted Random Selection (based on scores):
        var totalWeight = cardScores.Sum(pair => Math.Max(pair.Value, 0));

        if (totalWeight > 0)
        {
            var random = new Random();
            var randomValue = random.Next(totalWeight);
            var currentWeight = 0;
            foreach (KeyValuePair<Card, int> pair in cardScores)
            {
                currentWeight += Math.Max(pair.Value, 0);
                if (randomValue >= currentWeight) continue;
                bestCard = pair.Key;
                break;
            }
        }

        // 2. Heuristic-based Selection (consider additional factors):
        // Example: Prioritize matching color over Wild cards if hand size is small.
        if (bestCard != null || Cards.Count >= 5) return bestCard;
        foreach (var card in playableCards.Where(card =>
                     card.Suit == currentCard.Suit && cardScores[card] > bestScore))
        {
            bestCard = card;
            bestScore = cardScores[card];
        }

        // 3. Advanced Techniques (optional):
        // - Machine learning for complex decision-making
        // - Minimax algorithm for strategic reasoning

        // Return the chosen best card
        return bestCard;
    }

    public void OnCardDiscard()
    {
        var card = _playerOfThisHand?.CardOnAction;
        if (card != null)
        {
            Cards.Remove(card);
        }

        if (Cards.Count == 0)
        {
        }
    }

    public List<Card> GetAvailableToDefendCards()
    {
        var cards = GetCardsFromZone();
        return cards.Where(c => _discardPile.CheckIfCardCardCanRescueFromPenalty(c)).ToList();
    }

    public bool TryHighlightNextSequenceCard()
    {
        var cards = GetAvailableSequencedCard();
        return cards.Count != 0;
    }

    private List<Card> GetAvailableSequencedCard()
    {
        var cards = GetCardsFromZone();
        var card = _discardPile.GetTopCardFromDiscardPile();
        return card == null
            ? cards
            : cards.Where(c => c.Suit == card.Suit && _discardPile.IsNextSequencedCard(c, _playerOfThisHand))
                .ToList();
    }
}