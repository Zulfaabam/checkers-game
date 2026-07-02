public class GameService : IGameService
{
  private IBoard _board;
  private List<IPlayer> _players;

  public IPlayer CurrentPlayer { get; set; }
  public Dictionary<IPlayer, List<IPiece>> PlayersPieces { get; set; }

  public GameService(IBoard board, List<IPlayer> players)
  {
    _board = board;
    _players = players;

    CurrentPlayer = players.Find(p => p.IsPlayerOne) ?? players[0];

    PlayersPieces = _players.ToDictionary(
      player => player,
      FilterPiecesByPlayer
    );
  }

  /// <summary>
  /// Initializes the board cells with pieces in their starting positions.
  /// </summary>
  /// <param name="size">The size of the board</param>
  /// <param name="players">The list of players</param>
  /// <returns>A 2D array of initialized cells</returns>
  public static ICell[,] InitializeBoardCells(BoardSize size, List<IPlayer> players)
  {
    int boardSize = (int)size;

    int rowToFill = size switch
    {
      BoardSize.Small => 2,
      BoardSize.Standard => 3,
      BoardSize.Large => 4,
      BoardSize.VeryLarge => 5,
      _ => 3
    };

    ICell[,] cells = new Cell[boardSize, boardSize];

    for( int row = 0; row < boardSize; row++ )
    {
      for( int column = 0; column < boardSize; column++ )
      {
        cells[row, column] = new Cell(new Position(row, column), null);

        if( ( row + column ) % 2 == 1 )
        {
          if( row < rowToFill )
          {
            cells[row, column] = new Cell(
              new Position(row, column),
              new Piece(PieceType.Man, players.First(p => !p.IsPlayerOne).Color));
          }
          else if( row >= boardSize - rowToFill )
          {
            cells[row, column] = new Cell(
              new Position(row, column),
              new Piece(PieceType.Man, players.First(p => p.IsPlayerOne).Color));
          }
        }
      }
    }

    return cells;
  }

  public static bool TryParsePosition(string input, out Position position)
  {
    position = default;

    if( string.IsNullOrWhiteSpace(input) )
    {
      return false;
    }

    string[] parts = input.Split(
      ',',
      StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
    );

    if( parts.Length != 2 )
    {
      return false;
    }

    if( int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y) )
    {
      position = new Position(x, y);
      return true;
    }

    return false;
  }

  public GameResponseDto InitializeBoard(CreateGameDto? createGameDto)
  {
    BoardSize size = createGameDto?.Size ?? BoardSize.Standard;

    // Reset player details, board size and pieces on board
    if( createGameDto != null )
    {
      IPlayer? player1 = _players.Find(p => p.IsPlayerOne);
      if( player1 != null )
      {
        player1.Name = createGameDto.PlayerOneName;
        player1.Color = createGameDto.PlayerOnePreferenceColor;
      }
      IPlayer? player2 = _players.Find(p => !p.IsPlayerOne);
      if( player2 != null )
      {
        player2.Name = createGameDto.PlayerTwoName;
        player2.Color = createGameDto.PlayerTwoPreferenceColor;
      }
    }

    ICell[,] initializedCells = InitializeBoardCells(size, _players);
    _board = new Board(size, initializedCells);

    CurrentPlayer = _players.Find(p => p.IsPlayerOne) ?? _players[0];

    PlayersPieces = _players.ToDictionary(
      player => player,
      FilterPiecesByPlayer
    );

    GameResponseDto gameResponseDto = new GameResponseDto
    {
      CurrentPlayer = CurrentPlayer,
      Winner = null,
      Board = _board,
    };

    return gameResponseDto;
  }

  public UpdatePiecePositionResultDto TryMove(UpdatePiecePositionDto dto)
  {
    if( !IsInside(dto.FromPosition) || !IsInside(dto.ToPosition) )
    {
      return CreateMoveResult();
    }

    IPiece? piece = GetPieceAt(dto.FromPosition);

    if( piece == null )
    {
      return CreateMoveResult();
    }

    LegalMovesResponseDto legalMoves = GetLegalMoves(dto.FromPosition);

    if( !legalMoves.Moves.Contains(dto.ToPosition) )
    {
      return CreateMoveResult();
    }

    UpdatePiecePositionResultDto res = PerformMove(piece, dto.FromPosition, dto.ToPosition);

    return CreateMoveResult(
      res.MovementSucceed,
      res.Crowned,
      res.Captured,
      res.HasMoreCaptures
    );
  }

  public IPiece? GetPieceAt(Position position)
  {
    return _board.Cell[position.X, position.Y].Piece;
  }

  public IEnumerable<IPiece> AllPieces()
  {
    return _board
      .Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null)
      .Select(cell => cell.Piece!);
  }

  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    IPiece? piece = GetPieceAt(piecePosition);
    List<Position> legalMoves = [];
    LegalMovesResponseDto legalMovesResponseDto = new LegalMovesResponseDto { Moves = legalMoves };

    if( piece == null )
    {
      return legalMovesResponseDto;
    }

    if( CurrentPlayer.IsPlayerOne )
    {
      if( piece.PieceType == PieceType.Man )
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
      }
      else if( piece.PieceType == PieceType.King )
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
    }
    else
    {
      if( piece.PieceType == PieceType.Man )
      {
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
      else if( piece.PieceType == PieceType.King )
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
    }

    legalMovesResponseDto.Moves = legalMoves;

    return legalMovesResponseDto;
  }

  public Dictionary<Position, List<Position>> PlayerHasCaptureMoves(IPlayer player)
  {
    return _board.Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
      .Select(cell => new
      {
        From = cell.Position,
        Captures = GetLegalMoves(cell.Position).Moves
          .Where(move => Math.Abs(move.X - cell.Position.X) > 1)
          .ToList()
      })
      .Where(x => x.Captures.Any())
      .ToDictionary(x => x.From, x => x.Captures);
  }

  public bool PlayerHasAnyMoves(IPlayer player)
  {
    return _board
      .Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
      .Select(cell => GetLegalMoves(cell.Position))
      .Any(m => m.Moves.Any());
  }

  public List<Position> GetMovablePiecesFromPlayer(IPlayer player)
  {
    List<Position> positions = _board.Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
      .Where(cell => GetLegalMoves(cell.Position).Moves.Any())
      .Select(cell => cell.Position)
      .ToList();

    return positions;
  }

  public List<IPlayer> GetPlayers()
  {
    return _players;
  }

  public IPlayer? GetWinner()
  {
    List<IPlayer> playersWithRemainingPieces = PlayersPieces
      .Where(player => player.Value.Count != 0)
      .Select(p => p.Key)
      .ToList();

    bool thereIsAWinner = playersWithRemainingPieces.Count == 1;

    if( thereIsAWinner )
    {
      IPlayer theWinner = playersWithRemainingPieces[0];

      return theWinner;
    }

    return null;
  }

  public Dictionary<IPlayer, int> PlayersPieceCount()
  {
    return PlayersPieces.ToDictionary(
      player => player.Key,
      player => player.Value.Count
    );
  }

  public IBoard GetBoard()
  {
    return _board;
  }

  private UpdatePiecePositionResultDto PerformMove(IPiece piece, Position from, Position to)
  {
    List<IPiece> jumpedPieces = [];
    bool isCaptured = false;
    bool isCrowned = false;

    bool isManCrowned = piece.PieceType == PieceType.Man &&
      ( ( to.X == 0 && CurrentPlayer.IsPlayerOne ) ||
        ( to.X == (int)_board.Size - 1 && !CurrentPlayer.IsPlayerOne ) );

    if( isManCrowned )
    {
      piece.PieceType = PieceType.King;
      isCrowned = true;
    }

    bool isCapturing = Math.Abs(to.X - from.X) > 1;

    if( isCapturing )
    {
      int jumpedX = from.X + ( to.X - from.X ) / 2;
      int jumpedY = from.Y + ( to.Y - from.Y ) / 2;

      IPiece? jumpedPiece = _board.Cell[jumpedX, jumpedY].Piece;
      if( jumpedPiece != null )
      {
        jumpedPieces.Add(jumpedPiece);
        _board.Cell[jumpedX, jumpedY].Piece = null;

        IPlayer? owner = _players.FirstOrDefault(player => player.Color == jumpedPiece.Color);
        if( owner != null )
        {
          PlayersPieces[owner].Remove(jumpedPiece);
        }

        isCaptured = true;
      }
    }

    _board.Cell[to.X, to.Y].Piece = piece;
    _board.Cell[from.X, from.Y].Piece = null;

    bool canCaptureAgain = isCaptured && CanCaptureAgain(to);

    if( !canCaptureAgain ) SwitchTurn();

    UpdatePiecePositionResultDto result = new UpdatePiecePositionResultDto
    {
      MovementSucceed = true,
      Captured = isCaptured,
      Crowned = isCrowned,
      HasMoreCaptures = canCaptureAgain,
      CurrentPlayer = CurrentPlayer,
      Winner = GetWinner(),
      Board = _board,
    };

    return result;
  }

  private void SwitchTurn()
  {
    CurrentPlayer = _players.First(p => p != CurrentPlayer);
  }

  private bool IsInside(Position position)
  {
    return position.X >= 0
      && position.X < (int)_board.Size
      && position.Y >= 0
      && position.Y < (int)_board.Size;
  }

  private void EvaluateMoveDirection(Position currentPos, int xPositionAddition, int yPositionAddition, List<Position> legalMoves)
  {
    Position targetPos = new Position(currentPos.X + xPositionAddition, currentPos.Y + yPositionAddition);
    if( !IsInside(targetPos) )
    {
      return;
    }

    IPiece? pieceAtTarget = GetPieceAt(targetPos);
    if( pieceAtTarget == null )
    {
      legalMoves.Add(targetPos);
    }
    else if( pieceAtTarget.Color != CurrentPlayer.Color )
    {
      Position jumpPos = new Position(currentPos.X + xPositionAddition * 2, currentPos.Y + yPositionAddition * 2);

      if( IsInside(jumpPos) && GetPieceAt(jumpPos) == null )
      {
        legalMoves.Add(jumpPos);
      }
    }
  }

  private bool CanCaptureAgain(Position currentPosition)
  {
    LegalMovesResponseDto legalMoves = GetLegalMoves(currentPosition);

    return legalMoves.Moves.Any(move => Math.Abs(move.X - currentPosition.X) > 1);
  }

  private UpdatePiecePositionResultDto CreateMoveResult(
    bool succeed = false,
    bool crowned = false,
    bool captured = false,
    bool hasMoreCaptures = false,
    IPlayer? winner = null)
  {
    UpdatePiecePositionResultDto result = new UpdatePiecePositionResultDto
    {
      MovementSucceed = succeed,
      Crowned = crowned,
      Captured = captured,
      HasMoreCaptures = hasMoreCaptures,
      CurrentPlayer = CurrentPlayer,
      Winner = winner,
      Board = _board,
    };

    return result;
  }

  private List<IPiece> FilterPiecesByPlayer(IPlayer player)
  {
    return AllPieces().Where(p => p.Color == player.Color).ToList();
  }
}
