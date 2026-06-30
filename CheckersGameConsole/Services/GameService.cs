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

  public GameResponseDto InitializeBoard(CreateGameDto? dto)
  {
    BoardSize size = dto?.Size ?? BoardSize.Standard;

    // Reset player details, board size and pieces on board
    if (dto != null)
    {
      IPlayer? p1 = _players.Find(p => p.IsPlayerOne);
      if (p1 != null)
      {
        p1.Name = dto.PlayerOneName;
        p1.Color = dto.PlayerOnePreferenceColor;
      }
      IPlayer? p2 = _players.Find(p => !p.IsPlayerOne);
      if (p2 != null)
      {
        p2.Name = dto.PlayerTwoName;
        p2.Color = dto.PlayerTwoPreferenceColor;
      }
    }

    _board = new Board(size, _players);

    CurrentPlayer = _players.Find(p => p.IsPlayerOne) ?? _players[0];

    PlayersPieces = _players.ToDictionary(
      player => player,
      FilterPiecesByPlayer
    );

    return new GameResponseDto
    {
      CurrentPlayer = CurrentPlayer,
      Winner = null,
      Board = _board,
    };
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

    if( piece == null ) return new LegalMovesResponseDto { Moves = legalMoves };

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

    return new LegalMovesResponseDto { Moves = legalMoves };
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

  public IPlayer GetWinner()
  {
    return PlayersPieces
      .Where(player => player.Value.Count != 0)
      .Select(p => p.Key)
      .First();
  }

  public Dictionary<IPlayer, int> PlayersPieceCount()
  {
    return PlayersPieces.ToDictionary(
      player => player.Key,
      player => player.Value.Count
    );
  }

  public static bool TryParsePosition(string input, out Position position)
  {
    position = default;

    if( string.IsNullOrWhiteSpace(input) ) return false;

    string[] parts = input.Split(
      ',',
      StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
    );

    if( parts.Length != 2 ) return false;

    if( int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y) )
    {
      position = new Position(x, y);
      return true;
    }

    return false;
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

    return new UpdatePiecePositionResultDto
    {
      MovementSucceed = true,
      Captured = isCaptured,
      Crowned = isCrowned,
      HasMoreCaptures = canCaptureAgain,
      CurrentPlayer = CurrentPlayer,
      Winner = GetWinner(),
      Board = _board,
    };
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

  private void EvaluateMoveDirection(Position currentPos, int dx, int dy, List<Position> legalMoves)
  {
    Position targetPos = new Position(currentPos.X + dx, currentPos.Y + dy);
    if( !IsInside(targetPos) ) return;

    IPiece? pieceAtTarget = GetPieceAt(targetPos);
    if( pieceAtTarget == null )
    {
      legalMoves.Add(targetPos);
    }
    else if( pieceAtTarget.Color != CurrentPlayer.Color )
    {
      Position jumpPos = new Position(currentPos.X + dx * 2, currentPos.Y + dy * 2);

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
    return new UpdatePiecePositionResultDto
    {
      MovementSucceed = succeed,
      Crowned = crowned,
      Captured = captured,
      HasMoreCaptures = hasMoreCaptures,
      CurrentPlayer = CurrentPlayer,
      Winner = winner,
      Board = _board,
    };
  }

  private List<IPiece> FilterPiecesByPlayer(IPlayer player)
  {
    return AllPieces().Where(p => p.Color == player.Color).ToList();
  }
}
