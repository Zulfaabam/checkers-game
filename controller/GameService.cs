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

    CurrentPlayer = players.Find(p => p.IsPlayerOne) ?? throw new ArgumentNullException();

    PlayersPieces = _players.ToDictionary(
        player => player,
        player => _board.Cell
            .OfType<ICell>()
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
      Board = _board
    };
  }
  public UpdatePiecePositionResultDto TryMove(UpdatePiecePositionDto dto)
  {
    // check from and to positions are inside the board
    if (!IsInside(dto.FromPosition) || !IsInside(dto.ToPosition))
    {
      return new UpdatePiecePositionResultDto
      {
        MovementSucceed = false,
      };
    }

    // check if there is a piece at the from position
    IPiece? piece = GetPieceAt(dto.FromPosition);

    if (piece == null)
    {
      return new UpdatePiecePositionResultDto
      {
        MovementSucceed = false,
      };
    }

    // check if the move is legal
    var legalMoves = GetLegalMoves(dto.FromPosition);

    if (!legalMoves.Moves.Contains(dto.ToPosition))
    {
      return new UpdatePiecePositionResultDto
      {
        MovementSucceed = false,
      };
    }

    // perform the move
    PerformMove(piece, dto.FromPosition, dto.ToPosition);

    // check if the move resulted in a capture

    // check if the move resulted in a promotion

    // switch turn
    SwitchTurn();

    // return the result
    return new UpdatePiecePositionResultDto
    {
      MovementSucceed = true
    };
  }
  public IPiece? GetPieceAt(Position position)
  {
    return _board.Cell[position.X, position.Y].Piece;
  }
  public IEnumerable<IPiece> AllPieces()
  {
    return _board.Cell.Cast<ICell>().Where(c => c.Piece != null).Select(c => c.Piece!);
  }
  public LegalMovesResponseDto GetLegalMovesFromPlayer(IPlayer player)
  {
    IEnumerable<Position> legalMoves = new List<Position>();

    return new LegalMovesResponseDto
    {
      Moves = legalMoves
    };
  }
  public LegalMovesResponseDto GetLegalMovesFromPiece(IPiece piece)
  {
    IEnumerable<Position> legalMoves = new List<Position>();

    return new LegalMovesResponseDto
    {
      Moves = legalMoves
    };
  }
  public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
  {
    var piece = GetPieceAt(piecePosition) ?? throw new ArgumentException("Invalid piece position");
    List<Position> legalMoves = new List<Position>();

    if (CurrentPlayer.IsPlayerOne)
    {
      if (piece.PieceType == PieceType.Man)
      {
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y + 1));
      }
      else if (piece.PieceType == PieceType.King)
      {
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y + 1));
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y + 1));
      }
    } else if (!CurrentPlayer.IsPlayerOne)
    {
      if (piece.PieceType == PieceType.Man)
      {
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y + 1));
      } else if (piece.PieceType == PieceType.King)
      {
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X - 1, piecePosition.Y + 1));
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y - 1));
        legalMoves.Add(new Position(piecePosition.X + 1, piecePosition.Y + 1));
      }
    }

    return new LegalMovesResponseDto
    {
        Moves = legalMoves
    };
  }
  public bool HasCaptureMoves(IPlayer player)
  {
    // TODO implement the correct logic
    return GetLegalMovesFromPlayer(player).Moves.Any();
  }
  public bool HasAnyMoves(IPlayer player)
  {
    return GetLegalMovesFromPlayer(player).Moves.Any();
  }

  public static bool TryParsePosition(string input, out Position position)
  {
      position = default;

      if (string.IsNullOrWhiteSpace(input))
          return false;

      var parts = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

      if (parts.Length != 2)
          return false;

      if (int.TryParse(parts[0], out int x) &&
          int.TryParse(parts[1], out int y))
      {
          position = new Position(x, y);
          return true;
      }

      return false;
  }

  private UpdatePiecePositionResultDto PerformMove(IPiece piece, Position from, Position to)
  {
    _board.Cell[to.X, to.Y].Piece = piece;

    _board.Cell[from.X, from.Y].Piece = null;

    return new UpdatePiecePositionResultDto
    {
      MovementSucceed = true
    };
  }
  private void SwitchTurn()
  {
    CurrentPlayer = _players.First(p => p != CurrentPlayer);
  }
  private bool IsInside(Position position)
  {
    return position.X >= 0 && position.X < (int)_board.Size && position.Y >= 0 && position.Y < (int)_board.Size;
  }
}