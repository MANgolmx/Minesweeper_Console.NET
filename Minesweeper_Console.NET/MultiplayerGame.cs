using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class MultiplayerGame
    {
        NetworkManager networkManager;
        public MultiplayerGame()
        {
            networkManager = new NetworkManager();
        }

        public void StartGame()
        {
            switch(MultiplayerMenu())
            {
                case 0:
                    CreateRoom();
                    break;
                case 1:
                    ConnectToRoom();
                    break;

            }

        }

        private void CreateRoom()
        {
            Console.Clear();

            string hexIP = networkManager.server.GetHexIPAddress();
            Console.WriteLine("Room code: " + hexIP);


            Console.ReadKey();
        }

        private void ConnectToRoom()
        {
            Console.Clear();
            Console.WriteLine("Input room code: ");

            Console.ReadLine();



        }

        private int MultiplayerMenu()
        {
            int choice = 0;

            while (true)
            {
                Console.Clear();
                if (choice == 0)
                    Console.Write("---> ");
                Console.Write("Create a room\n");
                if (choice == 1)
                    Console.Write("---> ");
                Console.Write("Connect to a room\n");

                ConsoleKeyInfo pressedKey = Console.ReadKey();
                switch (pressedKey.Key)
                {
                    case ConsoleKey.Enter:
                        return choice;
                    case ConsoleKey.DownArrow:
                        if (choice < 1)
                            choice++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (choice > 0)
                            choice--;
                        break;
                }
            }
        }

    }
}
