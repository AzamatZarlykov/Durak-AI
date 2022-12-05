using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;

namespace Helpers
{
    public static class Formatter
    {
        public static string toString<T>(List<T> list) =>
       "[" + string.Join(", ",
           list.Select(x => x == null ? "(null)" : x.ToString())) + "]";

        public static List<Card> SortCards(List<Card> cards) =>
            cards.OrderBy(c => (int)c.suit).ThenBy(c => (int)c.rank).ToList();
    }


}
