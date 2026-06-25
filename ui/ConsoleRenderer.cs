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
    List<Position> moves = legalMoves.Moves.ToList();

    if (moves.Count == 0)
    {
      throw new InvalidOperationException("No legal moves available.");
    }

    if (moves.Count == 1)
    {
      Console.Write($"Only one move available to ({moves[0].X},{moves[0].Y}). Press Enter to confirm: ");
      Console.ReadLine();
      return moves[0];
    }

    Console.WriteLine("Legal moves:");
    for (int i = 0; i < moves.Count; i++)
    {
      Console.WriteLine($"{i + 1}. Move to ({moves[i].X},{moves[i].Y})");
    }

    while (true)
    {
      Console.Write($"Choose move (1-{moves.Count}) [Default is 1, press Enter]: ");
      string input = Console.ReadLine() ?? "";

      if (string.IsNullOrWhiteSpace(input))
      {
        return moves[0];
      }

      if (int.TryParse(input, out int choice) && choice >= 1 && choice <= moves.Count)
      {
        return moves[choice - 1];
      }

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("Invalid selection. Please choose a valid number.");
      Console.ForegroundColor = ConsoleColor.Green;
    }
  }

  public void ForceCaptureMove(IPlayer player, Dictionary<Position, List<Position>> captureMoves)
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"{player.Name} has capture moves:");

    foreach (var kvp in captureMoves)
    {
      Position from = kvp.Key;
      foreach (Position to in kvp.Value)
      {
        Console.WriteLine($"- From {from.X},{from.Y} to {to.X},{to.Y}");
      }
    }

    Console.WriteLine($"You must capture the piece.");
    Console.ForegroundColor = ConsoleColor.Green;
  }

  public Position ReadForcedCapturePiece(Dictionary<Position, List<Position>> captureMoves)
  {
    List<Position> forcedPieces = captureMoves.Keys.ToList();

    if (forcedPieces.Count == 1)
    {
      Console.Write($"Piece at ({forcedPieces[0].X},{forcedPieces[0].Y}) must capture. Press Enter to select: ");
      Console.ReadLine();
      return forcedPieces[0];
    }

    Console.WriteLine("Multiple pieces can capture. Choose one to move:");
    for (int i = 0; i < forcedPieces.Count; i++)
    {
      Console.WriteLine($"{i + 1}. Piece at ({forcedPieces[i].X},{forcedPieces[i].Y})");
    }

    while (true)
    {
      Console.Write($"Enter number (1-{forcedPieces.Count}) [Default is 1, press Enter]: ");
      string input = Console.ReadLine() ?? "";

      if (string.IsNullOrWhiteSpace(input))
      {
        return forcedPieces[0];
      }

      if (int.TryParse(input, out int choice) && choice >= 1 && choice <= forcedPieces.Count)
      {
        return forcedPieces[choice - 1];
      }

      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine("Invalid selection. Please choose a valid number.");
      Console.ForegroundColor = ConsoleColor.Green;
    }
  }

  public void MoveEvent(object? o, MoveEventArgs args)
  {
    _eventMessage =
      $"Moved piece from ({args.FromPosition.X}, {args.FromPosition.Y}) to ({args.ToPosition.X}, {args.ToPosition.Y})";
    Render(args.Player);
    Console.WriteLine(_eventMessage);
  }
}
