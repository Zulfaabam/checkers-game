public class MoveEventArgs : EventArgs
{
    public IPlayer Player;
    public Position FromPosition;
    public Position ToPosition;
    public IPiece Piece;
    public bool Crowned;
}