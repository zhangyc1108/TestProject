HomeMgr = {}

-- Home的控制器

-- 加载主界面的2d资源和ui
function HomeMgr:Load()
    local go = CS.AssetLoader.Instance:Clone("Launch", "Assets/GAssets/Launch/Res/UI/Home/Prefab/HomeUI.prefab")

    self:DoMove(go, function (go)
        go:AddComponent(typeof(CS.MonoProxy)):BindScript("Launch", "UI/Home/HomeUI")
    end)

    self.homeUI = go
end

-- 销毁主界面的2d资源和界面ui
function HomeMgr:Unload()
    CS.UnityEngine.Object.Destroy(self.homeUI)
end

-- 用移动的方式规划HomeUI的出场
function HomeMgr:DoMove(go, onComplete)
    -- 查找Top子物体
    local top = go.transform:Find("Home/Top")
    top:GetComponent(typeof(CS.UnityEngine.RectTransform)).offsetMax = CS.UnityEngine.Vector2(0, 200)
    -- 使用dotween移动
    local tweenTop = CS.DG.Tweening.DOTweenModuleUI.DOAnchorPos(top:GetComponent(typeof(CS.UnityEngine.RectTransform)), CS.UnityEngine.Vector2.zero, 0.5)

    -- 查找Bottom子物体
    local bottom = go.transform:Find("Home/Bottom")
    bottom:GetComponent(typeof(CS.UnityEngine.RectTransform)).offsetMin = CS.UnityEngine.Vector2(0, -200)
    local tweenBottom = CS.DG.Tweening.DOTweenModuleUI.DOAnchorPos(bottom:GetComponent(typeof(CS.UnityEngine.RectTransform)), CS.UnityEngine.Vector2.zero, 0.5)

    -- 查找Left子物体
    local left = go.transform:Find("Home/Left")
    left:GetComponent(typeof(CS.UnityEngine.RectTransform)).offsetMin = CS.UnityEngine.Vector2(-540, 0)
    local tweenLeft = CS.DG.Tweening.DOTweenModuleUI.DOAnchorPos(left:GetComponent(typeof(CS.UnityEngine.RectTransform)), CS.UnityEngine.Vector2.zero, 0.5)

    -- 查找Right子物体
    local right = go.transform:Find("Home/Right")
    right:GetComponent(typeof(CS.UnityEngine.RectTransform)).offsetMax = CS.UnityEngine.Vector2(270, 0)
    local tweenRight = CS.DG.Tweening.DOTweenModuleUI.DOAnchorPos(right:GetComponent(typeof(CS.UnityEngine.RectTransform)), CS.UnityEngine.Vector2.zero, 0.5)

    -- 移动完成的回调
    tweenRight.onComplete = function ()
        onComplete(go)
    end
end

return HomeMgr