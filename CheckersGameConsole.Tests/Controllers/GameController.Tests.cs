[TestFixture]
public class GameControllerTests
{
  [Test]
  public void Start_InitializesBoardAndClearsPreviousEvents()
  {
    // Arrange
    List<IPlayer> players = new List<IPlayer>
    {
      new Player("Alice", ConsoleColor.Red, true),
      new Player("Bob", ConsoleColor.White, false)
    };

    ICell[,] cells = GameService.InitializeBoardCells(BoardSize.Standard, players);
    IBoard board = new Board(BoardSize.Standard, cells);
    IGameService service = new GameService(board, players);
    GameController controller = new GameController(service);

    controller.MoveMade += (_, _) => { };
    controller.GameEnded += (_, _) => { };

    CreateGameDto dto = new CreateGameDto
    {
      PlayerOneName = "Player 1",
      PlayerTwoName = "Player 2",
      PlayerOnePreferenceColor = ConsoleColor.Blue,
      PlayerTwoPreferenceColor = ConsoleColor.Yellow,
      Size = BoardSize.Standard
    };

    // Act
    GameResponseDto result = controller.Start(dto);

    // Assert
    Assert.That(result, Is.Not.Null);
    Assert.That(result.CurrentPlayer, Is.Not.Null);
    Assert.That(result.CurrentPlayer.Name, Is.EqualTo("Player 1"));
    Assert.That(result.Board, Is.Not.Null);
    Assert.That(result.Board.Size, Is.EqualTo(BoardSize.Standard));

    System.Reflection.FieldInfo? moveMadeField = typeof(GameController).GetField("MoveMade", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
    System.Reflection.FieldInfo? gameEndedField = typeof(GameController).GetField("GameEnded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);

    Assert.That(moveMadeField, Is.Not.Null);
    Assert.That(gameEndedField, Is.Not.Null);
    Assert.That(moveMadeField!.GetValue(controller), Is.Null);
    Assert.That(gameEndedField!.GetValue(controller), Is.Null);
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