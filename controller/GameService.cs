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

    CurrentPlayer = _players.Find(p => p.IsPlayerOne) ?? throw new ArgumentNullException();

    PlayersPieces = new Dictionary<IPlayer, List<IPiece>>
    {
      { _players[0], board.Cell.Cast<ICell>().Where(c => c.Piece != null && c.Piece.Color == players[0].Color).Select(c => c.Piece!).ToList() },
      { _players[1], board.Cell.Cast<ICell>().Where(c => c.Piece != null && c.Piece.Color == players[1].Color).Select(c => c.Piece!).ToList() }
    };
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
    int fromX = dto.FromPosition.X;
    int fromY = dto.FromPosition.Y;

    int toX = dto.ToPosition.X;
    int toY = dto.ToPosition.Y;

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

    // check the piece type and color

    // check if the move is legal

    // perform the move
    PerformMove(piece, dto.ToPosition);

    // check if the move resulted in a capture

    // check if the move resulted in a promotion

    // switch turn
    SwitchTurn();

    // return the result
  }
  public IPiece? GetPieceAt(Position position)
  {
    return _board.Cell[position.X, position.Y].Piece;
  }
  public IEnumerable<IPiece> AllPieces()
  {
    return _board.Cell.Cast<ICell>().Where(c => c.Piece != null).Select(c => c.Piece!);
  }
  public IEnumerable<LegalMovesResponseDto> GetLegalMovesFromPlayer(IPlayer player)
  {
    IEnumerable<Position> legalMoves = new List<Position>();
  }
  public IEnumerable<LegalMovesResponseDto> GetLegalMovesFromPiece(IPiece piece)
  {
    IEnumerable<Position> legalMoves = new List<Position>();

  }
  public bool HasCaptureMoves(IPlayer player)
  {

  }
  public bool HasAnyMoves(IPlayer player)
  {
    
  }
  private UpdatePiecePositionResultDto PerformMove(IPiece piece, Position to)
  {
    _board.Cell
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