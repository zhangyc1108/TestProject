using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace GateServer.Net
{
    /// <summary>
    /// 实现TcpServer的解码器
    /// </summary>
    public class TcpServerDecoder : ByteToMessageDecoder
    {
        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {

        }
    }
}