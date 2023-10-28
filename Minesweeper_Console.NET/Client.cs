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

        public TcpClient tcpClient;

        public Client()
        {
            ;
        }

        public void TryConnecting()
        {
            try
            {
                tcpClient = new TcpClient(serverIP.ToString(), port);
            } catch(Exception ex)
            { 
            Console.WriteLine("\nSomething went wrong!\n" + ex.ToString());
            }
        }

        public void SetClientIP(string roomCode)
        {
            IPAddress address;
            int port;

            int[] index = { int.Parse(roomCode[roomCode.Length - 1].ToString()),
                            int.Parse(roomCode[roomCode.Length - 2].ToString()),
                            int.Parse(roomCode[roomCode.Length - 3].ToString()),
                            int.Parse(roomCode[roomCode.Length - 4].ToString()) };

            string ipAddress = Convert.ToInt32(new string(roomCode.Take(new Range(0, index[3])).ToArray()), 16).ToString() + "." +
                Convert.ToInt32(new string(roomCode.Take(new Range(index[3], index[3] + index[2])).ToArray()), 16).ToString() + "." +
                Convert.ToInt32(new string(roomCode.Take(new Range(index[3] + index[2], index[3] + index[2] + index[1])).ToArray()), 16).ToString() + "." +
                Convert.ToInt32(new string(roomCode.Take(new Range(index[3] + index[2] + index[1], index[3] + index[2] + index[1] + index[0])).ToArray()), 16).ToString();

            address = IPAddress.Parse(ipAddress);

            port = Convert.ToInt32(new string(roomCode.Take(new Range(roomCode.Length - 8, roomCode.Length - 4)).ToArray()), 16);

            serverIP = address;
            this.port = port;
        }
    }
}
