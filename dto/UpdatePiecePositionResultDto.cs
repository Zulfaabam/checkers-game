public class UpdatePiecePositionResultDto {
    public bool MovementSucceed;
    public bool Crowned;
    public bool Captured;
    public bool HasMoreCaptures;
    public IPlayer CurrentPlayer;
    public IPlayer? Winner;
    public IBoard Board;
}