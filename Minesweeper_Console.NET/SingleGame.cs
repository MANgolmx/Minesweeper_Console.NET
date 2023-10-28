using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Minesweeper_Console.NET
{
    class SingleGame
    {
        private Vector2 mapSize;
        private int mineCount;
        private Cell[,] map;

        private Vector2 cursorPosition;

        public SingleGame()
        {
            cursorPosition = new Vector2(0, 0);
        }

        private void ManageGame()
        {
            bool isPlaying = true;

            while (isPlaying)
            {
                Console.Clear();
                PrintMap();
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

        public void StartGame()
        {
            GetMapInfo();
            ChooseFirstInput();
        }

        private void ChooseFirstInput()
        {
            GenerateMap();

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

        private void PrintMap()
        {
            for (int i = 0; i < mapSize.X; i++) {
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
                            switch(CalculateAdjascentMines(new Vector2(i, j)))
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

        private void FillMap()
        {
            int generatedMines = 0;
            do
            {
                int posX = Random.Shared.Next((int)mapSize.X);
                if ((int)cursorPosition.X == posX)
                    continue;

                int posY = Random.Shared.Next((int)mapSize.Y);
                if ((int)cursorPosition.Y == posY)
                    continue;

                if (!map[posX, posY].isMine && (map[posX, posY].isMine = true))
                    generatedMines++;

            } while (generatedMines < mineCount);

            OpenCells(cursorPosition);
        }

        private int InputManager(bool firstInput = false)
        {
            switch(Console.ReadKey().Key)
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
                            OpenCells(cursorPosition);
                        else
                        {
                            map[(int)cursorPosition.X, (int)cursorPosition.Y].isOpened = true;
                            if (map[(int)cursorPosition.X, (int)cursorPosition.Y].isMine)
                                return 1;
                        }
                    }
                    break;
            }

            return 0;
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

        private void GenerateMap()
        {
            map = new Cell[(int)mapSize.X, (int)mapSize.Y];

            for (int i = 0; i < mapSize.X; i++)
                for (int j = 0; j < mapSize.Y; j++)
                    map[i, j] = new Cell();
        }

        private void GetMapInfo()
        {
            int x = 0;
            int y = 0;
            bool rightInputFlag = false;
            do {
                Console.Clear();
                Console.Write("Input map size: ");
                string tokens = Console.ReadLine();

                try
                {
                    int spaceCount = 0;
                    int index = 0;
                    foreach(var s in tokens)
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
                    else if (x > 100 && y > 100)
                    {
                        Console.WriteLine("\nMap must be smaller than 100x100");
                        Console.ReadKey();
                    }
                    else rightInputFlag = true;
                }
                catch(Exception ex)
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

                    if (mines < 7)
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

            } while(!rightInputFlag);

            mapSize.X = x;
            mapSize.Y = y;

            mineCount = mines;
        }
    }
}
