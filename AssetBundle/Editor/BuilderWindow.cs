using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace AssetBundle {
    public class BuilderWindow : EditorWindow
    {
        public string assetBundleName;
        public string buildpath = "./Assets/StreamingAssets/AssetBundle";
        public BuildAssetBundleOptions buildOption = BuildAssetBundleOptions.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public bool genFile = false;
        private IBulidCtrl buildCtrl;
        
        public bool IsSingle { get; set; }
        private Object[] selectionGameObject;
        private Object[] selectionMaterial;
        private Object[] selectionTexture;

        void OnEnable()
        {
            buildCtrl = new BulidController();
            SelectionChanged();
            Selection.selectionChanged += SelectionChanged;
        }


        void OnGUI()
        {
            if (IsSingle)
            {
                #region 打包选中
                assetBundleName = EditorGUILayout.TextField("AbName", assetBundleName);
                EditorGUILayout.BeginHorizontal();
                buildpath = EditorGUILayout.TextField("ExportTo", buildpath);
                if (GUILayout.Button("选择路径"))
                {
                    buildpath = EditorUtility.SaveFolderPanel("选择保存路径", Application.streamingAssetsPath,"");
                }
                EditorGUILayout.EndHorizontal();

                buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", buildTarget);

                if (GUILayout.Button("BuildGameObjects"))
                {
                    buildCtrl.BuildSelectAssets(assetBundleName,buildpath, selectionGameObject, buildTarget);
                }
                else if (GUILayout.Button("BuildMaterials"))
                {
                    buildCtrl.BuildSelectAssets(assetBundleName, buildpath, selectionMaterial, buildTarget);
                }
                else if (GUILayout.Button("BuildTextures"))
                {
                    buildCtrl.BuildSelectAssets(assetBundleName, buildpath, selectionTexture, buildTarget);
                }
                #endregion
            }
            else
            {
                #region 全局打包
                buildpath = EditorGUILayout.TextField("ExportTo", buildpath);
                buildOption = (BuildAssetBundleOptions)EditorGUILayout.EnumMaskField("Options", buildOption);
                buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("BuildTarget", buildTarget);
                genFile = EditorGUILayout.Toggle("GenFile", genFile);
                if (GUILayout.Button("GlobleBulid"))
                {
                    buildCtrl.BuildGlobalAssetBundle(buildpath, buildOption, buildTarget, genFile);
                }
                #endregion
            }
        }
        void SelectionChanged()
        {
            selectionGameObject = Selection.GetFiltered(typeof(GameObject), SelectionMode.DeepAssets);
            selectionMaterial = Selection.GetFiltered(typeof(Material), SelectionMode.DeepAssets);
            selectionTexture = Selection.GetFiltered(typeof(Texture), SelectionMode.DeepAssets);

            if (Selection.activeObject!=null)
            {
                assetBundleName = Selection.activeObject.name;
            }
        }
    }
}
