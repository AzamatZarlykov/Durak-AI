using System;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            CLAP.Parser.Run<Parser>(args);


/*            int n = 2;
            int rankStartingPoint = 6;
            string player1 = "randomAI";
            string player2 = "randomAI";
            bool verbose = true;

            new GameParameters
            {
                NumberOfGames = n,
                StartingRank = rankStartingPoint,
                Agents = new List<Agent>
            }

            Controller controller = new Controller(n, player1, player2, rankStartingPoint);
            controller.Run();

*/

        }
    }
}