---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by 14222.
--- DateTime: 2022/5/20 22:23
---

LoginUI = {}

-- Login的视图

function LoginUI:Awake()
    local startGameBtn = self.MonoProxy.transform:Find("StartGameBtn"):GetComponent(typeof(CS.UnityEngine.UI.Button))
    startGameBtn.onClick:AddListener(self.OnStartGameBtn)
end

-- 点击开始游戏
function LoginUI.OnStartGameBtn()
    LoginUI:CreatePopupLogin(function (go)
        go:AddComponent(typeof(CS.MonoProxy)):BindScript("Launch", "UI/Login/PopupLogin")
    end)
end

-- 创建登录弹出界面
function LoginUI:CreatePopupLogin(onComplete)
    local go = CS.AssetLoader.Instance:Clone("Launch", "Assets/GAssets/Launch/Res/UI/Login/Prefab/PopupLogin.prefab")
    self:DoMove(go, onComplete)
end

-- 创建注册弹出界面
function LoginUI:CreatePopupSingup(onComplete)
    local go = CS.AssetLoader.Instance:Clone("Launch", "Assets/GAssets/Launch/Res/UI/Login/Prefab/PopupSignup.prefab")
    self:DoMove(go, onComplete)
end

-- 用移动的方式规划登录/注册弹框的出场
function LoginUI:DoMove(go, onComplete)
    go.transform:SetParent(self.MonoProxy.transform)
    go.transform.localPosition = CS.UnityEngine.Vector3.zero
    go.transform.localScale = CS.UnityEngine.Vector3.one

    -- 查找Popup子物体
    local popup = go.transform:Find("Popup")
    popup.transform.localPosition = CS.UnityEngine.Vector3(0, 1200, 0)

    -- 使用dotween移动
    local tween = popup.transform:DOLocalMove(CS.UnityEngine.Vector3.zero, 0.4)

    -- 移动完成的回调
    tween.onComplete = function ()
        onComplete(go)
    end
end

function LoginUI:Start()
end

function LoginUI:OnDestroy()
end

return LoginUI