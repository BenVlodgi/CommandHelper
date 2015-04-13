using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommandHelper
{
    public static class Console
    {
        private static Regex _writeRegex = new Regex("<[fb]=\\w+>");

        /// <summary>Writes the specified value to the console using the syntax <f=color> to color the output text, use <f=d> or <b=d> to set the foreground or background back to default. Adds a newline to the end of the given text.</summary>
        /// <param name="value">The text to write.</param>
        /// <param name="cursorPosition">The position where to start the text writing.</param>
        /// <param name="clearRestOfLine">if set to <c>true</c> Clear the rest of line.</param>
        public static void WriteLine(string value, int? cursorPosition = null, bool clearRestOfLine = false)
        {
            Write(value + Environment.NewLine, cursorPosition, clearRestOfLine);
        }
        /// <summary>Writes the specified value to the console using the syntax <f=color> to color the output text, use <f=d> or <b=d> to set the foreground or background back to default.</summary>
        /// <param name="value">The text to write.</param>
        /// <param name="cursorPosition">The position where to start the text writing.</param>
        /// <param name="clearRestOfLine">if set to <c>true</c> Clear the rest of line.</param>
        public static void Write(string value, int? cursorPosition = null, bool clearRestOfLine = false)
        {
            if (cursorPosition.HasValue)
                System.Console.CursorLeft = cursorPosition.Value;

            ConsoleColor defaultForegroundColor = System.Console.ForegroundColor;
            ConsoleColor defaultBackgroundColor = System.Console.BackgroundColor;

            var segments = _writeRegex.Split(value);
            var colors = _writeRegex.Matches(value);

            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0)
                {
                    ConsoleColor consoleColor;
                    var splits = colors[i - 1].Value.Trim(new char[] { '<', '>' }).Split('=').Select(str => str.ToLower().Trim()).ToArray();
                    if (splits[1] == "d")
                        if (splits[0][0] == 'b')
                            consoleColor = defaultBackgroundColor;
                        else
                            consoleColor = defaultForegroundColor;
                    else
                        consoleColor = Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().FirstOrDefault(en => en.ToString().ToLower() == splits[1]);
                    if (splits[0][0] == 'b')
                        System.Console.BackgroundColor = consoleColor;
                    else
                        System.Console.ForegroundColor = consoleColor;
                }
                if (segments[i].Length > 0)
                    System.Console.Write(segments[i]);
            }

            System.Console.ForegroundColor = defaultForegroundColor;
            System.Console.BackgroundColor = defaultBackgroundColor;

            if (clearRestOfLine)
                ClearRestOfLine();
        }

        /// <summary>Clears the line.</summary>
        public static void ClearLine()
        {
            int winTop = System.Console.WindowTop;
            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
            System.Console.CursorLeft = 0;
            System.Console.CursorTop--;
            System.Console.WindowTop = winTop;
        }
        /// <summary>Clears the rest of line.</summary>
        public static void ClearRestOfLine()
        {
            int winTop = System.Console.WindowTop;
            int left = System.Console.CursorLeft;
            System.Console.Write(new string(' ', System.Console.WindowWidth - left));
            System.Console.CursorLeft = left;
            System.Console.CursorTop--;
            System.Console.WindowTop = winTop;
        }
    }
}
