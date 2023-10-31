using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class Minesweeper
    {
        SingleGame singleGame;
        SurvivalGame multiplayerGame;
        TrustGame trustGame;

        public Minesweeper()
        {
            //singleGame = new SingleGame();
            //multiplayerGame = new MultiplayerGame();
            //trustGame = new TrustGame();
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

                ConsoleKeyInfo pressedKey = Console.ReadKey();
                switch(pressedKey.Key)
                {
                    case ConsoleKey.Enter:
                        return choice;
                    case ConsoleKey.DownArrow:
                        if (choice < 2)
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
            switch(MainMenu())
            {
                case 0:
                    singleGame = new SingleGame();
                    singleGame.StartGame();
                    break;
                case 1:
                    multiplayerGame = new SurvivalGame();
                    multiplayerGame.StartGame();
                    break;
                case 2:
                    trustGame = new TrustGame();
                    trustGame.StartGame();
                    break;

            }

        }

    }
}
