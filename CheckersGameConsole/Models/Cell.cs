public class Cell : ICell
{
  public Position Position { get; set; }
  public IPiece? Piece { get; set; }

  public Cell(Position position, IPiece? piece = null)
  {
    Position = position;
    Piece = piece;
  }
}