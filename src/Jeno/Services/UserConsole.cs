using System;
using Jeno.Interfaces;

namespace Jeno.Services
{
    public class UserConsole : IUserConsole
    {
        public string ReadInput(string parameterName, bool hideInput = false)
        {
            string input = string.Empty;

            Console.Write($"{parameterName}:");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input = input.Remove(input.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                else
                {
                    input += key.KeyChar;
                    Console.Write(hideInput ? '*' : key.KeyChar);
                }
            }

            return input;
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }
    }
}