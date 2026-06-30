

namespace CheckersGame;

class Program
{
  static void Main()
  {
    ConsoleRenderer.RenderGameTitle();
    CreateGameDto gameInfo = ConsoleRenderer.StartMenu();

    IPlayer player1 = new Player(gameInfo.PlayerOneName, gameInfo.PlayerOnePreferenceColor, true);
    IPlayer player2 = new Player(gameInfo.PlayerTwoName, gameInfo.PlayerTwoPreferenceColor, false);

    IBoard board = new Board(gameInfo.Size, [player1, player2]);

    GameService gameService = new GameService(board, [player1, player2]);

    GameController controller = new GameController(gameService);

    ConsoleRenderer consoleRenderer = new ConsoleRenderer(board, controller);

    CreateGameDto createGameDto = new CreateGameDto
    {
      PlayerOneName = player1.Name,
      PlayerTwoName = player2.Name,
      PlayerOnePreferenceColor = player1.Color,
      PlayerTwoPreferenceColor = player2.Color,
      Size = board.Size
    };

    bool playAgain;

    do
    {
      GameResponseDto res = controller.Start(createGameDto);

      consoleRenderer.SetBoard(board);

      controller.MoveMade += consoleRenderer.MoveEvent;
      controller.GameEnded += consoleRenderer.GameEndedEvent;

      while( res.Winner == null )
      {
        IPlayer player = res.CurrentPlayer;
        IPiece? movedPiece = null;
        Position fromPosition = default;
        Position toPosition = default;
        LegalMovesResponseDto legalMoves;

        consoleRenderer.RenderBoard();
        consoleRenderer.RenderGameStatus(player, controller.PlayersPieceCount());

        if( !controller.PlayerHasAnyMoves(player) )
        {
          res.Winner = controller.GetPlayers().FirstOrDefault(p => p.Color != player.Color);
          controller.EndGame(res.Winner);
          continue;
        }

        Dictionary<Position, List<Position>> captureMoves = controller.PlayerHasCaptureMoves(player);

        if( captureMoves.Count > 0 )
        {
          fromPosition = consoleRenderer.ReadForcedCapturePiece(player, captureMoves);
          movedPiece = controller.GetPieceAt(fromPosition);
          legalMoves = new LegalMovesResponseDto { Moves = captureMoves[fromPosition] };
          toPosition = legalMoves.Moves.First();
        }
        else
        {
          List<Position> movablePositions = controller.GetMovablePiecesFromPlayer(player);
          fromPosition = consoleRenderer.ReadChoosenPiecePosition(movablePositions);
          movedPiece = controller.GetPieceAt(fromPosition);

          legalMoves = controller.GetLegalMoves(fromPosition);
          toPosition = consoleRenderer.ReadMoveFromConsole(legalMoves);
        }

        UpdatePiecePositionDto updatePiecePosition = new UpdatePiecePositionDto
        {
          FromPosition = fromPosition,
          ToPosition = toPosition,
        };

        GameResponseDto moveResult = controller.Move(updatePiecePosition);

        consoleRenderer.SetBoard(res.Board);

        res = moveResult;
      }

      controller.EndGame(res.Winner);

      playAgain = consoleRenderer.PromptPostGame();

      if( playAgain )
      {
        res = controller.Restart();

        controller.MoveMade += consoleRenderer.MoveEvent;
        controller.GameEnded += consoleRenderer.GameEndedEvent;

        consoleRenderer.SetBoard(res.Board);
        consoleRenderer.ResetEventMessage();
      }
    } while( playAgain );
  }
}

