using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace AssetBundleBuilder {
    public class GlobalBuilderWindow : EditorWindow
    {
        [MenuItem(ABBUtility.Menu_GlobalBuildWindow)]
        static void BuildGlobalAssetBundles()
        {
            EditorWindow.GetWindow<GlobalBuilderWindow>("全局AssetBundle", true);
        }
        
        public string assetBundleName;
        public string buildpath = "";
        public BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        private SerializedProperty script;

        private const string Perfer_buildPath = "globalbuildPath";

        void OnEnable()
        {
            script = new SerializedObject(this).FindProperty("m_Script");
            if (EditorPrefs.HasKey(Perfer_buildPath))
            {
                buildpath = EditorPrefs.GetString(Perfer_buildPath);
            }
        }
        void OnGUI()
        {
            EditorGUILayout.PropertyField(script);
            EditorGUILayout.BeginHorizontal();
            buildpath = EditorGUILayout.TextField("ExportTo", buildpath);
            if (GUILayout.Button("选择路径"))
            {
                var path = EditorUtility.SaveFolderPanel("选择保存路径", buildpath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    buildpath = path;
                    EditorPrefs.SetString(Perfer_buildPath, buildpath);
                    this.Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", buildTarget);

         
                #region 全局打包
                buildOption = (BuildAssetBundleOptions)EditorGUILayout.EnumMaskField("Options", buildOption);
                if (GUILayout.Button("GlobleBulid"))
                {
                    ABBUtility.BuildGlobalAssetBundle(buildpath, buildOption, buildTarget);
                }
                #endregion
        }
    }
}
