using Spectre.Console;

public class GameController
{
  private IGameService _gameService;

  public event EventHandler<MoveEventArgs>? MoveMade;
  public event EventHandler<GameEndedEventArgs>? GameEnded;

  public GameController(IGameService gameService)
  {
    _gameService = gameService;
  }

  public GameResponseDto Start(CreateGameDto dto)
  {
    GameResponseDto res = _gameService.InitializeBoard(dto);

    // Clear previous event subscribers to avoid duplicates on restarts
    MoveMade = null;

    var consoleRenderer = new ConsoleRenderer(res.Board, this);
    MoveMade += consoleRenderer.MoveEvent;

    while( res.Winner == null )
    {
      IPlayer player = _gameService.CurrentPlayer;
      IPiece? movedPiece = null;
      Position fromPosition = default;
      LegalMovesResponseDto legalMoves;

      consoleRenderer.RenderBoard();
      consoleRenderer.RenderGameStatus(player, _gameService.PlayersPieceCount());

      if( !_gameService.PlayerHasAnyMoves(player) )
      {
        res.Winner = _gameService.GetPlayers().FirstOrDefault(p => p.Color != player.Color);
        continue;
      }

      Dictionary<Position, List<Position>> captureMoves = _gameService.PlayerHasCaptureMoves(player);
      
      if( captureMoves.Count > 0 )
      {
        consoleRenderer.ForceCaptureMove(player, captureMoves);
        fromPosition = consoleRenderer.ReadForcedCapturePiece(captureMoves);
        movedPiece = _gameService.GetPieceAt(fromPosition);
        legalMoves = new LegalMovesResponseDto { Moves = captureMoves[fromPosition] };
      }
      else
      {
        fromPosition = consoleRenderer.ReadChoosenPiecePosition();
        movedPiece = _gameService.GetPieceAt(fromPosition);

        while( movedPiece == null || movedPiece.Color != player.Color )
        {
          AnsiConsole.MarkupLine("[bold red]Wrong piece! Please choose your piece[/]");
          fromPosition = consoleRenderer.ReadChoosenPiecePosition();
          movedPiece = _gameService.GetPieceAt(fromPosition);
        }

        legalMoves = GetLegalMoves(fromPosition);
      }

      Position toPosition = consoleRenderer.ReadMoveFromConsole(legalMoves);

      var updatePiecePosition = new UpdatePiecePositionDto
      {
        FromPosition = fromPosition,
        ToPosition = toPosition,
      };

      GameResponseDto moveResult = Move(updatePiecePosition);

      res = new GameResponseDto
      {
        CurrentPlayer = moveResult.CurrentPlayer,
        Winner = moveResult.Winner,
        Board = moveResult.Board
      };
    }

    consoleRenderer.ShowWinner(res.Winner);

    return res;
  }

  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    return _gameService.GetLegalMoves(piecePosition) ?? new LegalMovesResponseDto();
  }

  public GameResponseDto Move(UpdatePiecePositionDto dto)
  {
    IPlayer player = _gameService.CurrentPlayer;
    IPiece? movedPiece = _gameService.GetPieceAt(dto.FromPosition);
    UpdatePiecePositionResultDto res = _gameService.TryMove(dto);

    if( !res.MovementSucceed )
    {
      AnsiConsole.MarkupLine("[bold red]Invalid move[/]");
    }

    if( res.MovementSucceed && movedPiece != null )
    {
      MoveMade?.Invoke(this, new MoveEventArgs
      {
        Player = player,
        FromPosition = dto.FromPosition,
        ToPosition = dto.ToPosition,
        Piece = movedPiece,
        Crowned = res.Crowned
      });
    }

    return new GameResponseDto
    {
      CurrentPlayer = _gameService.CurrentPlayer,
      Winner = res.Winner,
      Board = res.Board
    };
  }

  public GameResponseDto Restart()
  {
    Console.Clear();
    AnsiConsole.MarkupLine("[yellow]Restarting game...[/]");

    return Start(new CreateGameDto
    {
      PlayerOneName = _gameService.GetPlayers().ElementAt(0).Name,
      PlayerOnePreferenceColor = _gameService.GetPlayers().ElementAt(0).Color,
      PlayerTwoName = _gameService.GetPlayers().ElementAt(1).Name,
      PlayerTwoPreferenceColor = _gameService.GetPlayers().ElementAt(1).Color,
      Size = BoardSize.Standard
    });
  }
}
