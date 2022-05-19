using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace IGrains
{
    public interface IPacketObserver : IGrainObserver
    {
        /// <summary>
        /// 当GateServer收到来自CardServer的消息
        /// </summary>
        /// <param name="netPackage"></param>
        void OnReceivePacket(NetPackage netPackage);
    }
}