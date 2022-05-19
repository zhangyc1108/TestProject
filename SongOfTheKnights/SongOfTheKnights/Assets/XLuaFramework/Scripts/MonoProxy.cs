using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using UnityEngine.Networking;
using UniRx;

/// <summary>
/// MonoBehaviour的代理类
/// </summary>
public class MonoProxy : MonoBehaviour
{
    /// <summary>
    /// 这个MonoProxy对象所绑定的lua脚本对象
    /// </summary>
    public LuaTable luaTable;

    private Action<LuaTable> luaStart;

    private Action<LuaTable> luaOnDestroy;

    /// <summary>
    /// 绑定对应的脚本
    /// </summary>
    /// <param name="scriptPath">输入的lua脚本的文件的相对路径</param>
    public LuaTable BindScript(string moduleName, string scriptPath)
    {
        Main.Instance.luaEnv.DoString("require '" + scriptPath + "'");

        luaTable = Main.Instance.luaEnv.Global.Get<LuaTable>(scriptPath);

        // 给这个luaTable对象添加一个字段指向这个c#的MonoProxy脚本对象

        luaTable.Set("MonoProxy", this);

        // 补一个Awake方法调用

        Action<LuaTable> luaAwake = luaTable.Get<Action<LuaTable>>("Awake");

        luaAwake?.Invoke(luaTable);

        // 获取lua脚本的成员方法

        luaTable.Get("Start", out luaStart);

        luaTable.Get("OnDestroy", out luaOnDestroy);

        return luaTable;
    }

    /// <summary>
    /// 给MonoProxy对应的Lua脚本绑定一个Update方法！按需使用！
    /// </summary>
    /// <param name="action"></param>
    public void BindUpdate(Action action)
    {
        Observable.EveryUpdate().Subscribe(_ => { action(); }).AddTo(this);
    }

    void Start()
    {
        luaStart?.Invoke(luaTable);
    }

    void OnDestroy()
    {
        luaOnDestroy?.Invoke(luaTable);
    }
}