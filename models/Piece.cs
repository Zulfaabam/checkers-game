public class Piece : IPiece
{
  public PieceType PieceType { get; set; }

  public Piece (PieceType pieceType) 
  {
    PieceType = pieceType;
  }
}