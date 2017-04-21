using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {
    public string assetname;
    public string bundlename;
    AssetBundleManager manager;
    private void Start()
    {
        manager = AssetBundleManager.GetInstance();
    }
    void OnGUI()
    {
        if (GUILayout.Button("加载"))
        {
            manager.LoadAssetFromUrlAsync<GameObject>(bundlename, assetname, (x) =>
            {
                Instantiate(x);
            });
        }
    }
}
