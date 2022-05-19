using Orleans;
using System;
using System.Threading.Tasks;

namespace IGrains
{
    public interface IPacketRouterGrain : IGrainWithIntegerKey
    {
        /// <summary>
        /// 当CardServer收到来自GateServer的消息
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        Task OnReceivePacket(NetPackage netPackage);

        /// <summary>
        /// 给CardServer绑定一个观察者observer，方便CardServer给GateServer推送消息
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        Task BindPacketObserver(IPacketObserver observer);
    }
}