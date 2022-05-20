using Common;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using IGrains;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GateServer.Net
{
    /// <summary>
    /// 实现TcpServer的编码器
    /// </summary>
    public class TcpServerEncoder : MessageToByteEncoder<NetPackage>
    {
        protected override void Encode(IChannelHandlerContext context, NetPackage netPackage, IByteBuffer output)
        {
            output.WriteInt(netPackage.bodyData.Length);

            output.WriteInt(netPackage.protoID);

            output.WriteBytes(netPackage.bodyData);

            //Logger.Instance.Information($"{context.Channel.RemoteAddress.ToString()} 发送协议号 {netPackage.protoID} 数据！");
        }
    }
}