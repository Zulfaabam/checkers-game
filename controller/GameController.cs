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
        var res = _gameService.InitializeBoard(dto);

        var consoleRenderer = new ConsoleRenderer(res.Board, this);
        
        while (res.Winner == null)
        {
            consoleRenderer.Render();

            var move = consoleRenderer.ReadMoveFromConsole();
            
            var moveResult = _gameService.TryMove(move);

            if (!moveResult.MovementSucceed)
            {
                Console.WriteLine("Invalid move");
                continue;
            }

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
        var piece = _gameService.GetPieceAt(piecePosition);
        
        if (piece == null)
            throw new ArgumentException("Invalid piece position");

        return _gameService.GetLegalMovesFromPiece(piece) ?? new LegalMovesResponseDto();
    }

    public GameResponseDto Move(UpdatePiecePositionDto dto)
    {
        var res = _gameService.TryMove(dto);
            
        if (!res.MovementSucceed)
            throw new InvalidOperationException("Invalid move");

        return new GameResponseDto
        {
            CurrentPlayer = _gameService.CurrentPlayer,
            Winner = res.Winner,
            Board = res.Board
        };
    }

    public GameResponseDto Restart()
    {
        // Implementation for restarting the game
        throw new NotImplementedException();
    }
}