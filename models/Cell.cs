public class Cell : ICell
{
  public Position Position { get; set; }
  public IPiece Piece { get; set; }

  public Cell(Position position, Dictionary<IPlayer, List<IPiece>> playersPiece)
  {
    Position = position;
  }
}