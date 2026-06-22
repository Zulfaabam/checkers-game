// See https://aka.ms/new-console-template for more information
namespace CheckersGame;

class Program {
    static void Main() {
        IPlayer player1 = new Player("Abam", ConsoleColor.White, true);
        IPlayer player2 = new Player("Maba", ConsoleColor.DarkBlue, false);

        IBoard board = new Board(BoardSize.Standard, [player1, player2]);

    }
}

