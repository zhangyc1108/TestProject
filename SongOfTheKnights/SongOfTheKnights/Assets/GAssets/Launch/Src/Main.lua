Main = { }

function Main:Awake()
	-- 读取硬盘缓存数据
	require "Base/DataStoreMgr"
	DataStoreMgr_ReadAll()

	require "Base/NetworkMgr"
	NetworkMgr:ConnectServer()
end

function Main:Start()

end

function Main:OnDestroy()
	NetworkMgr:Destroy()
end

return Main