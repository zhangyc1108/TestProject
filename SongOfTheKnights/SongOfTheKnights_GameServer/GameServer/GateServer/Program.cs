using GateServer.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Threading.Tasks;

namespace GateServer
{
    class Program
    {
        private static IClusterClient client;

        private static TcpServer tcpServer;

        static async Task Main(string[] args)
        {
            await ConnectClient();

            tcpServer = new TcpServer(client);

            await tcpServer.StartAsync();
        }

        /// <summary>
        /// 链接CardServer服务器
        /// </summary>
        /// <returns></returns>
        private static async Task<IClusterClient> ConnectClient()
        {
            client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "ClusterId";
                    options.ServiceId = "ServiceId";
                })
                .Build();

            await client.Connect();

            return client;
        }
    }
}