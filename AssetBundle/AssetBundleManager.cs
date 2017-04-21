using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using AssetBundles;

public class AssetBundleManager : MonoBehaviour
{
    #region 单例
    protected static AssetBundleManager instance = default(AssetBundleManager);
    private static object lockHelper = new object();
    private static bool isQuit = false;
    public static AssetBundleManager GetInstance()
    {
        if (instance == null)
        {
            lock (lockHelper)
            {
                if (instance == null && !isQuit)
                {
                    GameObject go = new GameObject("AssetBundleManager");
                    instance = go.AddComponent<AssetBundleManager>();
                }
            }
        }
        return instance;
    }
    void OnApplicationQuit()
    {
        isQuit = true;
    }
    protected virtual void OnDestroy()
    {
        if (instance == this){
            instance = null;
        }
    }
    #endregion
#if UNITY_EDITOR
    //private static int m_SimulateAssetBundleInEditor;
    private static string kSimulateAssetBundles = "simulateinEditor";
    private ISimulationLoader simuationLoader = new SimulationLoader();
    // Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
    public static bool SimulateAssetBundleInEditor
    {
        get
        {
            return UnityEditor.EditorPrefs.GetBool(kSimulateAssetBundles, true);
        }
        set
        {
            UnityEditor.EditorPrefs.SetBool(kSimulateAssetBundles, value);
        }
    }
#endif
  
    private IUrlAssetBundleLoadCtrl activeLoader;
    private bool isDownLanding;
    private bool menuLoaded;
    private Queue<Tuple<string, string, UnityAction<UnityEngine.Object>>> m_LoadObjectQueue =
      new Queue<Tuple<string, string, UnityAction<UnityEngine.Object>>>();
    protected void Awake()
    {
        //资源加载
        UrlAssetBundleLoadCtrl.logMode = AssetBundles.UrlAssetBundleLoadCtrl.LogMode.JustErrors;
        activeLoader = new UrlAssetBundleLoadCtrl("file:///" + Application.streamingAssetsPath, "AssetBundle");
        if (instance == null){
            instance = GetComponent<AssetBundleManager>();
        }
    }
    void Update()
    {
        if (activeLoader != null)
        {
            activeLoader.UpdateDownLand();
            if (!isDownLanding)
            {
                if (m_LoadObjectQueue.Count > 0)
                {
                    Tuple<string, string, UnityAction<UnityEngine.Object>> data = m_LoadObjectQueue.Dequeue();
                    LoadAssetFromUrlAsync(data.Element1, data.Element2, data.Element3);
                }
            }
        }
    }
    /// <summary>
    /// 加载依赖关系
    /// </summary>
    /// <param name="onMenuLoad"></param>
    private void LoadMenu(UnityAction onMenuLoad)
    {
        if (menuLoaded)
        {
            onMenuLoad();
        }
        else
        {
            UnityAction newOnMenuLoad = () =>
            {
                menuLoaded = true;
                if(onMenuLoad != null) onMenuLoad.Invoke();
            };
            AssetBundleLoadOperation initopera = activeLoader.Initialize();
            StartCoroutine(WaitInalize(initopera, newOnMenuLoad));
        }
    }
    /// <summary>
    /// 从url异步加载一个资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetBundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="onAssetLoad"></param>
    public void LoadAssetFromUrlAsync<T>(string assetBundleName, string assetName, UnityAction<T> onAssetLoad) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (SimulateAssetBundleInEditor)
        {
            T asset = simuationLoader.LoadAsset<T>(assetBundleName, assetName);
            onAssetLoad(asset);
            return;
        }
#endif
        LoadMenu(() =>
            {
                if (isDownLanding)
                {
                    m_LoadObjectQueue.Enqueue(new Tuple<string, string, UnityAction<UnityEngine.Object>>(assetBundleName, assetName, (x) => onAssetLoad((T)x)));
                    return;
                }
                else
                {
                    isDownLanding = true;
                    onAssetLoad += (x) => {
                        activeLoader.UnloadAssetBundle(assetBundleName);
                        isDownLanding = false;
                    };
                    AssetBundleLoadAssetOperation operation = activeLoader.LoadAssetAsync(assetBundleName, assetName, typeof(T));
                    StartCoroutine(WaitLoadObject(operation, onAssetLoad));
                }
            });

    }
    /// <summary>
    /// 异步加载一组资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetBundleName"></param>
    /// <param name="onAssetsLoad"></param>
    public void LoadAssetsFromUrlAsync<T>(string assetBundleName, UnityAction<T[]> onAssetsLoad) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (SimulateAssetBundleInEditor)
        {
            T[] asset = simuationLoader.LoadAssets<T>(assetBundleName);
            onAssetsLoad(asset);
            return;
        }
#endif
        if (activeLoader != null)
        {
            LoadMenu(() =>
            {
                onAssetsLoad += (x) => { activeLoader.UnloadAssetBundle(assetBundleName); };
                AssetBundleLoadAssetsOperation operation = activeLoader.LoadAssetsAsync(assetBundleName, typeof(T));
                StartCoroutine(WaitLoadObjects(operation, onAssetsLoad));
            });
        }
        else
        {
            Debug.Log("Please Set Menu");
        }
    }
    /// <summary>
    /// 从url异步从bundle中加载一组资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetBundleName"></param>
    /// <param name="assetNames"></param>
    /// <param name="allAssetLoad"></param>
    public void LoadAssetsFromUrlAsync<T>(string assetBundleName, string[] assetNames, UnityAction<T[]> allAssetLoad) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (SimulateAssetBundleInEditor)
        {
            T[] asset = simuationLoader.LoadAssets<T>(assetBundleName, assetNames);
            allAssetLoad(asset);
            return;
        }
#endif
        if (activeLoader != null)
        {
            LoadMenu(() =>
            {
                T[] objectPool = new T[assetNames.Length];
                int j = 0;


                for (int i = 0; i < assetNames.Length; i++)
                {
                    int index = i;
                    UnityAction<T> loadOnce = (x) =>
                    {
                        objectPool[index] = x;
                        j++;
                        if (j == assetNames.Length)
                        {
                            allAssetLoad(objectPool);
                            activeLoader.UnloadAssetBundle(assetBundleName);
                        }
                    };
                    AssetBundleLoadAssetOperation operation = activeLoader.LoadAssetAsync(assetBundleName, assetNames[index], typeof(T));
                    StartCoroutine(WaitLoadObject(operation, loadOnce));
                }
            });
        }
        else
        {
            Debug.Log("Please Set Menu");
        }
    }
    /// <summary>
    /// 从url加载出场景
    /// </summary>
    /// <param name="assetBundleName"></param>
    /// <param name="assetName"></param>
    /// <param name="isAddictive"></param>
    /// <param name="onLevelLoad"></param>
    public void LoadLevelFromUrlAsync(string assetBundleName, string assetName, bool isAddictive, UnityAction<float> onProgressChange)
    {
#if UNITY_EDITOR
        if (SimulateAssetBundleInEditor)
        {
            simuationLoader.LoadSceneAsync(assetBundleName, assetName, isAddictive, onProgressChange);
            return;
        }
#endif
        if (activeLoader != null)
        {
            LoadMenu(() =>
            {
                AssetBundleLoadLevelOperation operation = activeLoader.LoadLevelAsync(assetBundleName, assetName, isAddictive);
                StartCoroutine(WaitLoadLevel(operation, onProgressChange));
            });
        }
        else
        {
            Debug.Log("Please Set Menu");
        }
    }


    IEnumerator WaitInalize(AssetBundleLoadOperation operation, UnityAction onActive)
    {
        yield return operation;
        if (onActive != null) onActive.Invoke();
    }
    IEnumerator WaitLoadObject<T>(AssetBundleLoadAssetOperation operation, UnityAction<T> onLoad) where T : UnityEngine.Object
    {
        yield return operation;
        if (onLoad != null)
        {
            T asset = operation.GetAsset<T>();
            if (asset == null){
                Debug.Log(operation.IsDone());
            }
            onLoad.Invoke(asset);
        }
    }
    IEnumerator WaitLoadObjects<T>(AssetBundleLoadAssetsOperation operation, UnityAction<T[]> onLoad) where T : UnityEngine.Object
    {
        yield return operation;
        if (onLoad != null)
        {
            T[] asset = operation.GetAssets<T>();
            onLoad.Invoke(asset);
        }
    }
    IEnumerator WaitLoadLevel(AssetBundleLoadLevelOperation operation, UnityAction<float> onProgressChanged)
    {
        while (!operation.IsDone())
        {
            if (operation.m_Request != null)
            {
                operation.m_Request.allowSceneActivation = false;
                if (onProgressChanged != null) onProgressChanged(operation.m_Request.progress);
                if (operation.m_Request.progress >= 0.9f)
                {
                    operation.m_Request.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }
    IEnumerator WaitLoadLevel(AsyncOperation operation, UnityAction<float> onProgressChanged)
    {
        while (!operation.isDone)
        {
            operation.allowSceneActivation = false;
            if (onProgressChanged != null) onProgressChanged(operation.progress);
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
