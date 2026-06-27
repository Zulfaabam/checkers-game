public class GameEndedEventArgs : EventArgs
{
    public IPlayer? Winner;
    public string? Reason;
}