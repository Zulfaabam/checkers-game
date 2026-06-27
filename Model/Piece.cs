public class Piece : IPiece
{
  public PieceType PieceType { get; set; }

  public ConsoleColor Color { get; set; }

  public Piece (PieceType pieceType, ConsoleColor color)
  {
    PieceType = pieceType;
    Color = color;
  }
}