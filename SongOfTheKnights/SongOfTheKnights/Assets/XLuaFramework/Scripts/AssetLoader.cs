using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 模块资源加载器
/// </summary>
public class AssetLoader : Singleton<AssetLoader>
{
    /// <summary>
    /// 克隆一个GameObject对象
    /// </summary>
    /// <param name="moduleName">模块的名字</param>
    /// <param name="path"></param>
    /// <returns></returns>
    public GameObject Clone(string moduleName, string path)
    {
        AssetRef assetRef = LoadAssetRef<GameObject>(moduleName, path);

        if (assetRef == null || assetRef.asset == null)
        {
            return null;
        }

        GameObject gameObject = UnityEngine.Object.Instantiate(assetRef.asset) as GameObject;

        if (assetRef.children == null)
        {
            assetRef.children = new List<GameObject>();
        }

        assetRef.children.Add(gameObject);

        return gameObject;
    }

    /// <summary>
    /// 创建资源对象，并且将其赋予游戏对象gameObject
    /// </summary>
    /// <typeparam name="T">资源的类型</typeparam>
    /// <param name="moduleName">模块的名字</param>
    /// <param name="assetPath">资源的路径</param>
    /// <param name="gameObject">资源加载后，要挂载到的游戏对象</param>
    /// <returns></returns>
    public T CreateAsset<T>(string moduleName, string assetPath, GameObject gameObject) where T : UnityEngine.Object
    {
        if (typeof(T) == typeof(GameObject) || (!string.IsNullOrEmpty(assetPath) && assetPath.EndsWith(".prefab")))
        {
            Debug.LogError("不可以加载GameObject类型，请直接使用AssetLoader.Instance.Clone接口，path：" + assetPath);

            return null;
        }

        if (gameObject == null)
        {
            Debug.LogError("CreateAsset必须传递一个gameObject其将要被挂载的GameObject对象！");

            return null;
        }

        AssetRef assetRef = LoadAssetRef<T>(moduleName, assetPath);

        if (assetRef == null || assetRef.asset == null)
        {
            return null;
        }

        if (assetRef.children == null)
        {
            assetRef.children = new List<GameObject>();
        }

        assetRef.children.Add(gameObject);

        return assetRef.asset as T;
    }

    /// <summary>
    /// 全局卸载函数
    /// </summary>
    public void Unload(Dictionary<string, Hashtable> module2Assets)
    {
        foreach (string moduleName in module2Assets.Keys)
        {
            Hashtable Path2AssetRef = module2Assets[moduleName];

            if (Path2AssetRef == null)
            {
                continue;
            }

            foreach (AssetRef assetRef in Path2AssetRef.Values)
            {
                if (assetRef.children == null || assetRef.children.Count == 0)
                {
                    continue;
                }

                for (int i = assetRef.children.Count - 1; i >= 0; i--)
                {
                    GameObject go = assetRef.children[i];

                    if (go == null)
                    {
                        assetRef.children.RemoveAt(i);
                    }
                }

                // 如果这个资源assetRef已经没有被任何GameObject所依赖了，那么此assetRef就可以卸载了

                if (assetRef.children.Count == 0)
                {
                    assetRef.asset = null;

                    Resources.UnloadUnusedAssets();

                    // 对于assetRef所属的这个bundle，解除关系

                    assetRef.bundleRef.children.Remove(assetRef);

                    if (assetRef.bundleRef.children.Count == 0)
                    {
                        assetRef.bundleRef.bundle.Unload(true);
                    }

                    // 对于assetRef所依赖的那些bundle列表，解除关系

                    foreach (BundleRef bundleRef in assetRef.dependencies)
                    {
                        bundleRef.children.Remove(assetRef);

                        if (bundleRef.children.Count == 0)
                        {
                            bundleRef.bundle.Unload(true);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加载 AssetRef 对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">资源的相对路径</param>
    /// <returns></returns>
    private AssetRef LoadAssetRef<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (GlobalConfig.BundleMode == false)
        {
            return LoadAssetRef_Editor<T>(moduleName, assetPath);
        }
        else
        {
            return LoadAssetRef_Runtime<T>(moduleName, assetPath);
        }
#else
        return LoadAssetRef_Runtime<T>(moduleName, assetPath);
#endif
    }

    /// <summary>
    /// 在编辑器模式下加载 AssetRef 对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">资源的相对路径</param>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Editor<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        AssetRef assetRef = new AssetRef(null);

        assetRef.asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        return assetRef;
#else
        return null;
#endif
    }

    /// <summary>
    /// 在AB包模式下加载 AssetRef 对象
    /// </summary>
    /// <typeparam name="T">要加载的资源类型</typeparam>
    /// <param name="moduleName">模块名字</param>
    /// <param name="assetPath">资源的相对路径</param>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Runtime<T>(string moduleName, string assetPath) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        Hashtable module2AssetRef;

        if (GlobalConfig.HotUpdate == true)
        {
            module2AssetRef = update2Assets[moduleName];
        }
        else
        {
            module2AssetRef = base2Assets[moduleName];
        }

        AssetRef assetRef = (AssetRef)module2AssetRef[assetPath];

        if (assetRef == null)
        {
            Debug.LogError("未找到资源：moduleName " + moduleName + " path " + assetPath);

            return null;
        }

        if (assetRef.asset != null)
        {
            return assetRef;
        }

        // 1. 处理assetRef依赖的BundleRef列表

        foreach (BundleRef oneBundleRef in assetRef.dependencies)
        {
            if (oneBundleRef.bundle == null)
            {
                string bundlePath = BundlePath(oneBundleRef.witch, moduleName, oneBundleRef.bundleInfo.bundle_name);

                oneBundleRef.bundle = AssetBundle.LoadFromFile(bundlePath);
            }

            if (oneBundleRef.children == null)
            {
                oneBundleRef.children = new List<AssetRef>();
            }

            oneBundleRef.children.Add(assetRef);
        }

        // 2. 处理assetRef属于的那个BundleRef对象

        BundleRef bundleRef = assetRef.bundleRef;

        if (bundleRef.bundle == null)
        {
            bundleRef.bundle = AssetBundle.LoadFromFile(BundlePath(bundleRef.witch, moduleName, bundleRef.bundleInfo.bundle_name));
        }

        if (bundleRef.children == null)
        {
            bundleRef.children = new List<AssetRef>();
        }

        bundleRef.children.Add(assetRef);

        // 3. 从bundle中提取asset

        assetRef.asset = assetRef.bundleRef.bundle.LoadAsset<T>(assetRef.assetInfo.asset_path);

        if (typeof(T) == typeof(GameObject) && assetRef.assetInfo.asset_path.EndsWith(".prefab"))
        {
            assetRef.isGameObject = true;
        }
        else
        {
            assetRef.isGameObject = false;
        }

        return assetRef;
    }

    /// <summary>
    /// 工具函数 根据模块名字和bundle名字，返回其实际资源路径
    /// </summary>
    /// <param name="baseOrUpdate"></param>
    /// <param name="moduleName"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private string BundlePath(BaseOrUpdate baseOrUpdate, string moduleName, string bundleName)
    {
        if (baseOrUpdate == BaseOrUpdate.Update)
        {
            return Application.persistentDataPath + "/Bundles/" + moduleName + "/" + bundleName;
        }
        else
        {
            return Application.streamingAssetsPath + "/" + moduleName + "/" + bundleName;
        }
    }

    /// <summary>
    /// 平台对应的只读路径下的资源
    /// Key 模块名字
    /// Value 模块所有的资源
    /// </summary>
    public Dictionary<string, Hashtable> base2Assets;

    /// <summary>
    /// 平台对应的可读可写路径
    /// </summary>
    public Dictionary<string, Hashtable> update2Assets;

    /// <summary>
    /// 记录所有的BundleRef对象(不管是Base路径还是Update路径) 
    /// 键是bundle的名字
    /// </summary>
    public Dictionary<string, BundleRef> name2BundleRef;

    /// <summary>
    /// 模块资源加载器的构造函数
    /// </summary>
    public AssetLoader()
    {
        base2Assets = new Dictionary<string, Hashtable>();

        update2Assets = new Dictionary<string, Hashtable>();

        name2BundleRef = new Dictionary<string, BundleRef>();
    }

    /// <summary>
    /// 根据模块的json配置文件 创建 内存中的资源容器
    /// </summary>
    /// <param name="moduleABConfig"></param>
    /// <returns></returns>
    public Hashtable ConfigAssembly(ModuleABConfig moduleABConfig)
    {
        Hashtable Path2AssetRef = new Hashtable();

        for (int i = 0; i < moduleABConfig.AssetArray.Length; i++)
        {
            AssetInfo assetInfo = moduleABConfig.AssetArray[i];

            // 装配一个AssetRef对象

            AssetRef assetRef = new AssetRef(assetInfo);

            assetRef.bundleRef = name2BundleRef[assetInfo.bundle_name];

            int count = assetInfo.dependencies.Count;

            assetRef.dependencies = new BundleRef[count];

            for (int index = 0; index < count; index++)
            {
                string bundleName = assetInfo.dependencies[index];

                assetRef.dependencies[index] = name2BundleRef[bundleName];
            }

            // 装配好了放到Path2AssetRef容器中

            Path2AssetRef.Add(assetInfo.asset_path, assetRef);
        }

        return Path2AssetRef;
    }

    /// <summary>
    /// 加载模块的AB资源配置文件
    /// </summary>
    /// <param name="baseOrUpdate">只读路径还是可读可写路径</param>
    /// <param name="moduleName">模块名字</param>
    /// <param name="bundleConfigName">AB资源配置文件的名字</param>
    /// <returns></returns>
    public async Task<ModuleABConfig> LoadAssetBundleConfig(BaseOrUpdate baseOrUpdate, string moduleName, string bundleConfigName)
    {
        string url = BundlePath(baseOrUpdate, moduleName, bundleConfigName);

        UnityWebRequest request = UnityWebRequest.Get(url);

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == true)
        {
            return JsonMapper.ToObject<ModuleABConfig>(request.downloadHandler.text);
        }

        return null;
    }
}
