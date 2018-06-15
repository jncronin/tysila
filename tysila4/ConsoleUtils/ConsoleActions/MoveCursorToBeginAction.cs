
using System;

namespace ConsoleUtils.ConsoleActions
{
    public class MoveCursorToBeginAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            console.CursorPosition = 0;
        }
    }
}
