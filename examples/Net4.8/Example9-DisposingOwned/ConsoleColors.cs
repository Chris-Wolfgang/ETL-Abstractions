namespace Example9_DisposingOwned
{
    internal class ConsoleColors
    {
        private const char Escape = (char)27;

        public static readonly string Green = Escape + "[32m";
        public static readonly string Yellow = Escape + "[33m";
        public static readonly string Reset = Escape + "[0m";
    }
}
