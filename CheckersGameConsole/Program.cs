

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

    consoleRenderer.RunGame(createGameDto);
  }
}

