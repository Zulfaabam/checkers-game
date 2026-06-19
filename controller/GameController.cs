public class GameController
{
    private IGameService _gameService;

    // public event EventHandler~MoveEventArgs~? MoveMade
    // public event EventHandler~GameEndedEventArgs~? GameEnded

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    public GameResponseDto Start(CreateGameDto dto)
    {
        return _gameService.InitializeBoard(dto);
    }

    public LegalMovesResponseDto GetLegalMoves(Position piecePosition)
    {
        var piece = _gameService.GetPieceAt(piecePosition);
        if (piece == null)
            throw new ArgumentException("Invalid piece position");

        return _gameService.GetLegalMoves(piece).FirstOrDefault() ?? new LegalMovesResponseDto();
    }

    // public GameResponseDto Move(UpdatePiecePositionDto dto)
    // {
    //     return _gameService.TryMove(dto);
    // }

    public GameResponseDto Restart()
    {
        // Implementation for restarting the game
        throw new NotImplementedException();
    }
}