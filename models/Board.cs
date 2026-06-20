public class Board : IBoard
{
  public ICell[,] Cell { get;}

  public BoardSize Size { get; set; }

  public Board (BoardSize size, Dictionary<IPlayer, List<IPiece>> playersPiece)
  {
    Size = size;

    int boardSize = size switch
    {
      BoardSize.Small => 6,
      BoardSize.Standard => 8,
      BoardSize.Large => 10,
      BoardSize.VeryLarge => 12,
      _ => throw new ArgumentOutOfRangeException()
    };

    Cell = new Cell[boardSize, boardSize];

    for (int row = 0; row < boardSize; row++)
    {
      for (int column = 0; column < boardSize; column++)
      {
          Cell[row, column] = new Cell(new Position(row, column), playersPiece);
      }
    }
  }
}