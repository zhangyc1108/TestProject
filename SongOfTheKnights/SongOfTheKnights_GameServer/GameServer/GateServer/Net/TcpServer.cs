using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    public static class TcpServer
    {
        /// <summary>
        /// 启动TcpServer
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Task Start(int port)
        {
            return RunServerAsync(port);
        }

        /// <summary>
        /// 关闭TcpServer
        /// </summary>
        /// <returns></returns>
        public static async Task Stop()
        {
            await Task.WhenAll(
                bootstrapChannel.CloseAsync(),
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            );

            Console.WriteLine("关闭网关服务器成功！");
        }

        private static async Task RunServerAsync(int port)
        {
            bossGroup = new MultithreadEventLoopGroup(1);

            // 默认是 CPU核数 * 2

            workerGroup = new MultithreadEventLoopGroup();

            try
            {
                ServerBootstrap bootstrap = new ServerBootstrap();

                bootstrap.Group(bossGroup, workerGroup);

                bootstrap.Channel<TcpServerSocketChannel>();

                bootstrap
                    .Option(ChannelOption.SoBacklog, 65535)
                    .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                    .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                    .ChildOption(ChannelOption.SoKeepalive, true)
                    .ChildOption(ChannelOption.TcpNodelay, true)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast("IdleChecker", new IdleStateHandler(50, 50, 0));

                        pipeline.AddLast(new TcpServerEncoder(), new TcpServerDecoder(), new TcpServerHandler());
                    }));

                bootstrapChannel = await bootstrap.BindAsync(port);

                Console.WriteLine($"启动网关服务器成功！监听端口号：{port}");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                throw new Exception("启动 TcpServer 失败! \n" + e.StackTrace);
            }
        }

        private static IEventLoopGroup bossGroup;

        private static IEventLoopGroup workerGroup;

        private static IChannel bootstrapChannel;
    }
}