public class CreateGameDto {
    public required string PlayerOneName;
    public required string PlayerTwoName;
    public ConsoleColor PlayerOnePreferenceColor;
    public ConsoleColor PlayerTwoPreferenceColor;
    public BoardSize Size;
}