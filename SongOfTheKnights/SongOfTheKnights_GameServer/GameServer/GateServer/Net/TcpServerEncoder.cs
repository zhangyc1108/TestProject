using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GateServer.Net
{
    /// <summary>
    /// 实现TcpServer的编码器
    /// </summary>
    public class TcpServerEncoder : MessageToByteEncoder<TcpMessage>
    {
        protected override void Encode(IChannelHandlerContext context, TcpMessage message, IByteBuffer output)
        {

        }
    }
}