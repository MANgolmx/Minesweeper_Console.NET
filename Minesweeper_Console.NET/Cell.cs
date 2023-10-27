using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class Cell
    {
        public bool isOpened;

        public bool isMine;
        public bool isFlagged;
        public bool isUndefined;

        public Cell()
        {
            isOpened = false;
            isMine = false;
            isFlagged = false;
            isUndefined = false;
        }

    }
}
