using IGrains;
using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Common;

namespace IGrains
{
    public class PacketRouterGrain : Orleans.Grain, IPacketRouterGrain
    {
        private IPacketObserver observer;

        /// <summary>
        /// 记录此Grain对应的玩家是否在线
        /// </summary>
        public bool onLine { get; set; }

        /// <summary>
        /// 当CardServer收到来自GateServer的消息
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public Task OnReceivePacket(NetPackage netPackage)
        {
            // 测试协议

            if (netPackage.protoID == (int)LaunchPB.ProtoCode.EHero)
            {
                // 将包体字节流反序列化成PB对象

                IMessage message = new LaunchPB.Hero();

                LaunchPB.Hero hero = message.Descriptor.Parser.ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as LaunchPB.Hero;

                hero.Name = "Pizza 猫大哥";

                hero.Age = 28;

                // 将数据包再发回到网关服务器

                observer.OnReceivePacket(netPackage);
            }

            return Task.CompletedTask;
        }

        public Task BindPacketObserver(IPacketObserver observer)
        {
            this.observer = observer;

            return Task.CompletedTask;
        }

        public Task OffLine()
        {
            onLine = false;

            string account = GrainReference.GrainIdentity.PrimaryKeyString;

            Logger.Instance.Information($"{account} 下线");

            return Task.CompletedTask;
        }

        public Task OnLine()
        {
            onLine = true;

            string account = GrainReference.GrainIdentity.PrimaryKeyString;

            Logger.Instance.Information($"{account} 上线");

            return Task.CompletedTask;
        }
    }
}