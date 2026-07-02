[TestFixture]
public class GameServiceTests
{
    [Test]
    public void InitializeBoardCells_MissingRequiredPlayer_ThrowsInvalidOperationException()
    {
        // Arrange
        IPlayer playerOne = new Player("Player 1", ConsoleColor.Red, true);
        List<IPlayer> players = new List<IPlayer> { playerOne };

        // Act
        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);

        // Act + Assert
        // TODO: expected to return invalid board and console the msg
        Assert.That(() => cells, Throws.InvalidOperationException);
    }

    // [Test]
    // public void InitializeBoard_()
    // {

    // }

    [TestCase(BoardSize.Small)]
    [TestCase(BoardSize.Standard)]
    [TestCase(BoardSize.Large)]
    [TestCase(BoardSize.VeryLarge)]
    public void InitializeBoardCells_BoardSizeWithTwoPlayers_PopulatesStartingPieces(BoardSize boardSize)
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();
        IPlayer playerOne = players.Find(p => p.IsPlayerOne)!;
        IPlayer playerTwo = players.Find(p => !p.IsPlayerOne)!;

        int totalLength = (int)boardSize * (int)boardSize;
        int totalPieceCount = ( totalLength / 2 ) - (int)boardSize;
        int maxRow = (int)boardSize - 1;
        int emptyRow = ( (int)boardSize / 2 ) - 1;

        // Act
        ICell[,] cells = GameService.InitializeBoardCells(boardSize, players);

        // Assert
        Assert.That(cells.Length, Is.EqualTo(totalLength));

        int pieceCount = cells.Cast<ICell>().Count(cell => cell.Piece != null);
        Assert.That(pieceCount, Is.EqualTo(totalPieceCount)); // expected starting pieces

        Assert.That(cells[0, 1].Piece?.Color, Is.EqualTo(playerTwo.Color));
        Assert.That(cells[maxRow, 0].Piece?.Color, Is.EqualTo(playerOne.Color));
        Assert.That(cells[emptyRow, 3].Piece, Is.Null);
    }

    [Test]
    public void TryMove_MissingPieceInFromPosition_ReturnsMovementFailed()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(3, 3),
            ToPosition = new Position(4, 4),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = service.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.False);
    }

    [Test]
    public void TryMove_ToPositionOccupiedByOwnPiece_ReturnsMovementFailed()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(6, 1),
            ToPosition = new Position(5, 0),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = service.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.False);
        // TODO: add more assertions, IsCaptured etc
    }

    [Test]
    public void TryMove_ValidMovement_ReturnsMovementSucceed()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(5, 0),
            ToPosition = new Position(4, 1),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = service.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.True);
    }

    [Test]
    public void GetPieceAt_NoPieceAtPosition_ReturnsNull()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        // Act
        IPiece? piece = service.GetPieceAt(new Position(3, 3));

        // Assert
        Assert.That(piece, Is.Null);
    }

    [Test]
    public void GetPieceAt_FoundPieceAtPosition_ReturnsPiece()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        // Act
        IPiece? piece = service.GetPieceAt(new Position(5, 0));

        // Assert
        Assert.That(piece, Is.Not.Null);
        Assert.That(piece, Is.TypeOf<Piece>());
    }

    // [Test]
    // public void AllPieces_()
    // {

    // }

    [Test]
    public void GetLegalMoves_NoPieceFound_ReturnsNoLegalMoves()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players)
        {
            CurrentPlayer = players.Find(p => p.IsPlayerOne)!
        };

        // Act
        LegalMovesResponseDto result = service.GetLegalMoves(new Position(3, 3));

        // Assert
        Assert.That(result.Moves, Is.Empty);
    }

    [Test]
    public void GetLegalMoves_PlayerOneManWithOpenForwardMoves_ReturnsDiagonalPositions()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players)
        {
            CurrentPlayer = players.Find(p => p.IsPlayerOne)!
        };

        // Act
        LegalMovesResponseDto result = service.GetLegalMoves(new Position(5, 2));

        // Assert
        Assert.That(result.Moves, Is.EquivalentTo(new[]
        {
            new Position(4, 1),
            new Position(4, 3)
        }));
    }

    [Test]
    public void GetLegalMoves_PlayerTwoManWithOpenForwardMoves_ReturnsDiagonalPositions()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players)
        {
            CurrentPlayer = players.Find(p => !p.IsPlayerOne)!
        };

        // Act
        LegalMovesResponseDto result = service.GetLegalMoves(new Position(2, 1));

        // Assert
        Assert.That(result.Moves, Is.EquivalentTo(new[]
        {
            new Position(3, 0),
            new Position(3, 2)
        }));
    }

    [Test]
    public void PlayerHasCaptureMoves_NoEnemyPieceFound_ReturnsZeroCaptureMove()
    {

    }

    [Test]
    public void PlayerHasCaptureMoves_EnemyPieceFound_ReturnsValidCaptureMoves()
    {

    }

    [Test]
    public void PlayerHasAnyMoves_PlayerPieceIsEmpty_ReturnsFalse()
    {

    }

    [Test]
    public void PlayerHasAnyMoves_PlayerCantMoveAnyPiece_ReturnsFalse()
    {

    }

    [Test]
    public void PlayerHasAnyMoves_PlayerCanMoveOneOrMorePieces_ReturnsTrue()
    {

    }

    [Test]
    public void GetMovablePiecesFromPlayer_NoPiecesCanMove_ReturnsEmptyList()
    {

    }

    [Test]
    public void GetMovablePiecesFromPlayer_OneOrMorePiecesCanMove_ReturnsListOfPieces()
    {

    }

    [Test]
    public void GetPlayers_PlayerCannotBeFound_ReturnsEmptyList()
    {

    }

    [Test]
    public void GetPlayers_FoundPlayers_ReturnsListOfPlayers()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        // Act
        List<IPlayer> playersFromService = service.GetPlayers();

        // Assert
        Assert.That(playersFromService, Is.Not.Null);
        Assert.That(playersFromService, Is.Not.Empty);
        Assert.That(playersFromService, Is.TypeOf<List<IPlayer>>());
    }

    [Test]
    public void GetWinner_GameStillProgressing_ReturnsNull()
    {

    }

    [Test]
    public void GetWinner_GameEnded_ReturnsPlayerThatWins()
    {

    }

    [Test]
    public void PlayersPieceCount_PlayerNotFound_ThrowsInvalid()
    {

    }

    [Test]
    public void PlayersPieceCount_PlayerHavePieces_ReturnPairOfPlayerAndSumOfPieces()
    {

    }

    [Test]
    public void PlayersPieceCount_PlayerHaveNoPieces_ReturnPairOfPlayerAndZero()
    {

    }

    [Test]
    public void GetBoard_BoardCannotBeFound_ThrowSomething()
    {

    }

    [Test]
    public void GetBoard_FoundBoard_ReturnsBoard()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players);

        // Act
        IBoard boardFromService = service.GetBoard();

        // Assert
        Assert.That(boardFromService, Is.Not.Null);
        Assert.That(boardFromService, Is.TypeOf<Board>());
    }

    private List<IPlayer> SetupPlayers()
    {
        IPlayer playerOne = new Player("Alice", ConsoleColor.Red, true);
        IPlayer playerTwo = new Player("Bob", ConsoleColor.White, false);
        List<IPlayer> players = new List<IPlayer> { playerOne, playerTwo };

        return players;
    }
}
