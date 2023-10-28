using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Minesweeper_Console.NET
{
    class Server
    {
        private IPAddress serverIP;
        private int port;

        private TcpListener listener;

        public TcpClient tcpClient;

        public Server()
        {
            SetServerIP();
            SetTCPListener();
        }

        public string GetHexIPAddress()
        {
            string res;
            string[] ip = serverIP.ToString().Split('.');
            
            Console.WriteLine(serverIP.ToString());
            Console.ReadKey();
            res = int.Parse(ip[0]).ToString("X") + int.Parse(ip[1]).ToString("X") + int.Parse(ip[2]).ToString("X") + 
                int.Parse(ip[3]).ToString("X") + port.ToString("X") + int.Parse(ip[0]).ToString("X").Count() +
                int.Parse(ip[1]).ToString("X").Count() + int.Parse(ip[2]).ToString("X").Count() + int.Parse(ip[3]).ToString("X").Count();

            return res;
        }

        public void StartListening()
        {
            listener.Start();

            tcpClient = listener.AcceptTcpClient();
        }

        private void SetTCPListener()
        {
            listener = new TcpListener(serverIP, port);
        }

        private void SetServerIP()
        {
            try
            {
                serverIP = GetIPAddress();
                port = GetPort();
            }
            catch(Exception ex)
            {
                Console.WriteLine("\nSomething went wrong!\n" + ex.Message);
            }
        }

        private IPAddress GetIPAddress()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addr = ipEntry.AddressList;
            foreach (var address in addr)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork && address.ToString().Contains("192."))
                    return address;
                
            }
            return IPAddress.Parse("0.0.0.0");
        }

        private int GetPort()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            for (int ixxxx = 1; ixxxx < 7; ixxxx++)
                for (int xxxxi = 1; xxxxi < 10; xxxxi++)
                {
                    int port = Int32.Parse(ixxxx.ToString() + "303" + xxxxi.ToString());
                    bool isAvailable = true;

                    foreach (IPEndPoint endPoint in ipEndPoints)
                    {
                        if (endPoint.Port == port)
                        {
                            isAvailable = true;
                            break;
                        }
                    }

                    if (isAvailable)
                        return port;
                }

            return 0;
        }

    }
}
