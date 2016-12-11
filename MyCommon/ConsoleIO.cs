using System;
using System.Collections.Generic;

namespace MyCommon
{
    /// <summary>
    /// Manager Console with Async I/O
    /// </summary>
    public class ConsoleIO
    {
        private static string inputBuffer = "";
        private static List<ColorStrings> outputBuffer = new List<ColorStrings>();
        public static string Title { get { return Console.Title; } set { Console.Title = value; } }
        private static object locker = new object();

        /// <summary>
        /// Write a Colored text
        /// </summary>
        /// <remarks>as System.Console.WriteLine()</remarks>
        public static void Write(ColorStrings Text)
        {
            outputBuffer.Add(Text);
            while (outputBuffer.Count > Console.WindowHeight - 2) { outputBuffer.RemoveAt(0); }
            Update();
        }

        /// <summary>
        /// Read the next characters line
        /// </summary>
        /// <remarks>as System.Console.ReadLine()</remarks>
        public static string Read()
        {
            ConsoleKeyInfo key = Console.ReadKey();
            while (key.Key != ConsoleKey.Enter)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Backspace:
                        if (inputBuffer.Length == 0) { SetInputPos(); }
                        if (inputBuffer.Length == 1) { inputBuffer = ""; }
                        if (inputBuffer.Length > 1) { inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1); }
                        break;

                    default:
                        inputBuffer += key.KeyChar;
                        break;
                }
                Update();
                key = Console.ReadKey();
            }
            Console.WriteLine();
            string res = inputBuffer;
            inputBuffer = "";
            return res;
        }

        private static void Update()
        {
            lock (locker)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                foreach (ColorStrings output in outputBuffer) { output.Write(); }
                SetInputPos();
                Console.ForegroundColor = ConsoleColor.White;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("> " + inputBuffer);
            }
        }

        private static void SetInputPos()
        {
            Console.SetCursorPosition(0, Math.Max(Console.WindowHeight - 1, Console.CursorTop + 1));
        }

        /// <summary>
        /// Clear buffer during ConsoleIO.Read()
        /// </summary>
        public static void ClearInput()
        {
            inputBuffer = ""; Update();
        }

        /// <summary>
        /// Clear Console lines
        /// </summary>
        /// <remarks>as System.Console.Clear()</remarks>
        public static void ClearOutput()
        {
            outputBuffer.Clear(); Update();
        }
    }

    /// <summary>
    /// Multiple Colors Text
    /// </summary>
    public class ColorStrings
    {
        public ColorString[] Text;

        public ColorStrings(params ColorString[] strings)
        {
            Text = strings;
        }

        public ColorStrings(string text)
        {
            Text = new ColorString[1] { new ColorString(text) };
        }

        public void Write()
        {
            foreach (ColorString cstring in Text)
            {
                Console.BackgroundColor = cstring.Back;
                Console.ForegroundColor = cstring.Fore;
                Console.Write(cstring.Text);
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Single Color Text
    /// </summary>
    public class ColorString
    {
        public string Text;
        public ConsoleColor Fore;
        public ConsoleColor Back;

        public ColorString(string text, ConsoleColor fore = ConsoleColor.White, ConsoleColor back = ConsoleColor.Black)
        {
            Text = text;
            Fore = fore;
            Back = back;
        }
    }
}