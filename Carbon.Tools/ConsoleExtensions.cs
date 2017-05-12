using System;

namespace Carbon.Tools
{
    public static class ConsoleExtensions
    {
        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = System.Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter)
            {
                if (info.Key != ConsoleKey.Backspace)
                {
                    System.Console.Write("*");
                    password += info.KeyChar;
                }
                else if (info.Key == ConsoleKey.Backspace)
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = System.Console.CursorLeft;
                        // move the cursor to the left by one character
                        System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                        // replace it with space
                        System.Console.Write(" ");
                        // move the cursor to the left by one character again
                        System.Console.SetCursorPosition(pos - 1, System.Console.CursorTop);
                    }
                }
                info = System.Console.ReadKey(true);
            }
            // add a new line because user pressed enter at the end of their password
            System.Console.WriteLine();
            return password;
        }
    }
}
