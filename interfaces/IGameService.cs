public interface IGameService
{
    public IPlayer CurrentPlayer { get; set; }
    public Dictionary<IPlayer, List<IPiece>> PlayersPieces { get; set; }

    public GameResponseDto InitializeBoard(CreateGameDto? dto);
    public UpdatePiecePositionResultDto TryMove(UpdatePiecePositionDto dto);
    public IPiece? GetPieceAt(Position position);
    public IEnumerable<IPiece> AllPieces();
    public LegalMovesResponseDto GetLegalMoves(Position piecePosition);
    public LegalMovesResponseDto GetLegalMovesFromPlayer(IPlayer player);
    public LegalMovesResponseDto GetLegalMovesFromPiece(IPiece piece);
    public Dictionary<Position, List<Position>> PlayerHasCaptureMoves(IPlayer player);
    public bool PlayerHasAnyMoves(IPlayer player);

}
