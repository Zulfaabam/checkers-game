

namespace CheckersGame;

class Program {
  static void Main() {
    IPlayer player1 = new Player("player 1", ConsoleColor.Red, true);
    IPlayer player2 = new Player("player 2", ConsoleColor.DarkBlue, false);

    IBoard board = new Board(BoardSize.Standard, [player1, player2]);

    var controller = new GameController(new GameService(board, [player1, player2]));

    var consoleRenderer = new ConsoleRenderer(board, controller);

    consoleRenderer.RenderGameTitle();

    CreateGameDto playersInfo = consoleRenderer.AskPlayersInfo();

    player1.Name = playersInfo.PlayerOneName;
    player1.Color = playersInfo.PlayerOnePreferenceColor;
    player2.Name = playersInfo.PlayerTwoName;
    player2.Color = playersInfo.PlayerTwoPreferenceColor;

    controller.Start(new CreateGameDto
    {
      PlayerOneName = player1.Name,
      PlayerTwoName = player2.Name,
      PlayerOnePreferenceColor = player1.Color,
      PlayerTwoPreferenceColor = player2.Color,
      Size = BoardSize.Standard
    });
  }
}

