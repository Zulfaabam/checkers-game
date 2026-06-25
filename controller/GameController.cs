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

    var consoleRenderer = new ConsoleRenderer(res.Board, this);
    MoveMade += consoleRenderer.MoveEvent;

    while( res.Winner == null )
    {
      IPlayer player = _gameService.CurrentPlayer;
      IPiece? movedPiece = null;

      consoleRenderer.Render(player);

      // if player doesn't have any move, end the game
      // if (!_gameService.PlayerHasAnyMoves(player))
      // {
        
      // }

      // if player has capture moves, force the player to make a capture move
      // if (_gameService.PlayerHasCaptureMoves(player))
      // {
      //   Console.WriteLine("You must capture the piece");
      //   piecePos = 
      //   targetPos =
      //   movedPiece = 
      // }

      Position fromPosition = consoleRenderer.ReadChoosenPiecePosition();

      movedPiece = _gameService.GetPieceAt(fromPosition);

      while (movedPiece == null || movedPiece.Color != player.Color)
      {
        Console.WriteLine("Invalid piece position. Choose a valid piece");
        fromPosition = consoleRenderer.ReadChoosenPiecePosition();
        movedPiece = _gameService.GetPieceAt(fromPosition);
      }

      LegalMovesResponseDto legalMoves = GetLegalMoves(fromPosition);

      Position toPosition = consoleRenderer.ReadMoveFromConsole(legalMoves);

      var updatePiecePosition = new UpdatePiecePositionDto
      {
        FromPosition = fromPosition,
        ToPosition = toPosition,
      };

      UpdatePiecePositionResultDto moveResult = _gameService.TryMove(updatePiecePosition);

      if( !moveResult.MovementSucceed )
      {
        Console.WriteLine("Invalid move");
        continue;
      }

      if( movedPiece != null )
      {
        MoveMade?.Invoke(this, new MoveEventArgs
        {
          Player = player,
          FromPosition = fromPosition,
          ToPosition = toPosition,
          Piece = movedPiece,
          Crowned = moveResult.Crowned
        });
      }

      res = new GameResponseDto
      {
        CurrentPlayer = moveResult.CurrentPlayer,
        Winner = moveResult.Winner,
        Board = moveResult.Board
      };
    }

    return res;
  }

  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    return _gameService.GetLegalMoves(piecePosition) ?? new LegalMovesResponseDto();
  }

  public GameResponseDto Move(UpdatePiecePositionDto dto)
  {
    var player = _gameService.CurrentPlayer;
    var movedPiece = _gameService.GetPieceAt(dto.FromPosition);
    UpdatePiecePositionResultDto res = _gameService.TryMove(dto);

    if( !res.MovementSucceed )
      throw new InvalidOperationException("Invalid move");

    if( movedPiece != null )
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

    return new GameResponseDto { CurrentPlayer = _gameService.CurrentPlayer, Winner = res.Winner, Board = res.Board };
  }

  public GameResponseDto Restart()
  {
    Console.Clear();
    Console.WriteLine("Restarting game...");

    return Start(null);
  }
}
