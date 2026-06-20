public interface IBoard
{
    public ICell[,] Cell { get; }
    public BoardSize Size { get; set; }
}