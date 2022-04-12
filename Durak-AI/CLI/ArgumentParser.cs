using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CLI.exceptions;

namespace CLI.Parser
{
    public class ArgumentParser
    {
        
        private int argumentSize;
        private string[] command;

        private static string[] aiNames = { "randomAI", "greedyAI", "minimaxAI" };

        private SimulationType simulationType;
        private AIType firstAI;
        private string parameter1;

        private AIType secondAI;
        private string parameter2;
        public ArgumentParser(string[] args)
        {
            this.argumentSize = args.Length;
            this.command = args;
        }

        public SimulationType getSimulationType()
        {
            return simulationType;
        }

        public AIType getFirstAIType()
        {
            return firstAI;
        }

        public string getParameterOne()
        {
            return parameter1;
        }

        public AIType getSecondAIType()
        {
            return secondAI;
        }

        public string getParameterTwo()
        {
            return parameter2;
        }

        private AIType ExtractAIName(int order)
        {
            string ai = command[order];
            
            if (Array.IndexOf(aiNames, ai) < 0)
            {
                throw new UnknownAIException(
                    string.Format("{0} is not in the list of AI algorithms", ai));
            }

            if (ai == "randomAI") return AIType.Random;
            if (ai == "greedyAI") return AIType.Greedy;
            return AIType.Minimax;
        }

        public void Parse()
        {
            // ai vs ai or ai vs human
            if (argumentSize != 5 && argumentSize != 2)
            {
                throw new ArgumentException("Command is unknown");
            }

            try
            {
                // ai vs human
                if (argumentSize == 2)
                {
                    simulationType = SimulationType.AiVSHuman;
                    firstAI = ExtractAIName(0);
                    parameter1 = command[1];
                } else
                {
                    simulationType = SimulationType.AiVSAi;
                    firstAI = ExtractAIName(0);
                    parameter1 = command[1];
                    secondAI = ExtractAIName(2);
                    parameter2 = command[2];
                }
            } catch (UnknownAIException e)
            {
                Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
            }

        }

    }
}

// dotnet run "ai_1_name" [options for ai_1_name] "ai_2_name" [options for ai_2_name] [number of games] 
// dotnet run "ai_name" [options for ai_1_name] 