using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
namespace AssetBundle
{
    public class AssetBundlesMenu : Editor
    {
        [MenuItem("Assets/AssetBundle/BuildSelect")]
        static void BuildSingleAssetBundle()
        {
            BuilderWindow window = EditorWindow.GetWindow<BuilderWindow>("局部AssetBundle", true);
            window.IsSingle = true;
        }
        [MenuItem("Assets/AssetBundle/BuildGlobalAssetBundles")]
        static void BuildGlobalAssetBundles()
        {
            BuilderWindow window = EditorWindow.GetWindow<BuilderWindow>("全局AssetBundle", true);
           window.IsSingle = false;
        }
        [MenuItem("Assets/AssetBundle/GenMd5 of Files")]
        static void GenStreamingAssetFile()
        {
            //MD5TableParse.GetMD5CSV(AppFixed.NetWorks.localDir, AppFixed.CSVTable.MD5CSVINSIDE);
        }
        [MenuItem("Assets/AssetBundle/BundleName")]
        static void OpenAssetBundleNameConfigWidow()
        {
            BundleNameWindow window = EditorWindow.GetWindow<BundleNameWindow>("assetBundle配制", true);
            window.position = new Rect(400, 300, 700, 500);
            window.Show();
        }
        [MenuItem("Assets/AssetBundle/ArtBundleSetter")]
        static void OpenArtBundleSetterWidow()
        {
            ArtBundles window = EditorWindow.GetWindow<ArtBundles>("assetBundle批量设置", true);
            window.position = new Rect(400, 300, 600, 400);
            window.Show();
        }
        [MenuItem("Assets/AssetBundle/Clear Cache")]
        static void ClearGameCache()
        {
            Caching.CleanCache();
        }
    }

}
