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

    return res;
  }

  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    return _gameService.GetLegalMoves(piecePosition);
  }

  public GameResponseDto Move(UpdatePiecePositionDto dto)
  {
    IPlayer player = _gameService.CurrentPlayer;
    IPiece? movedPiece = _gameService.GetPieceAt(dto.FromPosition);
    UpdatePiecePositionResultDto res = _gameService.TryMove(dto);

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
      CurrentPlayer = res.CurrentPlayer,
      Winner = res.Winner,
      Board = res.Board
    };

    return gameResponseDto;
  }

  public Dictionary<IPlayer, int> PlayersPieceCount()
  {
    return _gameService.PlayersPieceCount();
  }

  public bool PlayerHasAnyMoves(IPlayer player)
  {
    return _gameService.PlayerHasAnyMoves(player);
  }

  public List<IPlayer> GetPlayers()
  {
    return _gameService.GetPlayers();
  }

   public Dictionary<Position, List<Position>> PlayerHasCaptureMoves(IPlayer player)
  {
    return _gameService.PlayerHasCaptureMoves(player);
  }

  public IPiece? GetPieceAt(Position position)
  {
    return _gameService.GetPieceAt(position);
  }

  public List<Position> GetMovablePiecesFromPlayer(IPlayer player)
  {
    return _gameService.GetMovablePiecesFromPlayer(player);
  }

  public GameResponseDto Restart()
  {
    CreateGameDto createGameDto = new CreateGameDto
    {
      PlayerOneName = _gameService.GetPlayers().ElementAt(0).Name,
      PlayerOnePreferenceColor = _gameService.GetPlayers().ElementAt(0).Color,
      PlayerTwoName = _gameService.GetPlayers().ElementAt(1).Name,
      PlayerTwoPreferenceColor = _gameService.GetPlayers().ElementAt(1).Color,
      Size = _gameService.GetBoard().Size
    };

    return Start(createGameDto);
  }

  public void EndGame(IPlayer? winner)
  {
    if( winner == null ) return;

    GameEnded?.Invoke(this, new GameEndedEventArgs
    {
      Winner = winner,
      // TODO: determing the correct reason
      Reason = $"{winner.Name}"
    });
  }
}
