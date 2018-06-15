
using System;

namespace ConsoleUtils.ConsoleActions
{
    public class ClearLineAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            console.CurrentLine = string.Empty;
            console.CursorPosition = 0;
        }
    }
}
