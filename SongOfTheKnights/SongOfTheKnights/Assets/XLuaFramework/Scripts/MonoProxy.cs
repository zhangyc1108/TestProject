using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XLua;
using UnityEngine.Networking;
using UniRx;

/// <summary>
/// MonoBehaviour�Ĵ�����
/// </summary>
public class MonoProxy : MonoBehaviour
{
    /// <summary>
    /// ���MonoProxy�������󶨵�lua�ű�����
    /// </summary>
    public LuaTable luaTable;

    private Action<LuaTable> luaStart;

    private Action<LuaTable> luaOnDestroy;

    /// <summary>
    /// �󶨶�Ӧ�Ľű�
    /// </summary>
    /// <param name="scriptPath">�����lua�ű����ļ������·��</param>
    public LuaTable BindScript(string moduleName, string scriptPath)
    {
        Main.Instance.luaEnv.DoString("require '" + scriptPath + "'");

        luaTable = Main.Instance.luaEnv.Global.Get<LuaTable>(scriptPath);

        // �����luaTable�������һ���ֶ�ָ�����c#��MonoProxy�ű�����

        luaTable.Set("MonoProxy", this);

        // ��һ��Awake��������

        Action<LuaTable> luaAwake = luaTable.Get<Action<LuaTable>>("Awake");

        luaAwake?.Invoke(luaTable);

        // ��ȡlua�ű��ĳ�Ա����

        luaTable.Get("Start", out luaStart);

        luaTable.Get("OnDestroy", out luaOnDestroy);

        return luaTable;
    }

    /// <summary>
    /// ��MonoProxy��Ӧ��Lua�ű���һ��Update����������ʹ�ã�
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