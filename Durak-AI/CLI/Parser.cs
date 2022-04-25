using System;
using System.Collections.Generic;
using CLAP;
using CLAP.Validation;

using AIAgent;
using Helpers.Writer;

namespace CLI
{
    public class Parser
    {
        private static Agent GetAgentType(string type)
        {
            if (type == "random")
            {
                return new RandomAI();
            }
            return new RandomAI();
        }

        [Verb(IsDefault = true)]
        static void Parse(
            [DefaultValue("random"), Aliases("ai1")]
            string ai_name_one,

            [DefaultValue(4), LessOrEqualTo(5)]
            int depth_one,

            [DefaultValue("random"), Aliases("ai2")]
            string ai_name_two,

            [DefaultValue(4), LessOrEqualTo(5)]
            int depth_two,

            [Required, DefaultValue(1000)]
            int total_games,

            [DefaultValue(6)]
            int start_rank,

            [DefaultValue(true)]
            bool verbose
        )
        {
            var agents = new List<Agent>
            {
                GetAgentType(ai_name_one),
                GetAgentType(ai_name_two)
            };

            var gameParam = new GameParameters
            {
                NumberOfGames = total_games,
                StartingRank = start_rank,
                Agents = agents,
            };

            var writer = new Writer(Console.Out, verbose);

            Controller controller = new Controller(gameParam, writer);
            controller.Run();
        }
    }
}
