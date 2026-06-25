public class ConsoleRenderer
{
  private IBoard _board;
  private string _eventMessage = "";
  private GameController _controller;

  public ConsoleRenderer(IBoard board, GameController controller)
  {
    _board = board;
    _controller = controller;
  }

  public void Render(IPlayer currentPlayer)
  {
    Console.Clear();

    for( int row = -1; row < (int)_board.Size; row++ )
    {
      for( int column = -1; column < (int)_board.Size; column++ )
      {
        Console.ForegroundColor = ConsoleColor.Gray;
        if( row < 0 && column < 0 )
        {
          Console.Write($"X,Y ");
        }
        else if( row < 0 )
        {
          Console.Write($" {column} ");
        }
        else if( column < 0 )
        {
          Console.Write($" {row}  ");
        }
        else
        {
          var cell = _board.Cell[row, column];

          if( cell.Piece != null )
          {
            Console.ForegroundColor = cell.Piece.Color;
            Console.Write(cell.Piece.PieceType == PieceType.Man ? "[M]" : "[K]");
            // Console.Write(cell.Piece.PieceType == PieceType.Man ? $"[{row},{column}]" : $"[{row},{column}]");
          }
          else
          {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write($"[ ]");
          }
        }
      }

      Console.WriteLine();
    }

    Console.ForegroundColor = ConsoleColor.Green;

    Console.WriteLine();
    Console.WriteLine($"Turn: {currentPlayer.Name}, Color: {currentPlayer.Color}");
  }

  public Position ReadChoosenPiecePosition()
  {
    while (true)
    {
      Console.Write("Choose piece position to be moved (ex. 0,1):");
      string startPosition = Console.ReadLine();

      if ( GameService.TryParsePosition(startPosition, out Position from) )
      {
        return new Position(from.X, from.Y);
      }

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("Invalid piece position format. Use x,y like 2,3.");
      Console.ForegroundColor = ConsoleColor.Green;
    }
  }

  public Position ReadMoveFromConsole(LegalMovesResponseDto legalMoves)
  {
    Console.ForegroundColor = ConsoleColor.Green;

    while (true)
    {
      Console.WriteLine("Legal moves:");

      foreach (Position pos in legalMoves.Moves)
      {
        Console.WriteLine($"- {pos.X},{pos.Y}");
      }

      Console.Write("Where to move (ex. 0,1):");
      string endPosition = Console.ReadLine();

      if( GameService.TryParsePosition(endPosition, out Position to) )
      {
        if( legalMoves.Moves.Contains(to) )
        {
          return new Position(to.X, to.Y);
        }
        else
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Invalid move. Choose a valid position");
          Console.ForegroundColor = ConsoleColor.Green;
        }
      } 
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Invalid position format. Use x,y like 2,3.");
        Console.ForegroundColor = ConsoleColor.Green;
      }
    }
  }

  public void ForceCaptureMove(IPlayer player, LegalMovesResponseDto legalMoves)
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{player.Name} has capture moves from: ... to:");

    foreach (Position pos in legalMoves.Moves)
    {
      Console.WriteLine($"- {pos.X},{pos.Y}");
    }

    Console.WriteLine($"You must capture the piece.");
    Console.ForegroundColor = ConsoleColor.Green;
  }

  public void MoveEvent(object? o, MoveEventArgs args)
  {
    _eventMessage =
      $"Moved piece from ({args.FromPosition.X}, {args.FromPosition.Y}) to ({args.ToPosition.X}, {args.ToPosition.Y})";
    Render(args.Player);
    Console.WriteLine(_eventMessage);
  }
}
