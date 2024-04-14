namespace CardLibrary.Types;

public delegate void CardsDealtEventHandler(object sender, CardsDealtArgs e);

public abstract class GameEventArgs : EventArgs
{
}

public class CardsDealtArgs : GameEventArgs
{
}