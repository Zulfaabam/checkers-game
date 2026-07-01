public class Board : IBoard
{
  public ICell[,] Cell { get; set; }
  public BoardSize Size { get; set; }

  public Board(BoardSize size, ICell[,] initializedCells)
  {
    Size = size;
    Cell = initializedCells;
  }
}