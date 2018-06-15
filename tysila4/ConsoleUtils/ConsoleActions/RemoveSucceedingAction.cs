
using System;

namespace ConsoleUtils.ConsoleActions
{
    public class RemoveSucceedingAction : IConsoleAction
    {
        public void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo)
        {
            if (console.CursorPosition < console.CurrentLine.Length)
                console.CurrentLine = console.CurrentLine.Remove(console.CursorPosition);
        }
    }
}
