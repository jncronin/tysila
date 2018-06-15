using System;
using ConsoleUtils.ConsoleActions;

namespace ConsoleUtils
{
    internal class ConsoleExtInstance : IConsole
    {
        internal int start_cursor = 0;

        public PreviousLineBuffer PreviousLineBuffer { get { return ConsoleExt.PreviousLineBuffer; } }
        public string CurrentLine { get { return ConsoleExt.CurrentLine; } set { ConsoleExt.CurrentLine = value; } }
        public int CursorPosition { get { return Console.CursorLeft; } set { Console.CursorLeft = value; } }
        public int StartCursorPosition { get { return start_cursor; } set { start_cursor = value; } }
    }
}
