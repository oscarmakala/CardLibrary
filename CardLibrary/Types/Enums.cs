namespace CardLibrary.Types;



public enum GamePhase
{
    CardsDealing,
    TakeOrDiscard,
    PassOrDiscard,
    PassOrDiscardNextSequencedCard,
    OverbidOrTakePenalties,
    RoundEnded,
    GameEnded
}

public enum GameAction
{
    None,
    Pass,
    TakeDiscard,
    TakeStockPile,
    Discard,
    FinishRound,
    ReportRoundWinner,
    FinishGame,
    ShowEmoticon,
    ZoomCard,
    RedrawConfirm
}