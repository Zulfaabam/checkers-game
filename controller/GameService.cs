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
      player =>
        _board
          .Cell.OfType<ICell>()
          .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
          .Select(cell => cell.Piece!)
          .ToList()
    );
  }

  public GameResponseDto InitializeBoard(CreateGameDto? dto)
  {
    return new GameResponseDto
    {
      CurrentPlayer = CurrentPlayer,
      Winner = null,
      Board = _board,
    };
  }

  public UpdatePiecePositionResultDto TryMove(UpdatePiecePositionDto dto)
  {
    // check from and to positions are inside the board
    if (!IsInside(dto.FromPosition) || !IsInside(dto.ToPosition))
    {
      return new UpdatePiecePositionResultDto { MovementSucceed = false };
    }

    // check if there is a piece at the from position
    IPiece? piece = GetPieceAt(dto.FromPosition);

    if (piece == null)
    {
      return new UpdatePiecePositionResultDto { MovementSucceed = false };
    }

    // IPiece? enemyPiece = GetPieceAt(dto.ToPosition);
    //
    // if( enemyPiece != null )
    // {
    //
    // }

    // check all legal moves
    LegalMovesResponseDto legalMoves = GetLegalMoves(dto.FromPosition);

    if (!legalMoves.Moves.Contains(dto.ToPosition))
    {
      return new UpdatePiecePositionResultDto { MovementSucceed = false };
    }

    // perform the move
    UpdatePiecePositionResultDto res = PerformMove(piece, dto.FromPosition, dto.ToPosition);

    if (res.MovementSucceed) SwitchTurn();

    // return the result
    return new UpdatePiecePositionResultDto 
    { 
      MovementSucceed = res.MovementSucceed,
      Captured = res.Captured,
    };
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

  public LegalMovesResponseDto GetLegalMovesFromPlayer(IPlayer player)
  {
    IEnumerable<Position> legalMoves = new List<Position>();

    return new LegalMovesResponseDto { Moves = legalMoves };
  }

  public LegalMovesResponseDto GetLegalMovesFromPiece(IPiece piece)
  {
    IEnumerable<Position> legalMoves = new List<Position>();

    return new LegalMovesResponseDto { Moves = legalMoves };
  }

  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    var piece = GetPieceAt(piecePosition) ?? throw new ArgumentException("Invalid piece position");
    List<Position> legalMoves = new List<Position>();

    if (CurrentPlayer.IsPlayerOne)
    {
      if (piece.PieceType == PieceType.Man)
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
      }
      else if (piece.PieceType == PieceType.King)
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
    }
    else
    {
      if (piece.PieceType == PieceType.Man)
      {
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
      else if (piece.PieceType == PieceType.King)
      {
        EvaluateMoveDirection(piecePosition, -1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, -1, 1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, -1, legalMoves);
        EvaluateMoveDirection(piecePosition, 1, 1, legalMoves);
      }
    }

    return new LegalMovesResponseDto { Moves = legalMoves };
  }

  public bool PlayerHasCaptureMoves(IPlayer player)
  {
    return _board
      .Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
      .Any(cell => GetLegalMoves(cell.Position).Moves
        .Any(move => Math.Abs(move.X - cell.Position.X) > 1)
      );
  }

  public bool PlayerHasAnyMoves(IPlayer player)
  {
    return _board
      .Cell.OfType<ICell>()
      .Where(cell => cell.Piece != null && cell.Piece.Color == player.Color)
      .Select(cell => GetLegalMoves(cell.Position))
      .Any(m => m.Moves.Any());
  }

  public static bool TryParsePosition(string input, out Position position)
  {
    position = default;

    if (string.IsNullOrWhiteSpace(input)) return false;

    string[] parts = input.Split(
      ',',
      StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
    );

    if (parts.Length != 2) return false;

    if (int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
    {
      position = new Position(x, y);
      return true;
    }

    return false;
  }

  private UpdatePiecePositionResultDto PerformMove(IPiece piece, Position from, Position to)
  {
    List<IPiece> jumpedPieces = [];

    // check if captured any piece
    if (Math.Abs(to.X - from.X) > 1)
    {
      int jumpedX = from.X + (to.X - from.X) / 2;
      int jumpedY = from.Y + (to.Y - from.Y) / 2;
      
      IPiece? jumpedPiece = _board.Cell[jumpedX, jumpedY].Piece;

      if (jumpedPiece != null)
      {
        jumpedPieces.Add(jumpedPiece);
        _board.Cell[jumpedX, jumpedY].Piece = null;
        PlayersPieces[CurrentPlayer].Remove(jumpedPiece);
      }
    }

    // after one check, check again if can capture again
    // while(CanCaptureAgain(piece, to))
    // {
      
    // }

    if ((to.X == 0 && CurrentPlayer.IsPlayerOne) || 
      (to.X == (int)_board.Size - 1 && !CurrentPlayer.IsPlayerOne))
    {
      piece.PieceType = PieceType.King;
    }

    _board.Cell[to.X, to.Y].Piece = piece;

    _board.Cell[from.X, from.Y].Piece = null;

    return new UpdatePiecePositionResultDto 
    { 
      MovementSucceed = true, 
      Captured = jumpedPieces.Count > 0 
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
    var targetPos = new Position(currentPos.X + dx, currentPos.Y + dy);
    if (!IsInside(targetPos)) return;

    var pieceAtTarget = GetPieceAt(targetPos);
    if (pieceAtTarget == null)
    {
      legalMoves.Add(targetPos);
    }
    else if (pieceAtTarget.Color != CurrentPlayer.Color)
    {
      var jumpPos = new Position(currentPos.X + dx * 2, currentPos.Y + dy * 2);
      
      if (IsInside(jumpPos) && GetPieceAt(jumpPos) == null)
      {
        legalMoves.Add(jumpPos);
      }
    }
  }

  private bool CanCaptureAgain(IPiece piece, Position currentPosition)
  {
    LegalMovesResponseDto? legalMoves = GetLegalMoves(currentPosition);
    return legalMoves.Moves.Any();
  }
}
