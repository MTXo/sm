using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MauiApp1.Scripts
{
    internal class JustHelpers
    {
        public static string GetShortcut(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            // usuń śmieci typu znaki specjalne
            input = Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 🔹 wiele słów → pierwsze litery
            if (words.Length > 1)
            {
                var sb = new StringBuilder();

                foreach (var word in words)
                {
                    if (!string.IsNullOrEmpty(word))
                        sb.Append(char.ToUpper(word[0]));

                    if (sb.Length == 4)
                        break;
                }

                return sb.ToString();
            }
            else
            {
                // 🔹 jedno słowo → pierwsze 4 litery
                return input.Length <= 4
                    ? input.ToUpper()
                    : input.Substring(0, 4).ToUpper();
            }
        }
    }
}
