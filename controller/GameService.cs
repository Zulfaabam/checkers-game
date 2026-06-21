public class GameService : IGameService {
  private IBoard _board;
  private List<IPlayer> _players;
  public IPlayer CurrentPlayer { get; set;}
  public Dictionary<IPlayer, List<IPiece>> PlayersPieces { get; set;}

  public GameService (IBoard board, List<IPlayer> players)
  {
    _board = board;
    _players = players;
  }
  public GameResponseDto InitializeBoard(CreateGameDto? dto)
  {
    if (dto == null)
    {
      throw new ArgumentNullException();
    }

    IBoard board = new Board(dto.Size, PlayersPieces);

    return new GameResponseDto
    {
      CurrentPlayer = CurrentPlayer,
      Winner = null,
      Board = board
    };

  }
  public UpdatePiecePositionResultDto TryMove(UpdatePiecePositionDto dto)
  {
    
  }
  public IPiece? GetPieceAt(Position position)
  {
    return _board.Cell[position.X, position.Y].Piece;
  }
  public IEnumerable<IPiece> AllPieces()
  {
    
  }
  public IEnumerable<LegalMovesResponseDto> GetLegalMovesFromPlayer(IPlayer player)
  {
    
  }
  public IEnumerable<LegalMovesResponseDto> GetLegalMovesFromPiece(IPiece piece)
  {
    
  }
  public bool HasCaptureMoves(IPlayer player)
  {
    
  }
  public bool HasAnyMoves(IPlayer player)
  {
    
  }
  private UpdatePiecePositionResultDto PerformMove(IPiece piece, Position to)
  {
    
  }
  private void SwitchTurn()
  {
    
  }
  private bool IsInside(Position position)
  {
    
  }
}