using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GateServer.Net
{
    /// <summary>
    /// 实现TcpServer的处理器
    /// </summary>
    public class TcpServerHandler : SimpleChannelInboundHandler<TcpMessage>
    {
        protected override void ChannelRead0(IChannelHandlerContext ctx, TcpMessage msg)
        {

        }

        public override void ChannelActive(IChannelHandlerContext context)
        {
            base.ChannelActive(context);
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            base.ExceptionCaught(context, exception);
        }
    }
}