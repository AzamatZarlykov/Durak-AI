using System;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            int n = 1000;
            int rankStartingPoint = 6;
            string player1 = "randomAI";
            string player2 = "randomAI";

            Controller controller = new Controller(n, player1, player2, rankStartingPoint);
            controller.Run();



        }
    }
}