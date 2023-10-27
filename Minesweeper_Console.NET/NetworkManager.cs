using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class NetworkManager
    {

        public Client client;
        public Server server;

        public NetworkManager() 
        { 
            client = new Client();
            server = new Server();
        }

        public void SetClientIP(string roomCode)
        {
            IPAddress address;
            int port;

            try
            {
                int[] index = { int.Parse(roomCode[roomCode.Length - 1].ToString()),
                            int.Parse(roomCode[roomCode.Length - 2].ToString()),
                            int.Parse(roomCode[roomCode.Length - 3].ToString()),
                            int.Parse(roomCode[roomCode.Length - 4].ToString()) };

                string ipAddress = Convert.ToInt32(roomCode.Take(new Range(0, index[3])).ToString(), 16).ToString() + "." +
                    Convert.ToInt32(roomCode.Take(new Range(index[3], index[2])).ToString(), 16).ToString() + "." +
                    Convert.ToInt32(roomCode.Take(new Range(index[2], index[1])).ToString(), 16).ToString() + "." +
                    Convert.ToInt32(roomCode.Take(new Range(index[1], index[0])).ToString(), 16).ToString() + ".";

                address = IPAddress.Parse(ipAddress);

                port = Convert.ToInt32(roomCode.Take(new Range(roomCode.Length - 8, roomCode.Length - 4)).ToString(), 16);

                client.serverIP = address;
                client.port = port;
            }
            catch(Exception ex)
            {
                Console.WriteLine("\nSomething went wrong!\n" + ex.ToString());
            }
        }




    }
}
