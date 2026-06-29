public class UpdatePiecePositionResultDto {
    public bool MovementSucceed;
    public bool Crowned;
    public bool Captured;
    public bool HasMoreCaptures;
    public required IPlayer CurrentPlayer;
    public IPlayer? Winner;
    public required IBoard Board;
}