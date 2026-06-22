public class Player : IPlayer {
  public string Name { get; set;}
  public ConsoleColor Color { get; set;}
  public bool IsPlayerOne { get; set;}

  public Player (string name, ConsoleColor color, bool isPlayerOne)
  {
    Name = name;
    Color = color;
    IsPlayerOne = isPlayerOne;
  }
}