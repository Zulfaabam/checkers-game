public class ConsoleRenderer
{
    private IBoard _board;
    private string _eventMessage = "";
    private GameController _controller;

    public ConsoleRenderer(IBoard board, GameController controller)
    {
        _board = board;
        _controller = controller;
    }

    public void Render()
    {
        Console.Clear();

        for (int row = 0; row < (int)_board.Size; row++)
        {
            for (int column = 0; column < (int)_board.Size; column++)
            {
                var cell = _board.Cell[row, column];

                if (cell.Piece != null)
                {
                    Console.ForegroundColor = cell.Piece.Color;
                    Console.Write(cell.Piece.PieceType == PieceType.Man ? "[M]" : "[K]");
                    // Console.Write(cell.Piece.PieceType == PieceType.Man ? $"[{row},{column}]" : $"[{row},{column}]");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write($"[ ]");
                }
            }
            Console.WriteLine();
        }
    }

    public UpdatePiecePositionDto ReadMoveFromConsole()
    {
        Console.WriteLine();
        
        Console.Write("Choose piece position to be moved (ex. 0,1):");
        string startPosition = Console.ReadLine();

        Console.Write("Where to move (ex. 0,1):");
        string endPosition = Console.ReadLine();

        if (!GameService.TryParsePosition(startPosition, out Position from) ||
        !GameService.TryParsePosition(endPosition, out Position to))
        {
            throw new ArgumentException("Invalid position format. Use x,y like 2,3.");
        }

        return new UpdatePiecePositionDto
        {
            FromPosition = from,
            ToPosition = to
        };
    }

    public void MoveEvent(object? o, MoveEventArgs args)
    {
        _eventMessage = $"Moved piece from ({args.FromPosition.X}, {args.FromPosition.Y}) to ({args.ToPosition.X}, {args.ToPosition.Y})";
        Render();
        Console.WriteLine(_eventMessage);
    }
}