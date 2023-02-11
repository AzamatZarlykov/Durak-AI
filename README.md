# Durak-AI

This project is a console-based application focused on the accurate implementation of the strategic card game "Durak", specifically the variation "Podkidnoy Durak". Our aim is to achieve both accuracy and seamless integration of AI agents within the game model.

## Game Desciption

Durak is a strategic card game that originated in Russia. It is
played with a deck of cards and typically involves two to six players. Unlike most
other games, the aim of Durak is not to find a winner, but to find a loser. Players
take turns attacking and defending in a series of rounds. During an attack, the
attacking player leads with one or more cards, and the defending player must
attempt to beat them by playing a higher-ranked card. If the defending player
is unable or unwilling to do so, they must pick up all the cards. The goal of the
game is to get rid of all of one’s cards, and the player left holding cards at the
end is declared the fool." and the following goals that it achieved 

## Motivation and Challenges

Durak is a strategic card game that involves hidden information and randomness, making it an imperfect information type of game. This creates a challenging environment for developing AI agents that can make optimal moves. The goal of this project is to implement the game correctly with all relevant details, including the integration of various AI agents.

## Requirements and Dependencies

* This project requires .NET 6 framework to be installed in your system.
* The code for this project is written in C# programming language.
* Any editor that supports .NET and C# development can be used to run the code.

Please make sure you have the above requirements and dependencies installed before running the code for this project.

## Installation Instructions and Usage

1. Clone the repository to your local machine:  
``` bash
git clone https://github.com/AzamatZarlykov/Durak.git
```
2. Install the .NET 6 runtime and SDK. You can download the latest version from 
https://dotnet.microsoft.com/download/dotnet/6.0.
3. Open the solution file in Visual Studio or any other IDE of your choice.
4. Build the solution by navigating to `'Build > Build Solution'` in the menu or by pressing `'Ctrl + Shift + B'` on Windows or `'Cmd + Shift + B'` on macOS.
5. Run the application using the `'Start Debugging'` option in Visual Studio or navigate to the `'CLI'` folder and execute the `'dotnet run command'` in the command line. This will display the `'--help'` output with information on the usage of the command line parameters. Use the help output as a guide to experiment with various AI agents and their parameters, as well as changing the game environment.
```
   parse|p (Default)
        /ai1            : The agent for player 1. (String) (Default = random)
        /ai2            : The agent for player 2. (String) (Default = random)
        /bf             : Used for estimating the branching factor (with greedy agents) (Default = False)
        /config         : Used for grid search parameter configuration. For the purpose of experiments (Default = False)
        /d1             : Displays the number of states and search depth for each minimax move (Default = False)
        /d2             : Displays all the moves that minimax considers (Default = False)
        /include_trumps : Enable trump cards in the game (Default = True)
        /log            : Enable logs for writing in the file (Default = False)
        /open_world     : Make all cards visible to both players (Default = False)
        /seed           : A seed for random number generation (Int32)
        /start_rank     : The starting rank of cards in the deck (Int32) (Default = 6)
        /total_games    : The number of games to play (Int32) (Default = 1000)
        /tournament     : Runs the tournament with the agents specified. E.g: -tournament="random,greedy,smart,minimax:depth=5,mcts:limit=100" (String)
        /verbose        : Enable verbose output (Default = False)

        Possible Agents:
                random
                greedy
                smart
                minimax:depth=<value>,samples=<value>,eval=<playout/basic>
                mcts:limit=<value>,c=<value>,simulation=<greedy/playout>,samples=<value>

        usage example for multiple games:
                dotnet run --project .\CLI\ -ai1=random -ai2=greedy -total_games=1000 -verbose=true

        usage example for a specific game:
                dotnet run --project .\CLI\ -ai1=random -ai2=greedy -seed=29 -verbose=false

        usage example for a tournament between all the agents
                $dotnet run --project .\CLI\ -tournament="random, greedy, smart, minimax:depth=6, mcts:limit=100" -total_games=500 -open_world -start_rank=10
```

## Folder Structure

The project root directory consists of the following folders:

* Thesis: Contains the research work
* Poster: Contains the poster in SVG format
* Durak: Contains the implementation of the project

The Durak folder contains the following sub-folders:

* Agents: Contains all the AI agents' implementations
* CLI: Serves as the controller for the project and implements the command line interface
* Model: Contains the implementation of the game mechanics, including the rules and logic

Each folder contains the relevant files to their respective tasks, making the structure of the project easy to navigate and understand.

## License

This project is licensed under the MIT License. For more information, see the [LICENSE](LICENSE) file.