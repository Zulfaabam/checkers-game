public class MoveEventArgs : EventArgs
{
    public required IPlayer Player;
    public Position FromPosition;
    public Position ToPosition;
    public IPiece? Piece;
    public bool Crowned;
}