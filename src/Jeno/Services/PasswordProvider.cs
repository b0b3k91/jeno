using Jeno.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Services
{
    public class PasswordProvider : IPasswordProvider
    {
        public string GetPassword()
        {
            string password = string.Empty;

            Console.WriteLine("Password:");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password = password.Remove(password.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if(key.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else
                {
                        password += key.KeyChar;
                        Console.Write("*");
                }
            }

            return password;
        }
    }
}
