namespace ConsoleUtils.ConsoleActions
{
    public interface IConsole
    {
        PreviousLineBuffer PreviousLineBuffer { get; }
        string CurrentLine { get; set; }
        int CursorPosition { get; set; }
        int StartCursorPosition { get; set; }
    }
}