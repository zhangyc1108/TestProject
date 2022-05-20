using Common;
using GateServer.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Common.DB;
using Common.ORM;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GateServer
{
    class Program
    {
        private static IClusterClient client;

        private static TcpServer tcpServer;

        static async Task Main(string[] args)
        {
            Logger.Create("GateServer");

            await ConnectClient();

            Logger.Instance.Information("网关服务器(GateServer)链接游戏服务器(CardServer)成功!");

            tcpServer = new TcpServer(client);

            await tcpServer.StartAsync();
        }

        /// <summary>
        /// 网关服务器(GateServer)链接游戏服务器(CardServer)
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