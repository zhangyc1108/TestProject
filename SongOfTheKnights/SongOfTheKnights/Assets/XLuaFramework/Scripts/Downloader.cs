using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 下载器 工具类
/// </summary>
public class Downloader : Singleton<Downloader>
{
    /// <summary>
    /// 下载器唯一的对外API 下载指定模块
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <returns></returns>
    public async Task Download(ModuleConfig moduleConfig)
    {
        // 用来存放热更下来的资源的 本地路径

        string updatePath = GetUpdatePath(moduleConfig.moduleName);

        // 远程服务器上这个模块的AB资源配置文件的URL

        string configURL = GetServerURL(moduleConfig, moduleConfig.moduleName.ToLower() + ".json");

        UnityWebRequest request = UnityWebRequest.Get(configURL);

        request.downloadHandler = new DownloadHandlerFile(string.Format("{0}/{1}_temp.json", updatePath, moduleConfig.moduleName.ToLower()));

        Debug.Log("请求下载到本地路径: " + updatePath);

        await request.SendWebRequest();

        if (string.IsNullOrEmpty(request.error) == false)
        {
            Debug.LogWarning($"下载模块{moduleConfig.moduleName}的AB配置文件：{request.error}");

            bool result = await ShowMessageBox("网络异常，请检查网络后点击 继续下载", "继续下载", "退出游戏");

            if (result == false)
            {
                Application.Quit();

                return;
            }

            await Download(moduleConfig);

            return;
        }

        Tuple<List<BundleInfo>, BundleInfo[]> tuple = await GetDownloadList(moduleConfig.moduleName);

        List<BundleInfo> downloadList = tuple.Item1;

        BundleInfo[] removeList = tuple.Item2;

        long downloadSize = CalculateSize(downloadList);

        if (downloadSize == 0)
        {
            Clear(moduleConfig, removeList);

            return;
        }

        bool boxResult = await ShowMessageBox(moduleConfig, downloadSize);

        if (boxResult == false)
        {
            Application.Quit();

            return;
        }

        await ExecuteDownload(moduleConfig, downloadList);

        Clear(moduleConfig, removeList);

        return;
    }

    /// <summary>
    /// 模块热更新完成后的善后工作
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="removeList"></param>
    private void Clear(ModuleConfig moduleConfig, BundleInfo[] removeList)
    {
        string moduleName = moduleConfig.moduleName;

        string updatePath = GetUpdatePath(moduleName);

        // 删除不需要的本地bundle文件列表

        for (int i = removeList.Length - 1; i >= 0; i--)
        {
            BundleInfo bundleInfo = removeList[i];

            string filePath = string.Format("{0}/" + bundleInfo.bundle_name, updatePath);

            File.Delete(filePath);
        }

        // 删除旧的配置文件

        string oldFile = string.Format("{0}/{1}.json", updatePath, moduleName.ToLower());

        if (File.Exists(oldFile))
        {
            File.Delete(oldFile);
        }

        // 用新的配置文件替代

        string newFile = string.Format("{0}/{1}_temp.json", updatePath, moduleName.ToLower());

        File.Move(newFile, oldFile);
    }

    /// <summary>
    /// 计算需要下载的资源大小 单位是字节
    /// </summary>
    /// <param name="bundleList"></param>
    /// <returns></returns>
    private static long CalculateSize(List<BundleInfo> bundleList)
    {
        long totalSize = 0;

        foreach (BundleInfo bundleInfo in bundleList)
        {
            totalSize += bundleInfo.size;
        }

        return totalSize;
    }

    /// <summary>
    /// 新增给网络失败时的弹出对话框
    /// </summary>
    /// <param name="messageInfo"></param>
    /// <param name="first"></param>
    /// <param name="second"></param>
    /// <returns></returns>
    private static async Task<bool> ShowMessageBox(string messageInfo, string first, string second)
    {
        MessageBox messageBox = new MessageBox(messageInfo, first, second);

        MessageBox.BoxResult result = await messageBox.GetReplyAsync();

        messageBox.Close();

        if (result == MessageBox.BoxResult.First)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 弹出请求下载对话框
    /// </summary>
    /// <param name="bundleList"></param>
    private static async Task<bool> ShowMessageBox(ModuleConfig moduleConfig, long totalSize)
    {
        string downLoadSize = SizeToString(totalSize);

        string messageInfo = $"发现新版本，版本号为：{moduleConfig.moduleVersion}\n需要下载热更包，大小为：{downLoadSize}";

        MessageBox messageBox =  new MessageBox(messageInfo, "开始下载", "退出游戏");

        MessageBox.BoxResult result = await messageBox.GetReplyAsync();

        messageBox.Close();

        if (result == MessageBox.BoxResult.First)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 工具函数 把字节数转换成字符串形式
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    private static string SizeToString(long size)
    {
        string sizeStr = "";

        if (size >= 1024 * 1024)
        {
            long m = size / (1024 * 1024);

            size = size % (1024 * 1024);

            sizeStr += $"{m}[M]";
        }

        if (size >= 1024)
        {
            long k = size / 1024;

            size = size % 1024;

            sizeStr += $"{k}[K]";
        }

        long b = size;

        sizeStr += $"{b}[B]";

        return sizeStr;
    }

    /// <summary>
    /// 执行下载行为
    /// </summary>
    /// <param name="moduleConfig"></param>
    /// <param name="bundleList"></param>
    /// <returns></returns>
    private async Task ExecuteDownload(ModuleConfig moduleConfig, List<BundleInfo> bundleList)
    {
        while (bundleList.Count > 0)
        {
            BundleInfo bundleInfo = bundleList[0];

            UnityWebRequest request = UnityWebRequest.Get(GetServerURL(moduleConfig, bundleInfo.bundle_name));

            string updatePath = GetUpdatePath(moduleConfig.moduleName);

            request.downloadHandler = new DownloadHandlerFile(string.Format("{0}/" + bundleInfo.bundle_name, updatePath));

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("下载资源：" + bundleInfo.bundle_name + " 成功");

                bundleList.RemoveAt(0);
            }
            else
            {
                break;
            }
        }

        if (bundleList.Count > 0)
        {
            bool result = await ShowMessageBox("网络异常，请检查网络后点击 继续下载", "继续下载", "退出游戏");

            if (result == false)
            {
                Application.Quit();

                return;
            }

            await ExecuteDownload(moduleConfig, bundleList);

            return;
        }
    }

    /// <summary>
    /// 对于给定模块，返回其所有需要下载的BundleInfo组成的List和需要删除的本地Bundle数组
    /// </summary>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    private async Task<Tuple<List<BundleInfo>, BundleInfo[]>> GetDownloadList(string moduleName)
    {
        ModuleABConfig serverConfig = await AssetLoader.Instance.LoadAssetBundleConfig(BaseOrUpdate.Update, moduleName, moduleName.ToLower() + "_temp.json");

        if (serverConfig == null)
        {
            return null;
        }

        ModuleABConfig localConfig = await AssetLoader.Instance.LoadAssetBundleConfig(BaseOrUpdate.Update, moduleName, moduleName.ToLower() + ".json");

        // 注意：这里不用判断localConfig是否存在 本地的localConfig确实可能不存在，比如在此模块第一次热更新之前，本地update路径下啥都没有

        return CalculateDiff(moduleName, localConfig, serverConfig);
    }

    /// <summary>
    /// 通过两个AB资源配置文件，返回出有差异需要下载的Bundle列表和需要删除的本地Bundle数组
    /// </summary>
    /// <param name="moduleName"></param>
    /// <param name="localConfig"></param>
    /// <param name="serverConfig"></param>
    /// <returns></returns>
    private Tuple<List<BundleInfo>, BundleInfo[]> CalculateDiff(string moduleName, ModuleABConfig localConfig, ModuleABConfig serverConfig)
    {
        // 记录需要下载的bundle文件列表

        List<BundleInfo> bundleList = new List<BundleInfo>();

        // 记录需要删除的本地bundle文件列表

        Dictionary<string, BundleInfo> localBundleDic = new Dictionary<string, BundleInfo>();

        if (localConfig != null)
        {
            foreach (BundleInfo bundleInfo in localConfig.BundleArray.Values)
            {
                string uniqueId = string.Format("{0}|{1}", bundleInfo.bundle_name, bundleInfo.crc);

                localBundleDic.Add(uniqueId, bundleInfo);
            }
        }

        // 1. 找到那些差异的bundle文件，放到bundleList容器中
        // 2. 对于那些遗留在本地的无用的bundle文件，把它过滤在localBundleDic容器里

        foreach (BundleInfo bundleInfo in serverConfig.BundleArray.Values)
        {
            string uniqueId = string.Format("{0}|{1}", bundleInfo.bundle_name, bundleInfo.crc);

            if (localBundleDic.ContainsKey(uniqueId) == false)
            {
                bundleList.Add(bundleInfo);
            }
            else
            {
                localBundleDic.Remove(uniqueId);
            }
        }

        // 对于那些遗留在本地的无用的bundle文件，要清除，不然本地文件越积累越多

        BundleInfo[] removeList = localBundleDic.Values.ToArray();

        // 返回 需要下载的Bundle列表和需要删除的本地Bundle数组

        return new Tuple<List<BundleInfo>, BundleInfo[]>(bundleList, removeList);
    }

    /// <summary>
    /// 客户端给定模块的热更资源存放地址
    /// </summary>
    private string GetUpdatePath(string moduleName)
    {
        return Application.persistentDataPath + "/Bundles/" + moduleName;
    }

    /// <summary>
    /// 返回 给定模块的给定文件在服务器端的完整URL
    /// </summary>
    /// <param name="moduleConfig">模块配置对象</param>
    /// <param name="fileName">文件名字</param>
    /// <returns></returns>
    public string GetServerURL(ModuleConfig moduleConfig, string fileName)
    {
#if UNITY_ANDROID
        return string.Format("{0}/{1}/{2}", moduleConfig.DownloadURL, "Android", fileName);

#elif UNITY_IOS
        return string.Format("{0}/{1}/{2}", moduleConfig.DownloadURL, "iOS", fileName);

#elif UNITY_STANDALONE_WIN
        return string.Format("{0}/{1}/{2}", moduleConfig.DownloadURL, "StandaloneWindows64", fileName);

#endif
    }
}
