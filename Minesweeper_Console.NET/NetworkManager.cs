using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper_Console.NET
{
    class NetworkManager
    {
        public Client[] clients;
        public Server server;

        public bool readyToPlay = false;

        public NetworkManager(int clientCount = 1)
        {
            clients = new Client[clientCount];
            for (int i = 0; i < clientCount; i++)
                clients[i] = new Client();
            server = new Server();
        }

        ~NetworkManager()
        {
            foreach(var c in clients)
                c.tcpClient.Close();
            server.tcpClient.Close();
        }

        public void TryClientConnecting()
        {
            try
            {
                for (int i = 0; i < clients.Length; i++)
                    clients[i].tcpClient = new TcpClient(clients[i].serverIP.ToString(), clients[i].port);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nSomething went wrong!\n" + ex.ToString());
            }
        }

        public void SetClientHexIP(string hexip)
        {
            foreach(var c in clients)
                c.SetClientIP(hexip);
        }

        public void SendData(string dataToSend)
        {
            try
            {
                if (clients[0].tcpClient.Connected)
                {
                    NetworkStream nwStream = clients[0].tcpClient.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(dataToSend);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                }
            }  catch(Exception ex)
            {
                Console.Write("Something went wrong!\n" + ex.Message);
            }
        }

        public void SendData(string dataToSend, int clientCount)
        {
            try
            {
                foreach(var clnt in clients)
                if (clnt.tcpClient.Connected)
                {
                    NetworkStream nwStream = clnt.tcpClient.GetStream();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(dataToSend);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                }
            }
            catch (Exception ex)
            {
                Console.Write("Something went wrong!\n" + ex.Message);
            }
        }

        public void StartReceivingData(TeamGame session)
        {
            while (server.tcpClient.Connected)
            {
                try
                {
                    NetworkStream nwStream = server.tcpClient.GetStream();
                    byte[] buffer = new byte[server.tcpClient.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(buffer, 0, server.tcpClient.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (dataReceived.ToUpper() == "END")
                        session.AbortRecieverThread();
                    else session.HandleRecievedData(dataReceived);
                }
                catch (Exception ex)
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

        public void StartReceivingData(SurvivalGame session)
        {
            while (server.tcpClient.Connected)
            {
                try
                {
                    NetworkStream nwStream = server.tcpClient.GetStream();
                    byte[] buffer = new byte[server.tcpClient.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(buffer, 0, server.tcpClient.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (dataReceived.ToUpper() == "END")
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

        public void StartReceivingData(TrustGame session)
        {
            while (server.tcpClient.Connected)
            {
                try
                {
                    NetworkStream nwStream = server.tcpClient.GetStream();
                    byte[] buffer = new byte[server.tcpClient.ReceiveBufferSize];
                    int bytesRead = nwStream.Read(buffer, 0, server.tcpClient.ReceiveBufferSize);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (dataReceived == "END")
                        session.AbortRecieverThread();
                    else session.HandleRecievedData(dataReceived);
                }
                catch (Exception ex)
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
