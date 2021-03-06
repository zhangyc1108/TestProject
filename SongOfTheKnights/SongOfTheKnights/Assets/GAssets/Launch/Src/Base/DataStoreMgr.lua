---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by 14222.
--- DateTime: 2022/5/20 22:11
---

DataStoreMgr = {}

-- 数据持久存储，存储在硬盘

-- 全局函数
function DataStoreMgr_SaveAll()
    local rapidjson = require('rapidjson')
    local stringInfo = rapidjson.encode(DataStoreMgr)
    CS.UnityEngine.PlayerPrefs.SetString("DataStoreMgr", stringInfo)
end

-- 全局函数
function DataStoreMgr_ReadAll()
    local stringInfo = CS.UnityEngine.PlayerPrefs.GetString("DataStoreMgr")
    if stringInfo ~= nil and stringInfo ~= "" then
        print(stringInfo)
        local rapidjson = require('rapidjson')
        DataStoreMgr = rapidjson.decode(stringInfo)
    end
end

return DataStoreMgr