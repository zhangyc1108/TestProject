using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XLua;
using static XLua.LuaEnv;

/// <summary>
/// 用来加载lua文件和proto文件的特殊方法 工具类
/// </summary>
public class LoaderHelper
{
    /// <summary>
    /// 初始化自定义Lua加载器
    /// </summary>
    public static void InitCustomLoaders()
    {
#if UNITY_EDITOR
        InitCustomLoaders_Editor();
#else
        //todo...
#endif
    }

    /// <summary>
    /// 编辑器模式下的自定义Lua加载器
    /// </summary>
    public static void InitCustomLoaders_Editor()
    {
        DirectoryInfo baseDir = new DirectoryInfo(Application.dataPath + "/GAssets");

        // 遍历所有模块

        DirectoryInfo[] Dirs = baseDir.GetDirectories();

        foreach (DirectoryInfo moduleDir in Dirs)
        {
            string moduleName = moduleDir.Name;

            CustomLoader Loader = (ref string scriptPath) =>
            {
                string assetPath = Application.dataPath + "/GAssets/" + moduleName + "/Src/" + scriptPath.Trim() + ".lua";

                byte[] result = File.ReadAllBytes(assetPath);

                return result;
            };

            Main.Instance.luaEnv.AddLoader(Loader);
        }
    }

    /// <summary>
    /// 加载PB文件
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="protoPath"></param>
    /// <returns></returns>
    public static string LoadProtoFile(string moduleName, string protoPath)
    {
#if UNITY_EDITOR
        return LoadProtoFile_Editor(moduleName, protoPath);
#else
        //todo...
#endif
    }

    /// <summary>
    /// 编辑器模式下加载PB文件
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="protoPath"></param>
    /// <returns></returns>
    public static string LoadProtoFile_Editor(string moduleName, string protoPath)
    {
        string assetPath = Application.dataPath + protoPath.Substring(6);

        string result = File.ReadAllText(assetPath);

        return result;
    }
}