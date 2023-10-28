using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class MultiplayerGame
    {
        private NetworkManager networkManager;
        private Thread dataReciever;

        private bool canStart = false;


        private Vector2 mapSize;
        private int mineCount;
        private Cell[,] map;
        private Cell[,] mapEnemy;

        private Vector2 cursorPosition;

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
            Console.WriteLine("Waiting for the other player!");

            networkManager.server.StartListening();

            Console.WriteLine("Client connected! Press any key when ready!");
            Console.ReadLine();

            dataReciever = new Thread(() => networkManager.StartReceivingData(this));
            dataReciever.Start();

            networkManager.SendData("CAN_START");

            while (!canStart)
            {
                ;
            }


        }

        private void ConnectToRoom()
        {
            Console.Clear();
            Console.WriteLine("Input room code: ");

            string hexCode = Console.ReadLine();
            hexCode = hexCode.Trim();
            hexCode = hexCode.ToUpper();

            networkManager.client.SetClientIP(hexCode);
            networkManager.client.TryConnecting();

            Console.WriteLine("Connected to room! Press any key when ready!");

            dataReciever = new Thread(() => networkManager.StartReceivingData(this));
            dataReciever.Start();

            networkManager.SendData("CAN_START");

            while(!canStart)
            {
                ;
            }



        }

        private void PrintMaps()
        {


        }

        public void HandleRecievedData(string data)
        {
            if (data.Contains("CAN_START"))
                canStart = true;
            
        }

        public void AbortRecieverThread()
        {
            dataReciever.Abort();
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
