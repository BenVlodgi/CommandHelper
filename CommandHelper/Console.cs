using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace CommandHelper
{
    public static class Console
    {

        public delegate void ConsoleEventHandler(object sender, ConsoleEventArgs e);
        public class ConsoleEventArgs : EventArgs
        {
            public int CursorLeft { get; set; }
            public string Input { get; set; }
            public bool Update { get; set; } = false;
            public ConsoleEventArgs(int cursorLeft, string input) : base()
            {
                CursorLeft = cursorLeft;
                Input = input;
            }
        }

        private static Regex _writeRegex = new Regex("<[fb]=\\w+>");
        private static readonly Dictionary<string, ConsoleColor> _consoleColors =
            Enum.GetValues(typeof(ConsoleColor)).Cast<ConsoleColor>().ToDictionary(color => color.ToString().ToLower(), color => color);



        /// <summary>Writes the specified value to the console using the syntax <f=color> to color the output text, use <f=d> or <b=d> to set the foreground or background back to default. Adds a newline to the end of the given text.</summary>
        /// <param name="value">The text to write.</param>
        /// <param name="cursorPosition">The position where to start the text writing.</param>
        /// <param name="clearRestOfLine">if set to <c>true</c> Clear the rest of line.</param>
        public static void WriteLine(string value, int? cursorPosition = null, bool clearRestOfLine = false)
        {
            Write(value, cursorPosition, clearRestOfLine);
            System.Console.WriteLine();
        }

        /// <summary>Writes the specified value to the console using the syntax <f=color> to color the output text, use <f=d> or <b=d> to set the foreground or background back to default.</summary>
        /// <param name="value">The text to write.</param>
        /// <param name="cursorPosition">The position where to start the text writing.</param>
        /// <param name="clearRestOfLine">if set to <c>true</c> Clear the rest of line.</param>
        public static void Write(string value, int? cursorPosition = null, bool clearRestOfLine = false)
        {
            if (cursorPosition.HasValue)
            {
                System.Console.CursorLeft = cursorPosition.Value;
            }

            ConsoleColor defaultForegroundColor = System.Console.ForegroundColor;
            ConsoleColor defaultBackgroundColor = System.Console.BackgroundColor;

            var segments = _writeRegex.Split(value);
            var colors = _writeRegex.Matches(value);

            for (int i = 0; i < segments.Length; i++)
            {
                if (i > 0)
                {
                    // Now that we have the color tag, split it int two parts, 
                    // the target(foreground/background) and the color.
                    var splits = colors[i - 1].Value
                        .Trim(new char[] { '<', '>' })
                        .Split('=')
                        .Select(str => str.ToLower().Trim())
                        .ToArray();

                    // if the color is set to d (default), then depending on our target,
                    // set the color to be the default for that target.
                    ConsoleColor consoleColor = splits[1] == "d"
                        ? (splits[0] == "b"
                            ? defaultBackgroundColor
                            : defaultForegroundColor)
                        : _consoleColors[splits[1]];

                    // Set the now chosen color to the specified target.
                    if (splits[0] == "b")
                        System.Console.BackgroundColor = consoleColor;
                    else
                        System.Console.ForegroundColor = consoleColor;
                }

                // Only bother writing out, if we have something to write.
                if (segments[i].Length > 0)
                    System.Console.Write(segments[i]);
            }

            System.Console.ForegroundColor = defaultForegroundColor;
            System.Console.BackgroundColor = defaultBackgroundColor;

            if (clearRestOfLine)
                ClearRestOfLine();
        }

        public static string ReadLine(string input = "", ConsoleEventHandler onTab = null)
        {
            int startLeft = System.Console.CursorLeft;
            int startTop = System.Console.CursorTop;

            int relativeLeft = input.Length;

            while (true)
            {
                RedrawConsoleInput(input, startLeft, startTop);

                //System.Console.CursorLeft = startLeft + relativeLeft;

                int trl = startLeft + relativeLeft;
                System.Console.CursorLeft = trl % System.Console.WindowWidth;
                System.Console.CursorTop = trl / System.Console.WindowWidth + startTop;

                System.Console.CursorVisible = true;
                var key = System.Console.ReadKey(true);
                System.Console.CursorVisible = false;
                if (key.Key == ConsoleKey.Enter)
                    return input;
                else if (key.Key == ConsoleKey.Tab)
                {
                    if (onTab != null)
                    {
                        ConsoleEventArgs e = new ConsoleEventArgs(relativeLeft, input);
                        onTab(null, e);
                        if (e.Update)
                        {
                            relativeLeft = e.CursorLeft;
                            input = e.Input;
                        }
                    }
                }
                else if (key.Key == ConsoleKey.LeftArrow)
                {
                    if (relativeLeft > 0)
                        relativeLeft--;
                }
                else if (key.Key == ConsoleKey.RightArrow)
                {
                    if (relativeLeft < input.Length)
                        relativeLeft++;
                }
                else if (key.Key == ConsoleKey.Backspace)
                {
                    if (relativeLeft > 0)
                    {
                        relativeLeft--;
                        input = input.Remove(relativeLeft, 1);
                        RedrawConsoleInput(input, startLeft, startTop);
                    }
                }
                else if (key.Key == ConsoleKey.Delete)
                {
                    if (relativeLeft < input.Length)
                    {
                        input = input.Remove(relativeLeft, 1);
                        RedrawConsoleInput(input, startLeft, startTop);
                    }
                }
                else if (key.Key == ConsoleKey.Insert)
                {
                    // Can't enable insert mode.   
                }
                else if (key.Key == ConsoleKey.End)
                {
                    relativeLeft = input.Length;
                }
                else if (key.Key == ConsoleKey.Home)
                {
                    relativeLeft = 0;
                }
                else
                {
                    if (key.KeyChar != '\0')
                    {
                        input = input.Insert(relativeLeft, key.KeyChar.ToString());
                        RedrawConsoleInput(input, startLeft, startTop);
                        relativeLeft++;
                    }
                }

            }
        }

        private static void RedrawConsoleInput(string input, int left, int top)
        {
            int currentTop = System.Console.CursorTop;
            int currentLeft = System.Console.CursorLeft;
            System.Console.CursorTop = top;
            Write(input, left, true);
            System.Console.CursorTop = currentTop;
            System.Console.CursorLeft = currentLeft;
        }

        /// <summary>Clears the line the cursor is on.</summary>
        public static void ClearLine()
        {
            int winTop = System.Console.WindowTop;
            System.Console.CursorLeft = 0;
            System.Console.Write(new string(' ', System.Console.WindowWidth - 1));
            System.Console.CursorLeft = 0;
            System.Console.CursorTop--;
            System.Console.WindowTop = winTop;
        }

        /// <summary>Clears the rest of line after the cursor.</summary>
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
