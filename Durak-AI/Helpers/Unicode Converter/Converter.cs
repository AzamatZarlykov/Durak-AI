namespace Helpers.Unicode
{
    public static class UnicodeConverter
    {
        private static string[] suitUnicode = { "♠", "♥", "♦", "♣" };


        public static string GetSuit(int index)
        {
            return suitUnicode[index];
        }

        public static string GetRank(int value)
        {
            if (value < 11) { return value.ToString(); }
            
            return "JQKA"[value - 11].ToString();
        }
    }
}
