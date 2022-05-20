using Orleans;
using System;
using System.Threading.Tasks;

namespace IGrains
{
    public interface IPacketRouterGrain : IGrainWithStringKey
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

        /// <summary>
        /// 网关通知：当前Grain对应的玩家上线了
        /// </summary>
        /// <returns></returns>
        Task OnLine();

        /// <summary>
        /// 网关通知：当前Grain对应的玩家掉线了
        /// </summary>
        /// <returns></returns>
        Task OffLine();
    }
}