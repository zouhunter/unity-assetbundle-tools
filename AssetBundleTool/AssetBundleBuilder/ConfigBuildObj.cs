using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
namespace AssetBundleBuilder
{
    [CreateAssetMenu(menuName = "ScriptableObjects/ConfigBuildObj")]
    public class ConfigBuildObj : ScriptableObject
    {
        [System.Serializable]
        public class ObjectItem
        {
            public string assetPath;
            public string assetBundleName;//需要刷新时可刷新
            public Object obj;
            public ObjectItem(Object obj)
            {
                this.obj = obj;
            }
            public bool ReFelsh()
            {
                if (obj == null)
                {
                    Debug.LogError("assetPath :" + assetPath + "关联丢失");
                    return false;
                }
                else
                {
                    assetPath = AssetDatabase.GetAssetPath(obj);
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    assetBundleName = importer.assetBundleName;
                    return true;
                }
            }
        }
        public string ExportPath { get { return exportPath + "/" + menuName; } }
        public string exportPath;
        public string menuName;
        public BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool clearOld;
        public List<ObjectItem> needBuilds = new List<ObjectItem>();

        public AssetBundleBuild[] GetBundleBuilds()
        {
            Dictionary<string, AssetBundleBuild> bundleDic = new Dictionary<string, AssetBundleBuild>();
            foreach (var item in needBuilds)
            {
                if (!bundleDic.ContainsKey(item.assetBundleName))
                {
                    bundleDic.Add(item.assetBundleName, new AssetBundleBuild());
                }
                var asb = bundleDic[item.assetBundleName];

                asb.assetBundleName = item.assetBundleName;
                if (asb.assetNames == null) asb.assetNames = new string[0];
                List<string> assetNames = new List<string>(asb.assetNames);
                assetNames.Add(item.assetPath);
                asb.assetNames = assetNames.ToArray();

                bundleDic[item.assetBundleName] = asb;
            }

            List<AssetBundleBuild> builds = new List<AssetBundleBuild>(bundleDic.Values);
            return builds.ToArray();
        }

        public void ReFelsh()
        {
            var oldItems = needBuilds.ToArray();
            foreach (var item in oldItems)
            {
                if (!item.ReFelsh())
                {
                    needBuilds.Remove(item);
                    Debug.LogError("已经移除：" + item.assetPath);
                }
            }
        }
    }
}
#endif