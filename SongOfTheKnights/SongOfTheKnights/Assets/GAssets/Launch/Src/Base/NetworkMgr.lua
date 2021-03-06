---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by 14222.
--- DateTime: 2022/5/20 22:13
---

NetworkMgr = {}

NetworkMgr.Proto2Func = { }

function NetworkMgr:ConnectServer()
    self.tcpHelper = require "TcpHelper"

    self.tcpHelper:TCPConnect(
            self.OnTcpPacket,
            self.OnConnectSuccess,
            self.OnConnectFailed,
            self.OnConnectDisconnect
    )

    require "Main"

    Main.MonoProxy:BindUpdate(function()
        NetworkMgr.tcpHelper:TCPUpdate()
    end)

    self.pbHelper = require "PBHelper"

    self.pbHelper:LoadPBFiles(Main.MonoProxy.gameObject)
end

-- 给网络协议注册响应函数
function NetworkMgr:Register(protoCode, func)
    local protoEnum = self.pbHelper:Enum("LaunchPB.ProtoCode", protoCode)
    self.Proto2Func[protoEnum] = func
end

-- 取消网络协议对应的响应函数
function NetworkMgr:UnRegister(protoCode)
    local protoEnum = self.pbHelper:Enum("LaunchPB.ProtoCode", protoCode)
    self.Proto2Func[protoEnum] = nil
end

function NetworkMgr:Destroy()
    self.tcpHelper:TCPDestroy()
end

-- 当收到网络包
function NetworkMgr.OnTcpPacket(packet)
    print("收到数据包 packet.protoCode = " .. packet.protoCode)
    local func = NetworkMgr.Proto2Func[packet.protoCode]
    if func ~= nil then
        func(packet.PacketBodyBytes)
    else
        print("协议号packet.protoCode = " .. packet.protoCode .. "没有响应函数")
    end
end

-- 当链接成功时
function NetworkMgr.OnConnectSuccess()
    print("OnConnectSuccess")

    -- 加载登录界面
    require "UI/Login/LoginMgr"
    LoginMgr:Load()
end

-- 当链接失败时
function NetworkMgr.OnConnectFailed()
    print("OnConnectFailed")
end

-- 当链接断开时
function NetworkMgr.OnConnectDisconnect()
    print("OnConnectDisconnect")
end

return NetworkMgr