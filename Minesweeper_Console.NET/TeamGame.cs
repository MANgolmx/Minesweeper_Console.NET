﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Minesweeper_Console.NET
{
    class TeamGame
    {
        string[] playerNames = new string[4];

        private NetworkManager networkManager;
        private Thread dataReciever;

        private bool[] canStartFlag = new bool[3];
        private bool mapCreatedFlag = false;

        private bool waitingForInput = false;

        private bool enemyWon = false;
        private bool enemyLost = false;

        private Vector2 mapSize;
        private int mineCount;
        private Cell[,] map;
        private Cell[,,] mapEnemy;

        private Vector2 cursorPosition;

        public TeamGame(string playerName)
        {
            networkManager = new NetworkManager();
            cursorPosition = new Vector2(0, 0);

            for (int i = 0; i < 3; i++)
                canStartFlag[i] = false;

            playerNames[0] = playerName;
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

            while (!networkManager.readyToPlay)
            {
                ;
            }

            Console.WriteLine("Player connected! Press any key when ready!");
            waitingForInput = true;
            Console.ReadKey();
            waitingForInput = false;

            networkManager.SendData("CAN_START", 3);

            Console.WriteLine("Waiting for the other player to get ready...");

            System.Timers.Timer requestStartFlag = new System.Timers.Timer();
            requestStartFlag.Interval = 400;
            requestStartFlag.Elapsed += RequestStartFlag;
            requestStartFlag.Start();

            while (!canStartFlag[2])
            {
                ;
            }

            requestStartFlag.Stop();

            waitingForInput = true;
            GetMapInfo();
            Console.Clear();
            waitingForInput = false;

            networkManager.SendData("CREATE_MAP " + mineCount + " " + (int)mapSize.X + " " + (int)mapSize.Y, 3);

            CreateMap();
            CreateEnemyMaps();

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
                    networkManager.SetClientHexIP(hexCode);
                    isViable = true;
                }
                catch
                {
                    Console.WriteLine("Wrong room code!");
                    Console.ReadKey();
                }
            } while (!isViable);

            networkManager.clients[0].TryConnecting();

            networkManager.SendData("HEXCLIENTIP " + networkManager.server.GetHexIPAddress(), 3);
            networkManager.server.StartListening();

            dataReciever = new Thread(() => networkManager.StartReceivingData(this));
            dataReciever.Start();

            Console.WriteLine("Connected to room! Press any key when ready!");
            waitingForInput = true;
            Console.ReadKey();
            waitingForInput = false;

            networkManager.SendData("CAN_START", 3);

            Console.WriteLine("Waiting for the host to get ready...");

            System.Timers.Timer requestStartFlag = new System.Timers.Timer();
            requestStartFlag.Interval = 400;
            requestStartFlag.Elapsed += RequestStartFlag;
            requestStartFlag.Start();

            while (!canStartFlag[2])
            {
                ;
            }

            requestStartFlag.Stop();

            Console.WriteLine("Waiting for the host to create a map...");

            System.Timers.Timer requestMap = new System.Timers.Timer();
            requestMap.Interval = 500;
            requestMap.Elapsed += RequestMapData;
            requestMap.Start();

            while (!mapCreatedFlag)
            {
                ;
            }

            requestMap.Stop();

            ChooseFirstInput();
        }

        private void RequestStartFlag(object? sender, ElapsedEventArgs e)
        {
            networkManager.SendData("REQUEST_STARTFLAG", 3);
        }

        private void RequestMapData(object? sender, ElapsedEventArgs e)
        {
            networkManager.SendData("REQUEST_MAP", 3);
        }

        private void ChooseFirstInput()
        {
            while (true)
            {
                Console.Clear();
                PrintMaps();
                if (InputManager(true) == 1)
                {
                    ManageGame();
                    return;
                }
            }
        }

        private void ManageGame()
        {
            bool isPlaying = true;

            while (isPlaying)
            {
                Console.Clear();
                PrintMaps();
                if (InputManager() == 1)
                {
                    isPlaying = false;
                    Console.WriteLine("\nYou blew up! Be careful next time!\n");
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

        private int CheckWin()
        {
            for (int i = 0; i < mapSize.X; i++)
                for (int j = 0; j < mapSize.Y; j++)
                    if (!map[i, j].isOpened && !map[i, j].isMine)
                        return 0;
            return 1;
        }

        private void PrintMaps()
        {
            for (int i = 0; i < mapSize.X; i++)
            {
                for (int m = 0; m < 2; m++)
                {
                    for (int j = 0; j < mapSize.Y; j++)
                    {
                        if (m == 0)
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
                        else
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            if (mapEnemy[0, i, j].isOpened)
                            {
                                if (mapEnemy[0, i, j].isMine)
                                    Console.Write("*");
                                else
                                {
                                    switch (0)//CalculateAdjascentEnemyMines(new Vector2(i, j)))
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
                                            Console.Write("4");
                                            break;
                                        default:
                                            Console.Write(" ");
                                            break;
                                    }
                                }
                                continue;
                            }
                            if (mapEnemy[0, i, j].isUndefined)
                            {
                                Console.Write("?");
                                continue;
                            }
                            if (mapEnemy[0, i, j].isFlagged)
                            {
                                Console.Write("!");
                                continue;
                            }
                            Console.Write(".");
                        }
                    }
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  |  ");
                }
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\n");
            }
            if (enemyWon)
            {
                for (int i = 0; i < mapSize.Y + 5; i++)
                    Console.Write(" ");
                Console.Write("Player Won!");
            }
            else if (enemyLost)
            {
                for (int i = 0; i < mapSize.Y + 5; i++)
                    Console.Write(" ");
                Console.Write("Player Lost!");
            }
        }

        private int InputManager(bool firstInput = false)
        {
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
                    break;

                case ConsoleKey.Q:
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isUndefined = !map[(int)cursorPosition.X, (int)cursorPosition.Y].isUndefined;
                    break;

                case ConsoleKey.Tab:
                    map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged = !map[(int)cursorPosition.X, (int)cursorPosition.Y].isFlagged;
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
                            networkManager.SendData("OPEN_ENEMY_CELLS " + (int)cursorPosition.X + " " + (int)cursorPosition.Y, 3);
                        }
                        else
                        {
                            map[(int)cursorPosition.X, (int)cursorPosition.Y].isOpened = true;
                            networkManager.SendData("OPEN_ENEMY_CELLS " + (int)cursorPosition.X + " " + (int)cursorPosition.Y, 3);
                            if (map[(int)cursorPosition.X, (int)cursorPosition.Y].isMine)
                                return 1;
                        }
                    }
                    break;
            }

            return 0;
        }

        private void FillMap()
        {
            int generatedMines = 0;
            string minesData = "FILL_ENEMY_MAP " + cursorPosition.X + " " + cursorPosition.Y;

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

            networkManager.SendData(minesData, 3);
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

        private void OpenEnemyCells(Vector2 pos, int mapInd)
        {
            if (mapEnemy[mapInd, (int)pos.X, (int)pos.Y].isMine)
                return;

            if (mapEnemy[mapInd, (int)pos.X, (int)pos.Y].isOpened)
                return;

            if (true)//CalculateAdjascentEnemyMines(pos) > 0)
            {
                mapEnemy[mapInd, (int)pos.X, (int)pos.Y].isOpened = true;
                return;
            }
            else
                mapEnemy[mapInd, (int)pos.X, (int)pos.Y].isOpened = true;

            if (pos.X > 0) OpenEnemyCells(new Vector2(pos.X - 1, pos.Y), mapInd);
            if (pos.X < mapSize.X - 1) OpenEnemyCells(new Vector2(pos.X + 1, pos.Y), mapInd);
            if (pos.Y > 0) OpenEnemyCells(new Vector2(pos.X, pos.Y - 1), mapInd);
            if (pos.Y < mapSize.Y - 1) OpenEnemyCells(new Vector2(pos.X, pos.Y + 1), mapInd);
            if (pos.X > 0 && pos.Y > 0) OpenEnemyCells(new Vector2(pos.X - 1, pos.Y - 1), mapInd);
            if (pos.X > 0 && pos.Y < mapSize.Y - 1) OpenEnemyCells(new Vector2(pos.X - 1, pos.Y + 1), mapInd);
            if (pos.X < mapSize.X - 1 && pos.Y > 0) OpenEnemyCells(new Vector2(pos.X + 1, pos.Y - 1), mapInd);
            if (pos.X < mapSize.X - 1 && pos.Y < mapSize.Y - 1) OpenEnemyCells(new Vector2(pos.X + 1, pos.Y + 1), mapInd);
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

        private int CalculateAdjascentEnemyMines(Vector2 pos, int mapInd)
        {
            int mines = 0;

            if (pos.X > 0 && mapEnemy[mapInd, (int)pos.X - 1, (int)pos.Y].isMine) mines++;
            if (pos.X > 0 && pos.Y > 0 && mapEnemy[mapInd, (int)pos.X - 1, (int)pos.Y - 1].isMine) mines++;
            if (pos.X > 0 && pos.Y < mapSize.Y - 1 && mapEnemy[mapInd, (int)pos.X - 1, (int)pos.Y + 1].isMine) mines++;
            if (pos.Y > 0 && mapEnemy[mapInd, (int)pos.X, (int)pos.Y - 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && mapEnemy[mapInd, (int)pos.X + 1, (int)pos.Y].isMine) mines++;
            if (pos.Y < mapSize.Y - 1 && mapEnemy[mapInd, (int)pos.X, (int)pos.Y + 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && pos.Y < mapSize.Y - 1 && mapEnemy[mapInd, (int)pos.X + 1, (int)pos.Y + 1].isMine) mines++;
            if (pos.X < mapSize.X - 1 && pos.Y > 0 && mapEnemy[mapInd, (int)pos.X + 1, (int)pos.Y - 1].isMine) mines++;

            return mines;
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
                    Environment.Exit(1);
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
                    Environment.Exit(1);
                }

            } while (!rightInputFlag);

            mapSize.X = x;
            mapSize.Y = y;

            mineCount = mines;
        }

        public void HandleRecievedData(string data)
        {
            if (data.ToUpper().Contains("CAN_START"))
            {
                if (canStartFlag[0])
                    if (canStartFlag[1])
                        canStartFlag[2] = true;
                    else
                        canStartFlag[1] = true;
                else
                    canStartFlag[0] = true;
            }

            data = data.ToUpper();

            if (data.Contains("HEXCLIENTIP"))
            {
                data = data.Replace("HEXCLIENTIP ", "");
                networkManager.SetClientHexIP(data);
                networkManager.TryClientConnecting();

                networkManager.readyToPlay = true;
            }
            else if (data.Contains("CREATE_MAP"))
            {
                data = data.Replace("CREATE_MAP ", "");

                mineCount = int.Parse(data.Split()[0]);
                mapSize = new Vector2(int.Parse(data.Split()[1]), int.Parse(data.Split()[2]));

                CreateMap();
                CreateEnemyMaps();

                mapCreatedFlag = true;
            }
            else if (data.Contains("REQUEST_MAP"))
            {
                if (!waitingForInput)
                    networkManager.SendData("CREATE_MAP " + mineCount + " " + (int)mapSize.X + " " + (int)mapSize.Y, 3);
            }
            else if (data.Contains("REQUEST_STARTFLAG"))
            {
                if (!waitingForInput)
                    networkManager.SendData("CAN_START", 3);
            }
            else if (data.Contains("FILL_ENEMY_MAP"))
            {
                int enemyCursorX = 0;
                int enemyCursorY = 0;

                data = data.Replace("FILL_ENEMY_MAP ", "");

                string[] tokens = data.Split();

                enemyCursorX = int.Parse(tokens[1]);
                enemyCursorY = int.Parse(tokens[2]);

                int mapInd = int.Parse(tokens[0]);

                for (int i = 3; i < mineCount * 2 + 1; i += 2)
                    mapEnemy[mapInd, int.Parse(tokens[i]), int.Parse(tokens[i + 1])].isMine = true;

                OpenEnemyCells(new Vector2(enemyCursorX, enemyCursorY), mapInd);
            }
            else if (data.Contains("OPEN_ENEMY_CELLS"))
            {
                string[] tokens = data.Split();
                OpenEnemyCells(new Vector2( int.Parse(tokens[2]), int.Parse(tokens[3])), int.Parse(tokens[1]));
            }
            else if (data.Contains("ENEMY_WON"))
                enemyWon = true;
            else if (data.Contains("ENEMY_LOST"))
                enemyLost = false;
        }

        private void CreateMap()
        {
            map = new Cell[(int)mapSize.X, (int)mapSize.Y];

            for (int i = 0; i < mapSize.X; i++)
                for (int j = 0; j < mapSize.Y; j++)
                    map[i, j] = new Cell();
        }

        private void CreateEnemyMaps()
        {
            for (int ind = 0; ind < 3; ind++)
            {
                mapEnemy = new Cell[ind, (int)mapSize.X, (int)mapSize.Y];

                for (int i = 0; i < mapSize.X; i++)
                    for (int j = 0; j < mapSize.Y; j++)
                        mapEnemy[ind, i, j] = new Cell();
            }
        }

        public void AbortRecieverThread()
        {
            return;
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

                Console.Write("\nRules: You and your teammate have to open your maps faster than your opennents\n");

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
    }
}
