public class Player : IPlayer {
  public string Name { get; set;}

  public bool IsPlayerOne { get; set;}

  public Player (string name)
  {
    Name = name;
  }
}