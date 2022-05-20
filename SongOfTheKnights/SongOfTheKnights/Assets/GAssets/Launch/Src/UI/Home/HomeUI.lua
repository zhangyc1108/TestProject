HomeUI = {}

-- Home的视图

function HomeUI:Awake()
    local button_Play = self.MonoProxy.transform:Find("Home/Bottom/Button_Play"):GetComponent(typeof(CS.UnityEngine.UI.Button))
    button_Play.onClick:AddListener(self.OnPlay,self)
end

-- 当玩家点击冒险按钮
function HomeUI:OnPlay()
     -- 先销毁主界面的2d资源和ui资源
     require "HomeMgr"
     HomeMgr:Unload()
     -- 进入战斗界面
end

function HomeUI:Start()
end

function HomeUI:OnDestroy()
end

return HomeUI