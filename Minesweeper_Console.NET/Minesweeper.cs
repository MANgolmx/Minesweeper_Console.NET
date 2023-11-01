using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class Minesweeper
    {
        string playerName;

        SingleGame singleGame;
        SurvivalGame multiplayerGame;
        TrustGame trustGame;
        TeamGame teamGame;

        public Minesweeper()
        {
            //singleGame = new SingleGame();
            //multiplayerGame = new MultiplayerGame();
            //trustGame = new TrustGame();

            playerName = "Elena Abovyan #" + Random.Shared.Next(100);
        }

        private int MainMenu()
        {
            int choice = 0;

            while (true) {
                Console.Clear();
                if (choice == 0)
                    Console.Write("---> ");
                Console.Write("New game\n");
                if (choice == 1)
                    Console.Write("---> ");
                Console.Write("1v1 game\n");
                if (choice == 2)
                    Console.Write("---> ");
                Console.Write("Play with friend\n");
                if (choice == 3)
                    Console.Write("---> ");
                Console.Write("2v2 game\n");
                if (choice == 4)
                    Console.Write("---> ");
                Console.Write("Name: " + playerName + "\n");

                ConsoleKeyInfo pressedKey = Console.ReadKey();
                switch(pressedKey.Key)
                {
                    case ConsoleKey.Enter:
                        return choice;
                    case ConsoleKey.DownArrow:
                        if (choice < 3)
                            choice++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (choice > 0)
                            choice--;
                        break;
                    case ConsoleKey.Escape:
                        Environment.Exit(0);
                        return 0;
                }
            }
        }

        public void Start()
        {
            bool isStarted = true;
            while (isStarted)
            {
                switch (MainMenu())
                {
                    case 0:
                        singleGame = new SingleGame();
                        singleGame.StartGame();
                        isStarted = false;
                        break;
                    case 1:
                        multiplayerGame = new SurvivalGame(playerName);
                        multiplayerGame.StartGame();
                        isStarted = false;
                        break;
                    case 2:
                        trustGame = new TrustGame();
                        trustGame.StartGame();
                        isStarted = false;
                        break;
                    case 3:
                        teamGame = new TeamGame(playerName);
                        teamGame.StartGame();
                        isStarted = false;
                        break;
                    case 4:
                        Console.Write("\n Input your new name: ");
                        playerName = Console.ReadLine();
                        break;

                }
            }
        }

    }
}
