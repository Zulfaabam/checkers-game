public interface IBoard
{
    public ICell[,] Cell { get; set; }
    public BoardSize Size { get; set; }
}