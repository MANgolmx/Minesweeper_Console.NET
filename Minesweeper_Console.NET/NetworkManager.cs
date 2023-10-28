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

        public bool readyToPlay = false;

        public NetworkManager() 
        { 
            client = new Client();
            server = new Server();
        }

        ~NetworkManager()
        {
            client.tcpClient.Close();
            server.tcpClient.Close();
        }

        public void SendData(string dataToSend)
        {
            if (client.tcpClient.Connected)
            {
                NetworkStream nwStream = client.tcpClient.GetStream();
                byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(dataToSend);
                nwStream.Write(bytesToSend, 0, bytesToSend.Length);
            }
        }

        public void StartReceivingData(MultiplayerGame session)
        {
            while (server.tcpClient.Connected)
            {
                try
                {
                    NetworkStream nwStream = server.tcpClient.GetStream();
                    byte[] buffer = new byte[server.tcpClient.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(buffer, 0, server.tcpClient.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    dataReceived = dataReceived.ToUpper();

                    if (dataReceived == "END")
                        session.AbortRecieverThread();
                    else session.HandleRecievedData(dataReceived);
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    session.AbortRecieverThread();
                }
            }
            Console.WriteLine("Player disconnected!");
            Console.ReadKey();
            session.AbortRecieverThread();
        }

    }
}
