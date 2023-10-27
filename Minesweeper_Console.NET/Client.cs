using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class Client
    {
        public IPAddress serverIP;
        public int port;

        private TcpClient client;
        private NetworkStream nwStream;


        public Client()
        {
            ;
        }




    }
}
