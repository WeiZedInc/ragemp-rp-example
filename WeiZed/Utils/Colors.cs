using System;

namespace WeiZed.Core
{
    class Colors
    {
        public const string WHITE = "!{#FFFFFF}";
        public const string BLACK = "!{#000000}";
        public const string PURPLE = "!{#800080}";
        public const string ORCHID = "!{#DA70D6}";
        public const string BLUEVIOLET = "!{#8A2BE2}";
        public const string MAGENTA = "!{#FF00FF}";
        public const string ORANGE = "!{#FFA500}";
        public const string YELLOW = "!{#FFFF00}";
        public const string RED = "!{#FF0000}";
        public const string CRIMSON_RED = "!{#DC143C}";
        public const string GREY = "!{#808080}";
        public const string SILVER = "!{#C0C0C0}";
        public const string LIME = "!{#00FF00}";
        public const string LIMEGREEN = "!{#32CD32}";
        public const string SpringGREEN = "!{#00FF7F}";
        public const string GOLD = "!{#FFD700}";
        public const string AQUA = "!{#00FFFF}";
        public const string SKYBLUE = "!{#00BFFF}";
        public const string BLUE = "!{#0000FF}";

        public static void ColoredConsoleOutput(string msg, ConsoleColor consoleColor)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}
