using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace AssetBundleBuilder
{
    public class ConfigBuilderWindow : EditorWindow
    {
        [MenuItem(ABBUtility.Menu_ConfigBuildWindow)]
        static void BuildSingleAssetBundle()
        {
          var window =  EditorWindow.GetWindow<ConfigBuilderWindow>("局部AssetBundle", true);
            window.position = new Rect(100, 200, 800, 520);
        }
        public class LayerNode
        {
            public bool isExpanded { get; private set; }
            public bool isFolder;
            public bool selected { get; private set; }
            public int indent { get; private set; }
            public GUIContent content;
            public LayerNode parent;
            public List<LayerNode> childs = new List<LayerNode>();
            public Object layer { get; private set; }
            public string assetPath { get; private set; }
          
            private GUIContent _spritenormal;
            private string ContentName
            {
                get
                {
                    AssetImporter asset = AssetImporter.GetAtPath(assetPath);
                    if (string.IsNullOrEmpty(asset.assetBundleName))
                    {
                        return layer.name;
                    }
                    else
                    {
                        return string.Format("{0}  [ab]:{1}", layer.name, asset.assetBundleName);
                    }
                }
            }
            public LayerNode(string path)
            {
                this.assetPath = path;
                this.layer = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                this.indent = assetPath.Split('/').Length;
                isFolder = ProjectWindowUtil.IsFolder(layer.GetInstanceID());
                var name = ContentName;

                if (layer is Texture)
                {
                    _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("Texture Icon").image);
                }
                else if (layer is Material)
                {
                    _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("Material Icon").image);
                }
                else if (layer is GameObject)
                {
                    if (assetPath.EndsWith(".prefab"))
                    {
                        _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("PrefabNormal Icon").image);
                    }
                    else
                    {
                        _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("MeshRenderer Icon").image);
                    }
                }
                else if (layer is ScriptableObject)
                {
                    _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("ScriptableObject Icon").image);
                }
                else
                {
                    _spritenormal = new GUIContent(name, EditorGUIUtility.IconContent("FolderEmpty Icon").image);
                }

                content = _spritenormal;
                isExpanded = true;
            }

            public void Expland(bool on)
            {
                //content = on ? _groupOn : _groupff;
                //content.text = ContentName;
                isExpanded = on;
            }
            public void Select(bool on)
            {
                selected = on;
                if (childs != null)
                    foreach (var child in childs)
                    {
                        child.Select(on);
                    }
            }
        }
        private SerializedProperty script;
        private ConfigBuildObj buildObj;
        private LayerNode rootNode;
        private const string lastItem = "lastbuildObj";
        private Vector2 scrollPos;
        private Dictionary<string, LayerNode> nodeDic;
        private GUIContent _groupff;
        private GUIContent _groupOn;

        private void OnEnable()
        {
            _groupff = new GUIContent(EditorGUIUtility.IconContent("IN foldout focus").image);
            _groupOn =  new GUIContent(EditorGUIUtility.IconContent("IN foldout focus on").image);
            script = new SerializedObject(this).FindProperty("m_Script");
            if (EditorPrefs.HasKey(lastItem))
            {
                buildObj = AssetDatabase.LoadAssetAtPath<ConfigBuildObj>(AssetDatabase.GUIDToAssetPath(EditorPrefs.GetString(lastItem)));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.PropertyField(script);

            if (DrawObjectHolder())
            {
                DrawObjOptions();
                if (rootNode == null)
                {
                    nodeDic = LoadDicFromObj(buildObj);
                    rootNode = LoadNodesFromDic(nodeDic);
                }
                else
                {
                    DrawHeadTools();
                    EditorGUI.indentLevel = 0;
                    using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPos, false, true, GUILayout.Height(300)))
                    {
                        scroll.handleScrollWheel = true;
                        scrollPos = scroll.scrollPosition;
                        var rect = GUILayoutUtility.GetRect(300, 300);
                        EditorGUI.DrawRect(rect, new Color(0, 1, 0, 0.1f));
                        rect.height = EditorGUIUtility.singleLineHeight;
                        DrawData(rect, rootNode);
                    }
                    EditorGUI.indentLevel = 0;
                    DrawBottomTools();
                }
            }
        }
        private bool DrawObjectHolder()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                var obj = EditorGUILayout.ObjectField(buildObj, typeof(ConfigBuildObj), false) as ConfigBuildObj;
                if (obj != buildObj)
                {
                    buildObj = obj;
                    EditorPrefs.SetString(lastItem, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(buildObj)));
                }
                if (GUILayout.Button("创建"))
                {
                    buildObj = ScriptableObject.CreateInstance<ConfigBuildObj>();
                    ProjectWindowUtil.CreateAsset(buildObj, "configBuildObj.asset");
                }
            }

            if (buildObj == null)
            {
                EditorGUILayout.HelpBox("请先将配制对象放入", MessageType.Error);
                return false;
            }
            return true;
        }
        private void DrawObjOptions()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[资源导出路径]:", GUILayout.Width(100));
                buildObj.exportPath = EditorGUILayout.TextField(buildObj.exportPath);
                if (GUILayout.Button("选择"))
                {
                    buildObj.exportPath = EditorUtility.OpenFolderPanel("选择文件路径", buildObj.exportPath, "");
                }
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("[菜单名]:", GUILayout.Width(100));
                buildObj.menuName = EditorGUILayout.TextField(buildObj.menuName);
                if (GUILayout.Button("保存"))
                {
                    //保存
                    StoreLayerNodeToAsset(nodeDic, buildObj);
                }
            }
        }
        private static Dictionary<string, LayerNode> LoadDicFromObj(ConfigBuildObj buildObj)
        {
            if (buildObj.needBuilds == null)
            {
                return null;
            }
            else
            {
                Dictionary<string, LayerNode> nodeDic = new Dictionary<string, AssetBundleBuilder.ConfigBuilderWindow.LayerNode>();
                nodeDic.Add("Assets", new LayerNode("Assets"));
                foreach (var item in buildObj.needBuilds)
                {
                    RetriveAddFolder(item.assetPath, nodeDic);
                    if (!nodeDic.ContainsKey(item.assetPath))
                    {
                        var path = AssetDatabase.GetAssetPath(item.obj);
                        nodeDic.Add(item.assetPath, new LayerNode(path));
                    }
                }
                return nodeDic;
            }
        }
        private static LayerNode LoadNodesFromDic(Dictionary<string, LayerNode> nodeDic)
        {
            if (nodeDic == null) return null;
            LayerNode root = nodeDic["Assets"];
            foreach (var item in nodeDic)
            {
                item.Value.parent = null;
                item.Value.childs.Clear();
            }
            foreach (var item in nodeDic)
            {
                foreach (var child in nodeDic)
                {
                    if (child.Key.Contains(item.Key + "/") && !child.Key.Replace(item.Key + "/", "").Contains("/"))
                    {
                        item.Value.childs.Add(child.Value);
                        child.Value.parent = item.Value;
                        if (root == null || AssetDatabase.GetAssetPath(root.layer).Contains(item.Key))
                        {
                            root = item.Value;
                        }
                    }
                }
            }
            return root;

        }

        private static void RetriveAddFolder(string assetPath, Dictionary<string, LayerNode> nodeDic)
        {
            var folder = assetPath.Remove(assetPath.LastIndexOf("/"));
            if (folder != assetPath && !nodeDic.ContainsKey(folder))
            {
                nodeDic.Add(folder, new LayerNode(folder));
            }
            if (folder.Contains("/"))
            {
                RetriveAddFolder(folder, nodeDic);
            }
        }
        private static void StoreLayerNodeToAsset(Dictionary<string, LayerNode> nodeDic, ConfigBuildObj buildObj, bool selectedOnly = false)
        {
            foreach (var item in nodeDic)
            {
                if (!ProjectWindowUtil.IsFolder(item.Value.layer.GetInstanceID()))
                {
                    if (selectedOnly && !item.Value.selected)
                    {
                        continue;
                    }
                    var oitem = buildObj.needBuilds.Find(x => x.obj == item.Value.layer);
                    if (oitem == null)
                    {
                        buildObj.needBuilds.Add(new ConfigBuildObj.ObjectItem(item.Value.layer));
                    }
                }
            }
            buildObj.ReFelsh();
            EditorUtility.SetDirty(buildObj);
        }

        /// <summary>
        /// 遍历文件及子文件
        /// </summary>
        /// <param name="root"></param>
        /// <param name="OnRetrive"></param>
        private static void RetriveObject(string root, UnityAction<Object> OnRetrive)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(root);
            if (obj == null)
            {
                return;
            }
            else
            {
                OnRetrive(obj);
                if (ProjectWindowUtil.IsFolder(obj.GetInstanceID()))
                {
                    var files = System.IO.Directory.GetFiles(root);
                    foreach (var item in files)
                    {
                        if (!item.EndsWith(".meta"))
                        {
                            var path = item.Replace(Application.dataPath, "Assets");
                            RetriveObject(path, OnRetrive);
                        }
                    }
                }
            }

        }

        //绘制添加，删除,重置等功能         
        private void DrawHeadTools()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(new GUIContent("+", "从Project添加"), GUILayout.Width(20)))
                {
                    if (Selection.activeObject == null) return;

                    var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    RetriveObject(assetPath, (x) =>
                    {
                        assetPath = AssetDatabase.GetAssetPath(x);

                        if (!nodeDic.ContainsKey(assetPath))
                        {
                            nodeDic.Add(assetPath, new LayerNode(assetPath));
                        }
                    });

                    rootNode = LoadNodesFromDic(nodeDic);
                }
                if (GUILayout.Button(new GUIContent("-", "移除选中"), GUILayout.Width(20)))
                {
                    var selectedNode = new List<string>();
                    foreach (var item in nodeDic)
                    {
                        if (item.Value.selected) selectedNode.Add(item.Key);
                    }
                    foreach (var item in selectedNode)
                    {
                        nodeDic.Remove(item);
                    }
                    rootNode = LoadNodesFromDic(nodeDic);
                }
                //导入相关
                if (GUILayout.Button(new GUIContent("~", "导入关联资源"), GUILayout.Width(20)))
                {
                    var needAdd = new List<string>();
                    foreach (var item in nodeDic)
                    {
                        if (item.Value.selected)
                        {
                            var childs = AssetDatabase.GetDependencies(item.Value.assetPath,true);
                            foreach (var child in childs)
                            {
                                if (!needAdd.Contains(child))
                                {
                                    needAdd.Add(child);
                                }
                            }
                        }
                    }
                    foreach (var item in needAdd)
                    {
                        RetriveAddFolder(item, nodeDic);
                        if (!nodeDic.ContainsKey(item))
                        {
                            nodeDic.Add(item, new LayerNode(item));
                        }
                    }
                    rootNode = LoadNodesFromDic(nodeDic);
                }
                //选中所有引用
                if (GUILayout.Button(new GUIContent("&", "关联资源"), GUILayout.Width(20)))
                {
                    var needSelect = new List<string>();
                    foreach (var item in nodeDic)
                    {
                        if (item.Value.selected)
                        {
                            var childs = AssetDatabase.GetDependencies(item.Value.assetPath, true);
                            foreach (var child in childs)
                            {
                                if (!needSelect.Contains(child))
                                {
                                    needSelect.Add(child);
                                }
                            }
                        }
                    }
                    foreach (var item in nodeDic)
                    {
                        item.Value.Select((needSelect.Contains(item.Key)));
                    }
                }
                //刷新
                if (GUILayout.Button(new GUIContent("*", "刷新重置"), GUILayout.Width(20)))
                {
                    nodeDic = LoadDicFromObj(buildObj);
                    rootNode = LoadNodesFromDic(nodeDic);
                }
            }
        }

        private Rect DrawData(Rect rt, LayerNode data)
        {
            Rect newRt = rt;
            if (data.content != null)
            {
                EditorGUI.indentLevel = data.indent;
                newRt = DrawGUIData(rt, data);
            }
            if (data.isExpanded)
            {
                for (int i = 0; i < data.childs.Count; i++)
                {
                    LayerNode child = data.childs[i];
                    if (child.content != null)
                    {
                        EditorGUI.indentLevel = child.indent;
                        newRt.y += EditorGUIUtility.singleLineHeight;
                        if (child.childs.Count > 0)
                        {
                            newRt = DrawData(newRt, child);
                        }
                        else
                        {
                            newRt = DrawGUIData(newRt, child);
                        }
                    }
                }
            }
            return newRt;
        }
        private Rect DrawGUIData(Rect rt, LayerNode data)
        {
            GUIStyle style = "Label";
            //Rect rt = GUILayoutUtility.GetRect(data.content, style);

            var offset = (16 * EditorGUI.indentLevel);
            var pointWidth = 10;

            if (Event.current != null && rt.Contains(Event.current.mousePosition)
              && Event.current.button == 0 && Event.current.type <= EventType.mouseUp)
            {
                Selection.activeObject = data.layer;
            }

            var srect = new Rect(rt.x + offset, rt.y, rt.width - offset - pointWidth, EditorGUIUtility.singleLineHeight);
            var selected = EditorGUI.Toggle(srect, data.selected, style);
            if (selected != data.selected)
            {
                data.Select(selected);
            }
            if (data.selected)
            {
                EditorGUI.DrawRect(srect, new Color(1, 1, 1, 0.1f));
            }
            if (data.isFolder)
            {
                var btnRect = new Rect(rt.x + offset - pointWidth * 2, rt.y, pointWidth * 2, rt.height);
                if (GUI.Button(btnRect, data.isExpanded ? _groupOn : _groupff, style))
                {
                    data.Expland(!data.isExpanded);
                }
            }
          
            EditorGUI.LabelField(rt, data.content);
            return rt;
        }

        private void DrawBottomTools()
        {
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel("[目标平台]：");
                buildObj.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(buildObj.buildTarget);
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel("[打包选项]：");
                buildObj.buildOption = (BuildAssetBundleOptions)EditorGUILayout.EnumMaskField(buildObj.buildOption);
            }
            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.SelectableLabel("[清空文件]：");
                buildObj.clearOld = EditorGUILayout.Toggle(buildObj.clearOld);
            }

            using (var hor = new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("生成AB(全部)"))
                {
                    if (buildObj.clearOld) FileUtil.DeleteFileOrDirectory(buildObj.ExportPath);

                    ConfigBuildObj bo = ScriptableObject.CreateInstance<ConfigBuildObj>();
                    StoreLayerNodeToAsset(nodeDic, bo);
                    ABBUtility.BuildGroupBundles(buildObj.ExportPath, bo.GetBundleBuilds(), buildObj.buildOption, buildObj.buildTarget);
                }
                if (GUILayout.Button("生成AB(选中)"))
                {
                    if (buildObj.clearOld) FileUtil.DeleteFileOrDirectory(buildObj.ExportPath);

                    ConfigBuildObj bo = ScriptableObject.CreateInstance<ConfigBuildObj>();
                    StoreLayerNodeToAsset(nodeDic, bo, true);
                    ABBUtility.BuildGroupBundles(buildObj.ExportPath, bo.GetBundleBuilds(), buildObj.buildOption, buildObj.buildTarget);
                }
            }
        }

    }

}
