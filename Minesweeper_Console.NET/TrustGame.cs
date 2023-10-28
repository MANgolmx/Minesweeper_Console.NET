using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class TrustGame
    {
        private NetworkManager networkManager;

        private Thread dataReciever;

        private bool canStartFlag = false;
        private bool mapCreatedFlag = false;
        private bool host = false;

        private Vector2 mapSize;
        private int mineCount;
        private Cell[,] map;

        private Vector2 cursorPosition;

        public TrustGame()
        {
            networkManager = new NetworkManager();
        }

        public void StartGame()
        {
            switch (MultiplayerMenu())
            {
                case 0:
                    CreateRoom();
                    break;
                case 1:
                    ConnectToRoom();
                    break;
                case -1:
                    return;
            }
        }

        private void CreateRoom()
        {
            Console.Clear();

            string hexIP = networkManager.server.GetHexIPAddress();
            Console.WriteLine("Room code: " + hexIP);
            Console.WriteLine("Waiting for the other player to connect!");

            networkManager.server.StartListening();

            dataReciever = new Thread(() => networkManager.StartReceivingData(this));
            dataReciever.Start();

            host = true;

            while (!networkManager.readyToPlay)
            {
                ;
            }

            Console.WriteLine("Player connected! Press any key when ready!");
            Console.ReadKey();

            networkManager.SendData("CAN_START");

            while(!canStartFlag)
            {
                ;
            }

            GetMapInfo();
            Console.Clear();

            networkManager.SendData("CREATE_MAP " + mineCount + " " + (int)mapSize.X + " " + (int)mapSize.Y);

            CreateMap();

            ChooseFirstInput();
        }

        private void ConnectToRoom()
        {
            string hexCode;
            bool isViable = false;
            do
            {
                Console.Clear();
                Console.WriteLine("Input room code: ");

                hexCode = Console.ReadLine();
                hexCode = hexCode.Trim();
                hexCode = hexCode.ToUpper();

                try
                {
                    networkManager.client.SetClientIP(hexCode);
                    isViable = true;
                }
                catch
                {
                    Console.WriteLine("Wrong room code!");
                    Console.ReadKey();
                }
            } while (!isViable);

            networkManager.client.TryConnecting();

            networkManager.SendData("HEXCLIENTIP " + networkManager.server.GetHexIPAddress());
            networkManager.server.StartListening();

            dataReciever = new Thread(() => networkManager.StartReceivingData(this));
            dataReciever.Start();

            Console.WriteLine("Connected to room! Press any key when ready!");
            Console.ReadKey();

            networkManager.SendData("CAN_START");

            while (!canStartFlag)
            {
                ;
            }

            Console.WriteLine("Waiting for the host to create a map...");

            while (!mapCreatedFlag)
            {
                ;
            }

            ChooseFirstInput();
        }

        private void ChooseFirstInput()
        {
            while (true)
            {
                Console.Clear();
                PrintMap();
                //if (InputManager(true) == 1)
                {
                    //ManageGame();
                    return;
                }
            }
        }

        private void PrintMap()
        {
            for (int i = 0; i < mapSize.X; i++)
            {
                for (int j = 0; j < mapSize.Y; j++)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    if (cursorPosition.X == i && cursorPosition.Y == j)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                    }

                    if (map[i, j].isOpened)
                    {
                        if (map[i, j].isMine)
                            Console.Write("*");
                        else
                        {
                            switch (CalculateAdjascentMines(new Vector2(i, j)))
                            {
                                case 1:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("1");
                                    break;
                                case 2:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("2");
                                    break;
                                case 3:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Cyan;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("3");
                                    break;
                                case 4:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Blue;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("4");
                                    break;
                                case 5:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Magenta;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("5");
                                    break;
                                case 6:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("6");
                                    break;
                                case 7:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("7");
                                    break;
                                case 8:
                                    Console.BackgroundColor = ConsoleColor.Black;
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    if (cursorPosition.X == i && cursorPosition.Y == j)
                                        Console.BackgroundColor = ConsoleColor.DarkGray;
                                    Console.Write("8");
                                    break;
                                default:
                                    Console.Write(" ");
                                    break;
                            }
                        }
                        continue;
                    }
                    if (map[i, j].isUndefined)
                    {
                        Console.Write("?");
                        continue;
                    }
                    if (map[i, j].isFlagged)
                    {
                        Console.Write("!");
                        continue;
                    }
                    Console.Write(".");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n");
            }
        }

        private void GetMapInfo()
        {
            int x = 0;
            int y = 0;
            bool rightInputFlag = false;
            do
            {
                Console.Clear();
                Console.Write("Input map size: ");
                string tokens = Console.ReadLine();

                try
                {
                    int spaceCount = 0;
                    int index = 0;
                    foreach (var s in tokens)
                    {
                        if ((s == ' ' || s == '\n') && spaceCount < 1)
                            spaceCount++;
                        else if (s == ' ' || s == '\n')
                            tokens.Remove(index);
                        index++;
                    }

                    tokens.Split();

                    x = int.Parse(tokens.Split()[0]);
                    y = int.Parse(tokens.Split()[1]);

                    if (x < 5 || y < 5)
                    {
                        Console.WriteLine("\nMap must be bigger than 5x5");
                        Console.ReadKey();
                    }
                    else if (x > 70 && y > 70)
                    {
                        Console.WriteLine("\nMap must be smaller than 70x70");
                        Console.ReadKey();
                    }
                    else rightInputFlag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nSomething went wrong!\n" + ex.Message);
                    Console.ReadKey();
                    rightInputFlag = false;
                }
            } while (!rightInputFlag);

            rightInputFlag = false;
            int mines = 0;

            do
            {
                Console.Clear();
                Console.Write("Input mine count: ");
                try
                {
                    mines = int.Parse(Console.ReadLine());

                    if (mines < 5)
                    {
                        Console.WriteLine("\nThink bigger!");
                        Console.ReadKey();
                    }
                    else if (mines > (x*y) / 2)
                    {
                        Console.WriteLine("Oh you think you will survive that? No way! Go easy on yourself!");
                        Console.ReadKey();
                    }
                    else rightInputFlag = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\nSomething went wrong!\n" + ex.Message);
                    Console.ReadKey();
                    rightInputFlag = false;
                }

            } while (!rightInputFlag);

            mapSize.X = x;
            mapSize.Y = y;

            mineCount = mines;
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

                Console.Write("\nRules: Host sees map with coordinates and can move the cursor around the board. The other player sees full map but right side up. " +
                    "He has to coordinate player around obstacle to win the game. Every 6th move of the host, second player can see his cursor. Traps could spawn on the map, " +
                    "which only the second player can see. If the host steps on one of the traps, coordinates get rerandomized and map of the second player rotates");

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
                    case ConsoleKey.Escape:
                        return -1;
                }
            }
        }

        public void HandleRecievedData(string data)
        {
            if (data.Contains("HEXCLIENTIP"))
            {
                data = data.Replace("HEXCLIENTIP ", "");
                networkManager.client.SetClientIP(data);
                networkManager.client.TryConnecting();

                networkManager.readyToPlay = true;
            }
            else if (data.Contains("CAN_START"))
                canStartFlag = true;
            else if (data.Contains("CREATE_MAP"))
            {
                data = data.Replace("CREATE_MAP ", "");

                mineCount = int.Parse(data.Split()[0]);
                mapSize = new Vector2(int.Parse(data.Split()[1]), int.Parse(data.Split()[2]));

                CreateMap();

                mapCreatedFlag = true;
            } else if (data.Contains("FILL_MAP"))
            {
                int CursorX = 0;
                int CursorY = 0;

                data = data.Replace("FILL_MAP ", "");

                string[] tokens = data.Split();

                CursorX = int.Parse(tokens[0]);
                CursorY = int.Parse(tokens[1]);

                for (int i = 2; i < mineCount * 2; i += 2)
                    map[int.Parse(tokens[i]), int.Parse(tokens[i + 1])].isMine = true;

                OpenCells(new Vector2(CursorX, CursorY));
            }
            
        }

        private void FillMap()
        {
            int generatedMines = 0;
            string minesData = "FILL_MAP " + cursorPosition.X + " " + cursorPosition.Y;

            do
            {
                int posX = Random.Shared.Next((int)mapSize.X);
                if ((int)cursorPosition.X == posX)
                    continue;

                int posY = Random.Shared.Next((int)mapSize.Y);
                if ((int)cursorPosition.Y == posY)
                    continue;

                if (!map[posX, posY].isMine && (map[posX, posY].isMine = true))
                {
                    generatedMines++;
                    minesData += (" " + posX + " " + posY);
                }

            } while (generatedMines < mineCount);

            OpenCells(cursorPosition);

            networkManager.SendData(minesData);
        }

        private void OpenCells(Vector2 pos)
        {
            if (map[(int)pos.X, (int)pos.Y].isMine)
                return;

            if (map[(int)pos.X, (int)pos.Y].isOpened)
                return;

            if (CalculateAdjascentMines(pos) > 0)
            {
                map[(int)pos.X, (int)pos.Y].isOpened = true;
                return;
            }
            else
                map[(int)pos.X, (int)pos.Y].isOpened = true;

            if (pos.X > 0) OpenCells(new Vector2(pos.X - 1, pos.Y));
            if (pos.X < mapSize.X - 1) OpenCells(new Vector2(pos.X + 1, pos.Y));
            if (pos.Y > 0) OpenCells(new Vector2(pos.X, pos.Y - 1));
            if (pos.Y < mapSize.Y - 1) OpenCells(new Vector2(pos.X, pos.Y + 1));
        }

        private int CalculateAdjascentMines(Vector2 pos)
        {
            int mines = 0;

            if (pos.X > 0 && map[(int)pos.X - 1, (int)pos.Y].isMine) mines++;
            if (pos.X > 0 && pos.Y > 0 && map[(int)pos.X - 1, (int)pos.Y - 1].isMine) mines++;
            if (pos.X > 0 && pos.Y < mapSize.Y - 1 && map[(int)pos.X - 1, (int)pos.Y + 1].isMine) mines++;
            if (pos.Y > 0 && map[(int)pos.X, (int)pos.Y - 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && map[(int)pos.X + 1, (int)pos.Y].isMine) mines++;
            if (pos.Y < mapSize.Y - 1 && map[(int)pos.X, (int)pos.Y + 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && pos.Y < mapSize.Y - 1 && map[(int)pos.X + 1, (int)pos.Y + 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && pos.Y > 0 && map[(int)pos.X + 1, (int)pos.Y - 1].isMine) mines++;

            return mines;
        }

        private void CreateMap()
        {
            map = new Cell[(int)mapSize.X, (int)mapSize.Y];

            for (int i = 0; i < mapSize.X; i++)
                for (int j = 0; j < mapSize.Y; j++)
                    map[i, j] = new Cell();
        }

        public void AbortRecieverThread()
        {
            return;
        }

    }
}
