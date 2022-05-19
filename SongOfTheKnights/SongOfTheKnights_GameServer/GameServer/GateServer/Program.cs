using GateServer.Net;
using System;
using System.Threading.Tasks;

namespace GateServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await TcpServer.Start(8899);
        }
    }
}