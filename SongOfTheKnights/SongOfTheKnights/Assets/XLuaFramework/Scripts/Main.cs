using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public async void Awake()
    {
        InitGlobal();

        // 启动模块
        ModuleConfig launchModule = new ModuleConfig()
        {
            moduleName = "Launch",
            moduleVersion = "20211008085930",
            moduleUrl = "http://192.168.0.7:8000"
        };

        bool result = await ModuleManager.Instance.Load(launchModule);

        if (result == true)
        {
            // 在这里 把代码控制权交给Lua 完毕！

            Debug.Log("Lua 代码开始...");

            AssetLoader.Instance.Clone("Launch", "Assets/GAssets/Launch/Sphere.prefab");

            GameObject pizzaCat = AssetLoader.Instance.Clone("Launch", "Assets/GAssets/Launch/PizzaCat.prefab");

            pizzaCat.GetComponent<SpriteRenderer>().sprite = 
                AssetLoader.Instance.CreateAsset<Sprite>("Launch", "Assets/GAssets/Launch/Sprite/header.jpg", pizzaCat);
        }
    }

    public void Update()
    {
        // 执行卸载策略

        AssetLoader.Instance.Unload(AssetLoader.Instance.base2Assets);
    }

    /// <summary>
    /// 初始化全局变量
    /// </summary>
    private void InitGlobal()
    {
        Instance = this;

        GlobalConfig.HotUpdate = true;

        GlobalConfig.BundleMode = true;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 主Mono对象
    /// </summary>
    public static Main Instance;
}
