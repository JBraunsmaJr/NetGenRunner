using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetGenRunner
{
    public static class Util
    {
        public static bool GetBool(string prompt)
        {
            PrintText(prompt, ConsoleColor.DarkYellow);

            string input = Console.ReadLine();

            if (input.ToLower().StartsWith("y"))
                return true;

            return false;
        }

        /// <summary>
        /// Outputs text with <paramref name="color"/>
        /// </summary>
        /// <param name="message">Text to display to user</param>
        /// <param name="color">Color to use (default is white)</param>
        /// <param name="newLine">Should cursor be moved to new line after printing? (default true)</param>
        public static void PrintText(string message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Force a user to be within <paramref name="min"/> and <paramref name="max"/>
        /// </summary>
        /// <typeparam name="T">Numeric Type</typeparam>
        /// <param name="prompt">Question to ask user prior to retrieving input</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>Returns user input as <typeparamref name="T"/> between <paramref name="min"/> and <paramref name="max"/></returns>
        public static T GetInput<T>(string prompt, T min, T max)
            where T : IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
        {
            T value = GetInput<T>(prompt);

            while(Comparer<T>.Default.Compare(value, min) < 0||
                Comparer<T>.Default.Compare(value, max) > 0)
            {
                value = GetInput<T>($"Expected a value between {min} - {max}");
            }

            return value;
        }

        /// <summary>
        /// Ask a user a question and retrieve their input
        /// </summary>
        /// <typeparam name="T">Type of data to retrieve</typeparam>
        /// <param name="prompt">Question to ask user</param>
        /// <returns>Returns the user as <typeparamref name="T"/></returns>
        public static T GetInput<T>(string prompt)
        {
            PrintText(prompt, ConsoleColor.DarkYellow);

            string input = string.Empty;
            bool valid = false;
            T value = default(T);

            do
            {
                input = Console.ReadLine();

                try
                {
                    value = (T)Convert.ChangeType(input, typeof(T));
                    valid = true;
                }
                catch
                {
                    PrintText($"Invalid input. Expected a value of type '{typeof(T).Name}'", ConsoleColor.Red);
                    valid = false;
                }
            } while (!valid);

            return value;
        }
    }
}
