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
      Position fromPosition = default;
      LegalMovesResponseDto legalMoves;

      consoleRenderer.Render(player);

      // if player doesn't have any move, end the game
      // if (!_gameService.PlayerHasAnyMoves(player))
      // {

      // }

      // if player has capture moves, force the player to make a capture move
      Dictionary<Position, List<Position>> captureMoves = _gameService.PlayerHasCaptureMoves(player);
      if (captureMoves.Count > 0)
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

        while (movedPiece == null || movedPiece.Color != player.Color)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine("Wrong piece! Please choose your piece");
          Console.ForegroundColor = ConsoleColor.Green;
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
      Console.WriteLine("Invalid move");
    }

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
    Console.WriteLine("Restarting game...");

    return Start(null);
  }
}
