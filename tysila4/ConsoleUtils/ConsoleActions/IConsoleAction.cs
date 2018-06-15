using System;

namespace ConsoleUtils.ConsoleActions
{
    public interface IConsoleAction
    {
        void Execute(IConsole console, ConsoleKeyInfo consoleKeyInfo);
    }
}