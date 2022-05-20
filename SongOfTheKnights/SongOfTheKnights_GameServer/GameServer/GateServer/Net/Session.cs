using Common;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using IGrains;
using LaunchPB;
using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GateServer.Net
{
    /// <summary>
    /// 网关服务器跟游戏客户端的一个链路Session对象
    /// </summary>
    public class Session
    {
        /// <summary>
        /// 代表这个链路的IChannelHandlerContext对象
        /// </summary>
        private IChannelHandlerContext context;

        /// <summary>
        /// 这个Session的观察者对象，Silo节点可以通过这个观察者对象向这个观察者对象发消息
        /// </summary>
        private PacketObserver packetObserver;

        /// <summary>
        /// 本进程(GateServer)向Silo节点发送数据包的目标Actor
        /// </summary>
        private IPacketRouterGrain routerGrain;

        /// <summary>
        /// 标记这个Session对象是否经过鉴权
        /// </summary>
        private bool isLogin;

        /// <summary>
        /// 这个网关服务器和Silo主机节点之间的链接
        /// </summary>
        private IClusterClient client;

        public Session(IClusterClient client, IChannelHandlerContext context)
        {
            this.client = client;

            this.context = context;

            isLogin = false;
        }

        /// <summary>
        /// 处理收到的来自游戏客户端的数据包
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public async Task DispatchReceivePacket(NetPackage netPackage)
        {
            try
            {
                // 未鉴权的Session对象 只能接收Register和Login这两类消息

                if (isLogin == false)
                {
                    ILoginGrain loginGrain = client.GetGrain<ILoginGrain>("SingleLoginGrain");

                    if (netPackage.protoID == (int)ProtoCode.ERegister)
                    {
                        NetPackage resultPackage = await loginGrain.OnRegister(netPackage);

                        await context.WriteAndFlushAsync(resultPackage);
                    }
                    else if (netPackage.protoID == (int)ProtoCode.ELogin)
                    {
                        NetPackage resultPackage = await loginGrain.OnLogin(netPackage);

                        await context.WriteAndFlushAsync(resultPackage);

                        await NotifyOnLine(resultPackage);

                    }
                    else
                    {
                        Logger.Instance.Information($"未被鉴权的Session发送协议: {netPackage.protoID} 忽略此协议！");
                    }
                }
                else
                {
                    await routerGrain.OnReceivePacket(netPackage);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 通知Silo主机节点 某个账号上线
        /// </summary>
        /// <param name="resultPackage"></param>
        /// <returns></returns>
        private async Task NotifyOnLine(NetPackage resultPackage)
        {
            IMessage message = new LoginResp();

            LoginResp loginResp = message.Descriptor.Parser.ParseFrom(resultPackage.bodyData, 0, resultPackage.bodyData.Length) as LoginResp;

            if (loginResp.Result == LoginResult.ELoginSuccess)
            {
                isLogin = true;

                routerGrain = client.GetGrain<IPacketRouterGrain>(loginResp.Account);

                packetObserver = new PacketObserver(context);

                IPacketObserver observerRef = await client.CreateObjectReference<IPacketObserver>(packetObserver);

                await routerGrain.BindPacketObserver(observerRef);

                await routerGrain.OnLine();
            }
        }

        /// <summary>
        /// 通知Silo主机节点 某个账号下线
        /// </summary>
        public void Disconnect()
        {
            if (routerGrain != null)
            {
                routerGrain.OffLine();
            }
        }
    }
}