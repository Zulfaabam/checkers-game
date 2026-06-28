using Spectre.Console;

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

  public void RenderGameTitle()
  {
    AnsiConsole.MarkupLine(@"
    ╔══════════════════════════════════════════════════════════════════════════════════════╗
    ║                                                                                      ║
    ║     /$$$$$$  /$$                           /$$                                       ║
    ║    /$$__  $$| $$                          | $$                                       ║
    ║   | $$  \__/| $$$$$$$   /$$$$$$   /$$$$$$$| $$   /$$  /$$$$$$   /$$$$$$   /$$$$$$$   ║
    ║   | $$      | $$__  $$ /$$__  $$ /$$_____/| $$  /$$/ /$$__  $$ /$$__  $$ /$$_____/   ║
    ║   | $$      | $$  \ $$| $$$$$$$$| $$      | $$$$$$/ | $$$$$$$$| $$  \__/|  $$$$$$    ║
    ║   | $$    $$| $$  | $$| $$_____/| $$      | $$_  $$ | $$_____/| $$       \____  $$   ║
    ║   |  $$$$$$/| $$  | $$|  $$$$$$$|  $$$$$$$| $$ \  $$|  $$$$$$$| $$       /$$$$$$$/   ║
    ║    \______/ |__/  |__/ \_______/ \_______/|__/  \__/ \_______/|__/      |_______/    ║
    ║                                                                                      ║
    ╚══════════════════════════════════════════════════════════════════════════════════════╝");
  }

  public CreateGameDto AskPlayersInfo()
  {
    string name1 = AnsiConsole.Ask<string>("Player one [red]name[/]: ");

    string name2 = AnsiConsole.Ask<string>("Player two [blue]name[/]: ");

    return new CreateGameDto
    {
      PlayerOneName = name1,
      PlayerOnePreferenceColor = ConsoleColor.Red,
      PlayerTwoName = name2,
      PlayerTwoPreferenceColor = ConsoleColor.DarkBlue
    };
  }

  public void RenderBoard()
  {
    Console.Clear();

    string manSymbol = """
    /M\
    \_/
    """;

    string kingSymbol = """
    /K\
    \_/
    """;

    Table table = new Table()
      .Ascii2Border()
      .ShowRowSeparators()
      .Title("[yellow]CHECKERS GAME[/]");

    table.AddColumn(" ", col => col.Width(4).Centered());

    for ( int i = 0; i < (int)_board.Size; i++ )
    {
      table.AddColumn($"[grey]{i}[/]", col => col.Width(7).Centered());
    }

    for( int row = 0; row < (int)_board.Size; row++ )
    {
      Markup[] rowCells = new Markup[(int)_board.Size + 1];

      rowCells[0] = new Markup($"[grey]{row}[/]");

      for( int column = 0; column < (int)_board.Size; column++ )
      {
        ICell cell = _board.Cell[row, column];
        string pieceSymbol = "\n\n";
      
        if (cell.Piece != null)
        {
          string type = cell.Piece.PieceType == PieceType.Man ? manSymbol : kingSymbol;
          string color = GetSpectreNamedColor(cell.Piece.Color);
          pieceSymbol = $"[{color}]{type}[/]";
        }
        
        rowCells[column + 1] = new Markup($@"{pieceSymbol}") { Justification = Justify.Center };
      }

      table.AddRow(rowCells);
    }

    AnsiConsole.Write(table);
  }

  public void RenderGameStatus(IPlayer currentPlayer, Dictionary<IPlayer, int> playersPieceCount)
  {
    string[] pieceCount = new string[2];

    for ( int i = 0; i < playersPieceCount.Count; i++ )
    {
      pieceCount[i] = $"{playersPieceCount.ElementAt(i).Key.Name} pieces: {playersPieceCount.ElementAt(i).Value}";
    }

    string statusText = $@"Turn: [{GetSpectreNamedColor(currentPlayer.Color)}]{currentPlayer.Name}[/], Color: [{GetSpectreNamedColor(currentPlayer.Color)}]{currentPlayer.Color}[/]
{pieceCount[0]} | {pieceCount[1]}";

    if (!string.IsNullOrEmpty(_eventMessage))
    {
      statusText += $"\n\n[aqua]Last Action:[/] {_eventMessage}";
    }

    Panel panel = new Panel(statusText)
      .Header("Game Status")
      .BorderColor(Color.DeepSkyBlue1);
  
    AnsiConsole.Write(panel);
  }

  public Position ReadChoosenPiecePosition()
  {
    while (true)
    {
      Console.Write("Choose piece position to move (ex. 0,1): ");
      string startPosition = Console.ReadLine() ?? "";

      if ( GameService.TryParsePosition(startPosition, out Position from) )
      {
        return new Position(from.X, from.Y);
      }

      AnsiConsole.MarkupLine("[bold red]Invalid piece position format. Use x,y like 2,3.[/]");
    }
  }

  public Position ReadMoveFromConsole(LegalMovesResponseDto legalMoves)
  {
    List<Position> moves = legalMoves.Moves.ToList();

    if (moves.Count == 0)
    {
      AnsiConsole.MarkupLine("[bold red]No legal moves available[/]");
      return default;
    }

    if (moves.Count == 1)
    {
      AnsiConsole.MarkupLine($"[green]Only one move available to ({moves[0].X},{moves[0].Y}). Press Enter to confirm:[/]");
      Console.ReadLine();
      return moves[0];
    }

    List<string> choices = new List<string>();

    foreach ( Position move in moves )
    {
      choices.Add($"{move.X},{move.Y}");
    }

    while (true)
    {
      string input = AnsiConsole.Prompt(
      new SelectionPrompt<string>()
        .Title("Choose move:")
        .AddChoices(choices));

      if (GameService.TryParsePosition(input, out Position toPosition))
      {
        return toPosition;
      }

      AnsiConsole.MarkupLine("[bold red]Invalid selection. Please choose a valid number.[/]");
    }
  }

  public Position ReadForcedCapturePiece(IPlayer player, Dictionary<Position, List<Position>> captureMoves)
  {
    AnsiConsole.MarkupLine($"[{GetSpectreNamedColor(player.Color)}]{player.Name}[/], [yellow]you must capture the piece.[/]");

    List<string> choices = new List<string>();

    foreach ( KeyValuePair<Position, List<Position>> kvp in captureMoves)
    {
      Position from = kvp.Key;
      foreach (Position to in kvp.Value)
      {
        choices.Add($"From {from.X},{from.Y} to {to.X},{to.Y}");
      }
    }

    while (true)
    {
      string input = AnsiConsole.Prompt(
      new SelectionPrompt<string>()
        .Title("Choose piece to capture:")
        .AddChoices(choices));

      string[] rawPosition = input.Replace("From ", "").Split(" to ");

      if (rawPosition.Length != 2)
      {
        AnsiConsole.MarkupLine("[bold red]Invalid move.[/]");
        continue;
      }

      if (GameService.TryParsePosition(rawPosition[0], out Position fromPosition))
      {
        return fromPosition;
      }

      AnsiConsole.MarkupLine("[bold red]Invalid selection. Please choose a valid move.[/]");
    }
  }

  public void ShowWinner(IPlayer winner)
  {
    FigletText figlet = new FigletText($"{winner.Name} wins!")
      .Color(winner.Color);
    
    RenderBoard();
    AnsiConsole.Write(figlet);
  }

  public void MoveEvent(object? o, MoveEventArgs args)
  {
    _eventMessage =
      $"{args.Player.Name} moved a piece from ({args.FromPosition.X}, {args.FromPosition.Y}) to ({args.ToPosition.X}, {args.ToPosition.Y})";
  }

  private static string GetSpectreNamedColor(ConsoleColor color)
  {
    return color switch
    {
      ConsoleColor.Red => "red",
      ConsoleColor.DarkBlue => "blue",
      _ => "white"
    };
  }
}
