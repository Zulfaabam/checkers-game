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

    // Clear previous event subscribers
    MoveMade = null;
    GameEnded = null;

    ConsoleRenderer consoleRenderer = new ConsoleRenderer(res.Board, this);
    MoveMade += consoleRenderer.MoveEvent;
    GameEnded += consoleRenderer.GameEndedEvent;

    while( res.Winner == null )
    {
      IPlayer player = _gameService.CurrentPlayer;
      IPiece? movedPiece = null;
      Position fromPosition = default;
      Position toPosition = default;
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
        fromPosition = consoleRenderer.ReadForcedCapturePiece(player, captureMoves);
        movedPiece = _gameService.GetPieceAt(fromPosition);
        legalMoves = new LegalMovesResponseDto { Moves = captureMoves[fromPosition] };
        toPosition = legalMoves.Moves.First();
      }
      else
      {
        List<Position> movablePositions = _gameService.GetMovablePiecesFromPlayer(player);
        fromPosition = consoleRenderer.ReadChoosenPiecePosition(movablePositions);
        movedPiece = _gameService.GetPieceAt(fromPosition);

        legalMoves = GetLegalMoves(fromPosition);
        toPosition = consoleRenderer.ReadMoveFromConsole(legalMoves);
      }

      UpdatePiecePositionDto updatePiecePosition = new UpdatePiecePositionDto
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

    if( res.Winner != null )
    {
      GameEnded?.Invoke(this, new GameEndedEventArgs
      {
        Winner = res.Winner,
        Reason = $"{res.Winner.Name}"
      });

      bool playAgain = consoleRenderer.PromptPostGame();
      if( playAgain )
      {
        return Restart();
      }
    }

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

    GameResponseDto gameResponseDto = new GameResponseDto
    {
      CurrentPlayer = _gameService.CurrentPlayer,
      Winner = res.Winner,
      Board = res.Board
    };

    return gameResponseDto;
  }

  public GameResponseDto Restart()
  {
    Console.Clear();

    CreateGameDto createGameDto = new CreateGameDto
    {
      PlayerOneName = _gameService.GetPlayers().ElementAt(0).Name,
      PlayerOnePreferenceColor = _gameService.GetPlayers().ElementAt(0).Color,
      PlayerTwoName = _gameService.GetPlayers().ElementAt(1).Name,
      PlayerTwoPreferenceColor = _gameService.GetPlayers().ElementAt(1).Color,
      Size = BoardSize.Standard
    };

    return Start(createGameDto);
  }
}
