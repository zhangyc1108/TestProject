using IGrains;
using System;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Grains
{
    public class PacketRouterGrain : Orleans.Grain, IPacketRouterGrain
    {
        private IPacketObserver observer;

        /// <summary>
        /// 当CardServer收到来自GateServer的消息
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public Task OnReceivePacket(NetPackage netPackage)
        {
            // 当前Grain的key

            long id = GrainReference.GrainIdentity.PrimaryKeyLong;

            Console.WriteLine($"CardServer {id} 收到NetPackage");

            // 将消息再发回客户端

            if (netPackage.protoID == (int)LaunchPB.ProtoCode.EHero)
            {
                // 将包体字节流反序列化成PB对象

                IMessage message = new LaunchPB.Hero();

                LaunchPB.Hero hero = message.Descriptor.Parser.ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as LaunchPB.Hero;

                hero.Name = "Pizza 猫大哥";

                hero.Age = 28;

                // 将数据包再发回到网关服务器

                observer.OnReceivePacket(netPackage);

                Console.WriteLine($"CardServer {id} 发送NetPackage");
            }

            return Task.CompletedTask;
        }

        public Task BindPacketObserver(IPacketObserver observer)
        {
            this.observer = observer;

            return Task.CompletedTask;
        }
    }
}