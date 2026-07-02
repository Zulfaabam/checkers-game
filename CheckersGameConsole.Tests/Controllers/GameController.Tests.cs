[TestFixture]
public class GameControllerTests
{
  private Mock<IGameService> _gameServiceMock = null!;
  private GameController _controller = null!;

  [SetUp]
  public void Setup()
  {
      _gameServiceMock = new Mock<IGameService>();
      _controller = new GameController(_gameServiceMock.Object);
  }

  [Test]
  public void Start_InitializesBoardAndClearsPreviousEvents_ReturnsGameResponseDto()
  {
    // Arrange
    CreateGameDto dto = new CreateGameDto
    {
        PlayerOneName = "Player 1",
        PlayerTwoName = "Player 2",
        PlayerOnePreferenceColor = ConsoleColor.Blue,
        PlayerTwoPreferenceColor = ConsoleColor.Yellow,
        Size = BoardSize.Standard
    };

    GameResponseDto expectedResponse = new GameResponseDto
    {
        CurrentPlayer = new Player("Player 1", ConsoleColor.Blue, true),
        Winner = null,
        Board = new Board(BoardSize.Standard, new ICell[0, 0])
    };

    _gameServiceMock
        .Setup(x => x.InitializeBoard(dto))
        .Returns(expectedResponse);

    _controller.MoveMade += (_, _) => { };
    _controller.GameEnded += (_, _) => { };

    // Act
    GameResponseDto result = _controller.Start(dto);

    // Assert
    Assert.That(result, Is.SameAs(expectedResponse));
    _gameServiceMock.Verify(x => x.InitializeBoard(dto), Times.Once);
  }

  [Test]
  public void GetLegalMoves_()
  {
    
  }

   [Test]
  public void Move_()
  {
    
  }

   [Test]
  public void PlayersPieceCount()
  {
    
  }

  [Test]
  public void PlayerHasAnyMoves_()
  {
    
  }

 [Test]
  public void GetPlayers()
  {
    
  }
  
  [Test]
  public void PlayerHasCaptureMoves()
  {
   
  }

  [Test]
  public void GetPieceAt()
  {
    
  }

  [Test]
  public void GetMovablePiecesFromPlayer()
  {
    
  }

  [Test]
  public void Restart()
  {
    // CreateGameDto createGameDto = new CreateGameDto
    // {
    //   PlayerOneName = _gameService.GetPlayers().ElementAt(0).Name,
    //   PlayerOnePreferenceColor = _gameService.GetPlayers().ElementAt(0).Color,
    //   PlayerTwoName = _gameService.GetPlayers().ElementAt(1).Name,
    //   PlayerTwoPreferenceColor = _gameService.GetPlayers().ElementAt(1).Color,
    //   Size = _gameService.GetBoard().Size
    // };

    // return Start(createGameDto);
  }

  [Test]
  public void EndGame()
  {
    // if( winner == null )
    // {
    //   return;
    // }

    // List<IPlayer> players = GetPlayers();
    // IPlayer? loser = players.FirstOrDefault(p => p != winner);

    // if( loser == null )
    // {
    //   return;
    // }

    // string reason;

    // Dictionary<IPlayer, int> pieceCounts = PlayersPieceCount();
    // bool loserHasNoPiece = pieceCounts[loser] == 0;

    // bool loserHasNoMove = !PlayerHasAnyMoves(loser);

    // if( loserHasNoPiece )
    // {
    //   reason = $"{winner.Name} wins because the opponent's pieces were wiped out";
    // }
    // else if( loserHasNoMove )
    // {
    //   reason = $"{winner.Name} wins because the opponent has no legal moves";
    // }
    // else
    // {
    //   reason = $"{winner.Name} wins";
    // }

    // GameEnded?.Invoke(this, new GameEndedEventArgs
    // {
    //   Winner = winner,
    //   Reason = reason
    // });
  }
}