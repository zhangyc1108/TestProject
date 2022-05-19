using DotNetty.Transport.Channels;
using IGrains;
using System;
using System.Collections.Generic;
using System.Text;

namespace GateServer.Net
{
    /// <summary>
    /// GateServer进程上的观察者对象，用来监听CardServer发来的消息
    /// </summary>
    public class PacketObserver : IPacketObserver
    {
        private readonly IChannelHandlerContext context;

        public PacketObserver(IChannelHandlerContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// 当GateServer收到来自CardServer的消息
        /// </summary>
        /// <param name="netPackage"></param>
        public void OnReceivePacket(NetPackage netPackage)
        {
            // 返回给客户端

            context.WriteAndFlushAsync(netPackage);
        }
    }
}