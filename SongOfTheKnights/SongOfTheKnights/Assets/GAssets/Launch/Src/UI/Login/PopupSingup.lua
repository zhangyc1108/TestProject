---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by 14222.
--- DateTime: 2022/5/20 22:28
---

PopupSingup = {}

function PopupSingup:Awake()
    -- 处理关闭按钮
    local CloseBtn = self.MonoProxy.transform:Find("Popup/Button_Close"):GetComponent(typeof(CS.UnityEngine.UI.Button))
    CloseBtn.onClick:AddListener(self.OnCloseBtn)

    -- 处理账号输入框
    self.AccountField = self.MonoProxy.transform:Find("Popup/AccountField"):GetComponent(typeof(CS.UnityEngine.UI.InputField))

    -- 处理密码输入框
    self.PasswordField = self.MonoProxy.transform:Find("Popup/PasswordField"):GetComponent(typeof(CS.UnityEngine.UI.InputField))

    -- 处理请求注册按钮
    local SignupBtn = self.MonoProxy.transform:Find("Popup/SignupBtn"):GetComponent(typeof(CS.UnityEngine.UI.Button))
    SignupBtn.onClick:AddListener(self.OnSignupBtn)
end

-- 当点击请求注册按钮
function PopupSingup.OnSignupBtn()
    local self = PopupSingup
    -- 取到账号名和密码
    local account = self.AccountField.text
    local password = self.PasswordField.text
    if account ~= nil and account ~= "" and password ~= nil and password ~= "" then
        require "UI/Login/LoginMgr"
        LoginMgr:Register(account, password)
    end
end

-- 当关闭注册弹框界面
function PopupSingup.OnCloseBtn()
    local self = PopupSingup
    -- 查找子物体
    local popup = self.MonoProxy.transform:Find("Popup")

    -- 使用dotween移动
    local tween = popup.transform:DOLocalMove(CS.UnityEngine.Vector3(0, 1200, 0), 0.4)

    -- 移动完成的回调
    tween.onComplete = function ()
        CS.UnityEngine.Object.Destroy(self.MonoProxy.gameObject)
    end
end

function PopupSingup:Start()
end

function PopupSingup:OnDestroy()
end

return PopupSingup