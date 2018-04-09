using System;
using System.Net;
using Socks;

namespace Driver
{
    class Program
    {
        static void Main(string[] args)
        {
            SocksServer socksServer = new SocksServer(IPAddress.Parse("127.0.0.1"), 5080);
            socksServer.Start();

            Console.ReadLine();
        }
    }
}
