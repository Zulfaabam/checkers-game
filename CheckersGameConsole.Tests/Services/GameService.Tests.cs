[TestFixture]
public class GameServiceTests
{
    private IBoard _board;
    private List<IPlayer> _players;
    private IGameService _gameService;

    [SetUp]
    public void SetUp()
    {
        _players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, _players);
        _board = new Board(BoardSize.Standard, cells);
        _gameService = new GameService(_board, _players);
    }

    [Test]
    public void InitializeBoardCells_MissingRequiredPlayer_ThrowsInvalidOperationException()
    {
        // Arrange
        IPlayer playerOne = new Player("Player 1", ConsoleColor.Red, true);
        List<IPlayer> players = new List<IPlayer> { playerOne };

        // Act + Assert
        Assert.That(() => GameService.InitializeBoardCells(BoardSize.Standard, players),
            Throws.InvalidOperationException);
    }

    [Test]
    public void TryParsePosition_InputIsNullOrEmpty_ReturnsFalse() 
    {
        // Arrange
        string input = "";
        Position position = new Position();

        // Act
        bool result = GameService.TryParsePosition(input, out position);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(position, Is.EqualTo(new Position()));
    }

    [Test]
    public void TryParsePosition_NoCommaInInput_ReturnsFalse() 
    {
        // Arrange
        string input = "3";
        Position position = new Position();

        // Act
        bool result = GameService.TryParsePosition(input, out position);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(position, Is.EqualTo(new Position()));
    }

    [Test]
    public void TryParsePosition_InputIsAValidPosition_ReturnsTrueAndPosition() 
    {
        // Arrange
        string input = "3,2";
        Position position = new Position();

        // Act
        bool result = GameService.TryParsePosition(input, out position);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(position, Is.EqualTo(new Position(3, 2)));
    }

    [Test]
    public void InitializeBoard_CreateGameDtoIsComplete_ReturnsGameResponseDto()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;
        IPlayer playerTwo = _players.Find(p => !p.IsPlayerOne)!;

        CreateGameDto createGameDto = new CreateGameDto
        {
            PlayerOneName = playerOne.Name,
            PlayerTwoName = playerTwo.Name,
            PlayerOnePreferenceColor = playerOne.Color,
            PlayerTwoPreferenceColor = playerTwo.Color,
            Size = _board.Size
        };

        // Act
        GameResponseDto gameResponseDto = _gameService.InitializeBoard(createGameDto);

        // Assert
        Assert.That(gameResponseDto, Is.Not.Null);
        Assert.That(gameResponseDto.CurrentPlayer, Is.EqualTo(playerOne));
        Assert.That(gameResponseDto.Winner, Is.Null);
        Assert.That(gameResponseDto.Board, Is.EqualTo(_board));
    }

    [TestCase(BoardSize.Small)]
    [TestCase(BoardSize.Standard)]
    [TestCase(BoardSize.Large)]
    [TestCase(BoardSize.VeryLarge)]
    public void InitializeBoardCells_BoardSizeWithTwoPlayers_PopulatesStartingPieces(BoardSize boardSize)
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;
        IPlayer playerTwo = _players.Find(p => !p.IsPlayerOne)!;

        int totalLength = (int)boardSize * (int)boardSize;
        int totalPieceCount = ( totalLength / 2 ) - (int)boardSize;
        int maxRow = (int)boardSize - 1;
        int emptyRow = ( (int)boardSize / 2 ) - 1;

        // Act
        ICell[,] cells = GameService.InitializeBoardCells(boardSize, _players);

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
        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(3, 3),
            ToPosition = new Position(4, 4),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = _gameService.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.False);
    }

    [Test]
    public void TryMove_ToPositionOccupiedByOwnPiece_ReturnsMovementFailed()
    {
        // Arrange
        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(6, 1),
            ToPosition = new Position(5, 0),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = _gameService.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.False);
        // TODO: add more assertions, IsCaptured etc
    }

    [Test]
    public void TryMove_ValidMovement_ReturnsMovementSucceed()
    {
        // Arrange
        UpdatePiecePositionDto position = new()
        {
            FromPosition = new Position(5, 0),
            ToPosition = new Position(4, 1),
        };

        // Act
        UpdatePiecePositionResultDto tryMove = _gameService.TryMove(position);

        // Assert
        Assert.That(tryMove.MovementSucceed, Is.True);
    }

    [Test]
    public void GetPieceAt_NoPieceAtPosition_ReturnsNull()
    {
        // Arrange
        Position position = new Position(3, 3);

        // Act
        IPiece? piece = _gameService.GetPieceAt(position);

        // Assert
        Assert.That(piece, Is.Null);
    }

    [Test]
    public void GetPieceAt_FoundPieceAtPosition_ReturnsPiece()
    {
        // Arrange
        Position position = new Position(5, 0);

        // Act
        IPiece? piece = _gameService.GetPieceAt(position);

        // Assert
        Assert.That(piece, Is.Not.Null);
        Assert.That(piece, Is.TypeOf<Piece>());
    }

    [Test]
    public void AllPieces_GetAllThePiecesOnTheBoard_ReturnsListOfPieces()
    {
        // Arrange

        // Act
        IEnumerable<IPiece> pieces = _gameService.AllPieces();

        // Assert
        Assert.That(pieces, Is.Not.Null);
        Assert.That(pieces, Is.Not.Empty);
        Assert.That(pieces, Is.TypeOf<IEnumerable<IPiece>>());
    }

    [Test]
    public void GetLegalMoves_NoPieceFound_ReturnsNoLegalMoves()
    {
        // Arrange
        Position position = new Position(3, 3);

        _gameService.CurrentPlayer = _players.Find(p => p.IsPlayerOne)!;

        // Act
        LegalMovesResponseDto result = _gameService.GetLegalMoves(position);

        // Assert
        Assert.That(result.Moves, Is.Empty);
    }

    [Test]
    public void GetLegalMoves_PlayerOneManWithOpenForwardMoves_ReturnsDiagonalPositions()
    {
        // Arrange
        Position position = new Position(5, 2);

        _gameService.CurrentPlayer = _players.Find(p => p.IsPlayerOne)!;

        // Act
        LegalMovesResponseDto result = _gameService.GetLegalMoves(position);

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
        Position position = new Position(2, 1);

        _gameService.CurrentPlayer = _players.Find(p => !p.IsPlayerOne)!;

        // Act
        LegalMovesResponseDto result = _gameService.GetLegalMoves(position);

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
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        _gameService.CurrentPlayer = playerOne;

        // Act
        Dictionary<Position, List<Position>> captureMoves = _gameService.PlayerHasCaptureMoves(playerOne);

        // Assert
        Assert.That(captureMoves, Is.Empty);
    }

    [Test]
    public void PlayerHasCaptureMoves_EnemyPieceFound_ReturnsValidCaptureMoves()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        _gameService.CurrentPlayer = playerOne;

        // Act
        Dictionary<Position, List<Position>> captureMoves = _gameService.PlayerHasCaptureMoves(playerOne);

        // Assert
        Assert.That(captureMoves.ContainsKey(new Position(3, 3)), Is.True);
        Assert.That(captureMoves[new Position(3, 3)], Is.EqualTo(new List<Position>
        {
            new Position(1, 1)
        }));
    }

    [Test]
    public void PlayerHasAnyMoves_PlayerPieceIsEmpty_ReturnsFalse()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        _gameService.CurrentPlayer = playerOne;

        // Act
        bool hasAnyMoves = _gameService.PlayerHasAnyMoves(playerOne);

        // Assert
        Assert.That(hasAnyMoves, Is.False);
    }

    [Test]
    public void PlayerHasAnyMoves_PlayerCantMoveAnyPiece_ReturnsFalse()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;
        
        ICell[,] cells = new Cell[(int)BoardSize.Standard, (int)BoardSize.Standard];

        for (int row = 0; row < cells.GetLength(0); row++)
        {
            for (int column = 0; column < cells.GetLength(1); column++)
            {
                cells[row, column] = new Cell(new Position(row, column));
            }
        }

        cells[0, 1] = new Cell(new Position(0, 1), new Piece(PieceType.Man, playerOne.Color));

        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, _players)
        {
            CurrentPlayer = playerOne
        };

        // Act
        bool hasAnyMoves = service.PlayerHasAnyMoves(playerOne);

        // Assert
        Assert.That(hasAnyMoves, Is.False);
    }

    [Test]
    public void PlayerHasAnyMoves_PlayerCanMoveOneOrMorePieces_ReturnsTrue()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        ICell[,] cells = new Cell[(int)BoardSize.Standard, (int)BoardSize.Standard];

        for (int row = 0; row < cells.GetLength(0); row++)
        {
            for (int column = 0; column < cells.GetLength(1); column++)
            {
                cells[row, column] = new Cell(new Position(row, column));
            }
        }

        cells[3, 3] = new Cell(new Position(3, 3), new Piece(PieceType.Man, playerOne.Color));

        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, _players)
        {
            CurrentPlayer = playerOne
        };

        // Act
        bool hasAnyMoves = service.PlayerHasAnyMoves(playerOne);

        // Assert
        Assert.That(hasAnyMoves, Is.True);
    }

    [Test]
    public void GetMovablePiecesFromPlayer_NoPiecesCanMove_ReturnsEmptyList()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        ICell[,] cells = new Cell[(int)BoardSize.Standard, (int)BoardSize.Standard];

        for (int row = 0; row < cells.GetLength(0); row++)
        {
            for (int column = 0; column < cells.GetLength(1); column++)
            {
                cells[row, column] = new Cell(new Position(row, column));
            }
        }

        cells[0, 1] = new Cell(new Position(0, 1), new Piece(PieceType.Man, playerOne.Color));

        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, _players)
        {
            CurrentPlayer = playerOne
        };

        // Act
        List<Position> movablePieces = service.GetMovablePiecesFromPlayer(playerOne);

        // Assert
        Assert.That(movablePieces, Is.Empty);
    }

    [Test]
    public void GetMovablePiecesFromPlayer_OneOrMorePiecesCanMove_ReturnsListOfPieces()
    {
        // Arrange
        IPlayer playerOne = _players.Find(p => p.IsPlayerOne)!;

        ICell[,] cells = new Cell[(int)BoardSize.Standard, (int)BoardSize.Standard];

        for (int row = 0; row < cells.GetLength(0); row++)
        {
            for (int column = 0; column < cells.GetLength(1); column++)
            {
                cells[row, column] = new Cell(new Position(row, column));
            }
        }

        cells[3, 3] = new Cell(new Position(3, 3), new Piece(PieceType.Man, playerOne.Color));

        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, _players)
        {
            CurrentPlayer = playerOne
        };

        // Act
        List<Position> movablePieces = service.GetMovablePiecesFromPlayer(playerOne);

        // Assert
        Assert.That(movablePieces, Is.EqualTo(new List<Position>
        {
            new Position(3, 3)
        }));
    }

    [Test]
    public void GetPlayers_FoundPlayers_ReturnsListOfPlayers()
    {
        // Arrange

        // Act
        List<IPlayer> playersFromService = _gameService.GetPlayers();

        // Assert
        Assert.That(playersFromService, Is.Not.Null);
        Assert.That(playersFromService, Is.Not.Empty);
        Assert.That(playersFromService, Is.TypeOf<List<IPlayer>>());
    }

    [Test]
    public void GetWinner_AllPlayersStillHavePieces_ReturnsNull()
    {
        // Arrange

        // Act
        IPlayer? winner = _gameService.GetWinner();

        // Assert
        Assert.That(winner, Is.Null);
    }

    [Test]
    public void GetWinner_OnePlayerHasNoPieces_ReturnsPlayerThatWins()
    {
        // Arrange
        _gameService.PlayersPieces = new Dictionary<IPlayer, List<IPiece>>
        {
            { _players[0], new List<IPiece>() },
            { _players[1], new List<IPiece> { new Piece(PieceType.Man, _players[1].Color) } }
        };

        // Act
        IPlayer? winner = _gameService.GetWinner();

        // Assert
        Assert.That(winner, Is.EqualTo(_players[1]));
    }

    [Test]
    public void PlayersPieceCount_PlayerHavePieces_ReturnPairOfPlayerAndSumOfPieces()
    {
        // Arrange
        _gameService.PlayersPieces = new Dictionary<IPlayer, List<IPiece>>
        {
            { _players[0], new List<IPiece>()
                {
                    new Piece(PieceType.Man, ConsoleColor.Red),
                    new Piece(PieceType.Man, ConsoleColor.Red),
                }
            },
            { _players[1], new List<IPiece>()
                {
                    new Piece(PieceType.Man, ConsoleColor.DarkBlue),
                    new Piece(PieceType.Man, ConsoleColor.DarkBlue),
                }
            }
        };

        // Act
        Dictionary<IPlayer, int> playersPieceCount = _gameService.PlayersPieceCount();

        // Assert
        Assert.That(playersPieceCount, Is.Not.Null);
        Assert.That(playersPieceCount, Is.Not.Empty);
        Assert.That(playersPieceCount, Is.TypeOf<Dictionary<IPlayer, int>>());
        Assert.That(playersPieceCount.Count, Is.EqualTo(2));
        Assert.That(playersPieceCount[_players[0]], Is.EqualTo(2));
        Assert.That(playersPieceCount[_players[1]], Is.EqualTo(2));
    }

    [Test]
    public void PlayersPieceCount_PlayerHaveNoPieces_ReturnPairOfPlayerAndZero()
    {
        // Arrange
        List<IPlayer> players = SetupPlayers();

        ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
        IBoard board = new Board(BoardSize.Standard, cells);
        IGameService service = new GameService(board, players)
        {
            PlayersPieces = new Dictionary<IPlayer, List<IPiece>>
            {
                { players[0], new List<IPiece>()
                    {
                        new Piece(PieceType.Man, ConsoleColor.Red),
                        new Piece(PieceType.Man, ConsoleColor.Red),
                    }
                },
                { players[1], new List<IPiece>() {}}
            }
        };

        // Act
        Dictionary<IPlayer, int> playersPieceCount = service.PlayersPieceCount();

        // Assert
        Assert.That(playersPieceCount, Is.Not.Null);
        Assert.That(playersPieceCount, Is.Not.Empty);
        Assert.That(playersPieceCount, Is.TypeOf<Dictionary<IPlayer, int>>());
        Assert.That(playersPieceCount.Count, Is.EqualTo(2));
        Assert.That(playersPieceCount[players[1]], Is.EqualTo(0));
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
