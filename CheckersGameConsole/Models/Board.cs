public class Board : IBoard
{
  public ICell[,] Cell { get; set; }
  public BoardSize Size { get; set; }

  public Board(BoardSize size, List<IPlayer> players)
  {
    Size = size;

    int boardSize = (int)size;

    int rowToFill = size switch
    {
      BoardSize.Small => 2,
      BoardSize.Standard => 3,
      BoardSize.Large => 4,
      BoardSize.VeryLarge => 5,
      _ => 3
    };

    Cell = new Cell[boardSize, boardSize];

    for( int row = 0; row < boardSize; row++ )
    {
      for( int column = 0; column < boardSize; column++ )
      {
        Cell[row, column] = new Cell(new Position(row, column), null);

        if( ( row + column ) % 2 == 1 )
        {
          if( row < rowToFill )
          {
            Cell[row, column] = new Cell(
              new Position(row, column),
              new Piece(PieceType.Man, players.First(p => !p.IsPlayerOne).Color));
          }
          else if( row >= boardSize - rowToFill )
          {
            Cell[row, column] = new Cell(
              new Position(row, column),
              new Piece(PieceType.Man, players.First(p => p.IsPlayerOne).Color));
          }
        }
      }
    }
  }
}