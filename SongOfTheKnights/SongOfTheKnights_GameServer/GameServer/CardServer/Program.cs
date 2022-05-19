using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CardServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await StartSilo();

            Console.WriteLine("开启CardServer！");

            Console.ReadLine();
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var host = new SiloHostBuilder()
                 .UseLocalhostClustering()
                 .Configure<ClusterOptions>(options =>
                 {
                     options.ClusterId = "ClusterId";
                     options.ServiceId = "ServiceId";
                 })
                 .ConfigureApplicationParts(parts =>
                 {
                     parts.AddFromApplicationBaseDirectory().WithReferences();
                 })
                 .Build();
            await host.StartAsync();//启动当前Silo.
            return host;
        }
    }
}