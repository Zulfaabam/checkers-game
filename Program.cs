// See https://aka.ms/new-console-template for more information
namespace CheckersGame;

class Program {
    static void Main() {
        IPlayer player1 = new Player("Abam", ConsoleColor.White, true);
        IPlayer player2 = new Player("Maba", ConsoleColor.DarkBlue, false);

        IBoard board = new Board(BoardSize.Standard, [player1, player2]);

        var controller = new GameController(new GameService(board, [player1, player2]));

        // Console.WriteLine(board.Cell[0, 1].Piece?.Color);

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

