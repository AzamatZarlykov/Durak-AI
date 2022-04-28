namespace Helpers.Unicode
{
    public static class UnicodeConverter
    {
        private static string[] suitUnicode = { "\u2660", "\u2665", "\u2666", "\u2663" };


        public static string GetSuit(int index)
        {
            return suitUnicode[index];
        }

        public static string GetRank(int value)
        {
            if (value < 11) { return value.ToString(); }

            switch (value)
            {
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                default: return "A";
            }
        }
    }
}
