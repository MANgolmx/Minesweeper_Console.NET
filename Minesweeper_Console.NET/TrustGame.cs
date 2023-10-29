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

        private bool isPlaying = true;
        private bool waitForUpdate = true;

        private int trapsCount;

        private Vector2 mapSize;
        private int mineCount;
        private Cell[,] map;

        private const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ@$%";
        private string[] coords;

        private Vector2 cursorPosition;

        public TrustGame()
        {
            networkManager = new NetworkManager();
            coords = new string[2];
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

            Console.WriteLine("Waiting for the other player to get ready...");

            while (!canStartFlag)
            {
                ;
            }

            GetMapInfo();
            Console.Clear();

            CreateMap();

            networkManager.SendData("CREATE_MAP " + mineCount + " " + (int)mapSize.X + " " + (int)mapSize.Y + " " + coords[0] + " " + coords[1]);

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

            Console.WriteLine("Waiting for the host to get ready...");

            while (!canStartFlag)
            {
                ;
            }

            Console.WriteLine("Waiting for the host to create a map...");

            while (!mapCreatedFlag)
            {
                ;
            }

            ManageGame();
        }

        private void ChooseFirstInput()
        {
            while (true)
            {
                Console.Clear();
                PrintMap();
                if (InputManager(true) == 1)
                {
                    ManageGame();
                    return;
                }
            }
        }

        private void ManageGame()
        {
            if (host)
            {
                isPlaying = true;

                while (isPlaying)
                {
                    Console.Clear();
                    PrintMap();
                    if (InputManager() == 1)
                    {
                        isPlaying = false;
                        Console.WriteLine("\nYou blew up! Be careful next time!\n");
                        networkManager.SendData("LOST");
                        Console.ReadKey();
                    }

                    if (CheckWin() == 1)
                    {
                        isPlaying = false;
                        Console.WriteLine("\nYou won! Good job boss man!\n");
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                isPlaying = true;
                while (isPlaying)
                {
                    Console.Clear();
                    PrintMap();

                    if (CheckWin() == 1)
                    {
                        isPlaying = false;
                        Console.WriteLine("\nYou won! Good job boss man!\n");
                        Console.ReadKey();
                    }

                    while (waitForUpdate)
                    {
                        ;
                    }
                    waitForUpdate = true;
                }
            }
        }

        private int CheckWin()
        {
            for (int i = 0; i < mapSize.X; i++)
                for (int j = 0; j < mapSize.Y; j++)
                    if (!map[i, j].isOpened && !map[i, j].isMine)
                        return 0;
            return 1;
        }

        private int InputManager(bool firstInput = false)
        {
            if (!host)
                return 0;
            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;

                case ConsoleKey.DownArrow:
                    if (cursorPosition.X < mapSize.X - 1)
                        cursorPosition.X++;
                    break;

                case ConsoleKey.UpArrow:
                    if (cursorPosition.X > 0)
                        cursorPosition.X--;
                    break;

                case ConsoleKey.LeftArrow:
                    if (cursorPosition.Y > 0)
                        cursorPosition.Y--;
                    else if (cursorPosition.X > 0)
                    {
                        cursorPosition.Y = mapSize.Y - 1;
                        cursorPosition.X--;
                    }
                    break;

                case ConsoleKey.RightArrow:

                    if (cursorPosition.Y < mapSize.Y - 1)
                        cursorPosition.Y++;
                    else if (cursorPosition.X < mapSize.X - 1)
                    {
                        cursorPosition.Y = 0;
                        cursorPosition.X++;
                    }
                    break;

                case ConsoleKey.Delete:
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isUndefined = false;
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged = false;
                    networkManager.SendData("DELETE_CELL " + (int)cursorPosition.X + " " + (int)cursorPosition.Y);
                    break;

                case ConsoleKey.Q:
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isUndefined = !map[(int)cursorPosition.X, (int)cursorPosition.Y].isUndefined;
                    networkManager.SendData("UNDEFINED_CELL " + (int)cursorPosition.X + " " + (int)cursorPosition.Y);
                    break;

                case ConsoleKey.Tab:
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged = !map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged;
                    networkManager.SendData("FLAGGED_CELL " + (int)cursorPosition.X + " " + (int)cursorPosition.Y);
                    break;

                case ConsoleKey.Enter:
                    if (firstInput)
                    {
                        FillMap();
                        return 1;
                    }
                    else if (!map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged)
                    {
                        if (!map[(int)cursorPosition.X, (int)cursorPosition.Y].isMine && CalculateAdjascentMines(cursorPosition) == 0)
                        {
                            OpenCells(cursorPosition);
                            networkManager.SendData("OPEN_CELLS " + (int)cursorPosition.X + " " + (int)cursorPosition.Y);
                        }
                        else
                        {
                            map[(int)cursorPosition.X, (int)cursorPosition.Y].isOpened = true;
                            networkManager.SendData("OPEN_CELLS " + (int)cursorPosition.X + " " + (int)cursorPosition.Y);
                            if (map[(int)cursorPosition.X, (int)cursorPosition.Y].isMine)
                                return 1;
                        }
                    }
                    break;
            }

            CheckTrap();

            return 0;
        }

        private void CheckTrap()
        {
            if (map[(int)cursorPosition.X, (int)cursorPosition.Y].isTrap)
            {
                CreateCoords();
                networkManager.SendData("COORDS " + coords[0] + " " + coords[1]);
            }
        }

        private void PrintMap()
        {
            Console.Write("  ");
            for (int k = 0; k < mapSize.Y; k++)
                Console.Write(coords[1][k]);
            Console.Write("\n");

            if (!host)
                for (int i = 0; i < mapSize.X; i++)
                {
                    for (int j = 0; j < mapSize.Y; j++)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (j == 0)
                            Console.Write(coords[0][i] + " ");

                        if (map[i, j].isOpened)
                        {
                            if (map[i, j].isMine)
                                Console.Write("*");
                            else if (map[i, j].isTrap)
                            {
                                Console.BackgroundColor = ConsoleColor.White;
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(" ");
                                continue;
                            }
                            else
                            {
                                switch (CalculateAdjascentMines(new Vector2(i, j)))
                                {
                                    case 1:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("1");
                                        break;
                                    case 2:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.Write("2");
                                        break;
                                    case 3:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.Write("3");
                                        break;
                                    case 4:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.Write("4");
                                        break;
                                    case 5:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                        Console.Write("5");
                                        break;
                                    case 6:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                        Console.Write("6");
                                        break;
                                    case 7:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.Write("7");
                                        break;
                                    case 8:
                                        Console.BackgroundColor = ConsoleColor.Black;
                                        Console.ForegroundColor = ConsoleColor.DarkRed;
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
            else
                for (int i = 0; i < mapSize.X; i++)
                {
                    for (int j = 0; j < mapSize.Y; j++)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (j == 0)
                            Console.Write(coords[0][i] + " ");
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
                                int t = Random.Shared.Next(13);
                                switch (CalculateAdjascentMines(new Vector2(i, j)))
                                {
                                    case 1:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 2:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 3:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 4:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 5:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 6:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 7:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    case 8:
                                        if (cursorPosition.X == i && cursorPosition.Y == j)
                                            Console.BackgroundColor = ConsoleColor.Gray;
                                        if (t >= 10)
                                            Console.Write(".");
                                        else
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(t);
                                        }
                                        break;
                                    default:
                                        int a = Random.Shared.Next(12);
                                        if (t < 1)
                                            Console.Write("*");
                                        else if (t < 2)
                                        {
                                            Console.BackgroundColor = ConsoleColor.Black;
                                            Console.ForegroundColor = (ConsoleColor)Random.Shared.Next(16);
                                            Console.Write(Random.Shared.Next(10));
                                        }
                                        else if (t < 3)
                                            Console.Write(".");
                                        else
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
                    else if (x > 60 && y > 60)
                    {
                        Console.WriteLine("\nMap must be smaller than 60x60");
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
            if (data.ToUpper().Contains("CREATE_MAP"))
            {
                data = data.Replace("CREATE_MAP ", "");

                mineCount = int.Parse(data.Split()[0]);
                mapSize = new Vector2(int.Parse(data.Split()[1]), int.Parse(data.Split()[2]));

                coords[0] = data.Split()[3];
                coords[1] = data.Split()[4];

                CreateMap();

                mapCreatedFlag = true;
            }

            data = data.ToUpper();

            if (data.Contains("HEXCLIENTIP"))
            {
                data = data.Replace("HEXCLIENTIP ", "");
                networkManager.client.SetClientIP(data);
                networkManager.client.TryConnecting();

                networkManager.readyToPlay = true;
            }
            else if (data.Contains("CAN_START"))
                canStartFlag = true;
            else if (data.Contains("FILL_MAP"))
            {
                int CursorX = 0;
                int CursorY = 0;

                data = data.Replace("FILL_MAP ", "");

                string[] tokens = data.Split();

                CursorX = int.Parse(tokens[0]);
                CursorY = int.Parse(tokens[1]);

                for (int i = 2; i < mineCount * 2 + 1; i += 2)
                    map[int.Parse(tokens[i]), int.Parse(tokens[i + 1])].isMine = true;

                List<Vector2> freeCells = new List<Vector2>();

                for (int i = 0; i < mapSize.X; i++)
                    for (int j = 0; j < mapSize.Y; j++)
                        if (CalculateAdjascentMines(new Vector2(i,j)) == 0)
                            freeCells.Add(new Vector2(i,j));

                while (trapsCount > freeCells.Count / 2)
                {
                    trapsCount /= 2;
                }

                int createdTraps = 0;

                string trapsData = "" + trapsCount;

                do
                {
                    int posIndex = Random.Shared.Next(freeCells.Count);
                    if (!map[(int)freeCells[posIndex].X, (int)freeCells[posIndex].Y].isTrap && (map[(int)freeCells[posIndex].X, (int)freeCells[posIndex].Y].isTrap = true))
                    {
                        trapsData += " " + (int)freeCells[posIndex].X + " " + (int)freeCells[posIndex].Y;
                        createdTraps++;
                    }
                } while (createdTraps < trapsCount);

                networkManager.SendData("FILL_TRAPS " + trapsData);

                OpenCells(new Vector2(CursorX, CursorY));
                waitForUpdate = false;
            }
            else if (data.Contains("COORDS"))
            {
                coords[0] = data.Split()[1];
                coords[1] = data.Split()[2];
                waitForUpdate = false;
            }
            else if (data.Contains("FILL_TRAPS"))
            {
                data = data.Replace("FILL_TRAPS ", "");
                string[] tokens = data.Split();

                trapsCount = int.Parse(tokens[0]);

                for (int i = 1; i < trapsCount * 2; i+=2)
                    map[int.Parse(tokens[i]), int.Parse(tokens[i + 1])].isTrap = true;
            }
            else if (data.Contains("OPEN_CELLS"))
            {
                string[] tokens = data.Split();
                OpenCells(new Vector2(int.Parse(tokens[1]), int.Parse(tokens[2])));
                waitForUpdate = false;
            }
            else if (data.Contains("FLAGGED_CELL"))
            {
                map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isFlagged = !map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isFlagged;
                waitForUpdate = false;
            }
            else if (data.Contains("UNDEFINED_CELL"))
            {
                map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isUndefined = !map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isUndefined;
                waitForUpdate = false;
            } 
            else if (data.Contains("DELETE_CELL"))
            {
                map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isUndefined = false;
                map[int.Parse(data.Split()[1]), int.Parse(data.Split()[2])].isFlagged = false;
                waitForUpdate = false;
            }
            else if (data.Contains("LOST"))
            {
                Console.WriteLine("Your partner blew up :(");
                Console.ReadKey();
                isPlaying = false;
                waitForUpdate = false;
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
                    if (CalculateAdjascentMines(new Vector2((int)cursorPosition.X, (int)cursorPosition.Y)) > 0)
                    {
                        map[posX, posY].isMine = false;
                        continue;
                    }
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
            if (pos.X > 0 && pos.Y > 0) OpenCells(new Vector2(pos.X - 1, pos.Y - 1));
            if (pos.X > 0 && pos.Y < mapSize.Y - 1) OpenCells(new Vector2(pos.X - 1, pos.Y + 1));
            if (pos.X < mapSize.X - 1 && pos.Y > 0) OpenCells(new Vector2(pos.X + 1, pos.Y - 1));
            if (pos.X < mapSize.X - 1 && pos.Y < mapSize.Y - 1) OpenCells(new Vector2(pos.X + 1, pos.Y + 1));      
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

            trapsCount = (int)(mapSize.X * mapSize.Y * 0.05);

            if (host)
                CreateCoords();
        }

        private void CreateCoords()
        {
            coords[0] = "";
            for (int i = 0; i < mapSize.X; i++)
            {
                int t = Random.Shared.Next(alphabet.Length);
                if (!coords[0].Contains(alphabet[t]))
                    coords[0] += alphabet[t];
                else i--;
            }

            coords[1] = "";
            for (int i = 0; i < mapSize.Y; i++)
            {
                int t = Random.Shared.Next(alphabet.Length);
                if (!coords[1].Contains(alphabet[t]))
                    coords[1] += alphabet[t];
                else i--;
            }
        }

        public void AbortRecieverThread()
        {
            isPlaying = false;
            waitForUpdate = false;
            return;
        }

    }
}
