using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace UniEasy.Editor
{
    public class BatchProcessingWindow : EditorWindow
    {
        private int index;
        private string findPath;
        private string searchPath;
        private Vector2 tabPanelPosition;
        private Vector2 viewPanelPosition;
        private Vector2 controlPanelPosition;
        private Object prefabObject = null;
        private Object[] matchedAssets;
        private Object[] foundAssets;
        private Dictionary<string, string> movePaths = new Dictionary<string, string>();
        private Dictionary<string, List<Object>> moveAssetObjects = new Dictionary<string, List<Object>>();
        private Dictionary<Material, bool> renderQueueFoldouts = new Dictionary<Material, bool>();
        private Dictionary<string, int> renderQueueLabels = new Dictionary<string, int>();

        private static string BatchProcessingWindowStr = "Batch Processing Window";
        private static string BaseSettingsStr = "Base Settings";
        private static string ViewPanelStr = "View Panel";
        private static string ControlPanelStr = "Control Panel";
        private static string FormatSelectedGameObjectsStr = "Format Selected GameObjects";
        private static string FormatSelectedGameObjectsNameStr = "Format Selected GameObjects Name";
        private static string OrderSelectedGameObjectsStr = "Order Selected GameObjects";
        private static string ReplaceGameObjectsWithPrefabStr = "Replace GameObjects With Prefab";
        private static string CategorizingReferencedAssetsStr = "Categorizing Referenced Assets";
        private static string FindReferencesInPathStr = "Find References In Path";
        private static string ChangeRenderQueueStr = "Change Render Queue";
        private static string ManageRenderQueueStr = "Manage RenderQueue";
        private static string PrefabForReplaceStr = "Prefab For Replace";
        private static string AnalysisObjectStr = "Analysis Object";
        private static string FindReferencesStr = "Find References";
        private static string RenderQueueStr = "Render Queue";
        private static string MovePathStr = "Move {0} To ...";
        private static string SearchPathStr = "Search Path";
        private static string FindPathStr = "Find Path";
        private static string CancelMoveStr = "Cancel Move";
        private static string StartMoveStr = "Start Move";
        private static string ElementStr = "Element {0}";
        private static string FolderStr = "...";
        private static string EmptyStr = "";
        private static string OkStr = "Ok";

        private static string SearchPathKey = "Batch_Processing_Window_Search_Path";
        private static string MovePathKey = "Batch_Processing_Window_Move_Path_";
        private static string FindPathKey = "Batch_Processing_Window_Find_Path";

        [MenuItem("Window/Batch Processing Window")]
        private static void Initialize()
        {
            var window = GetWindow<BatchProcessingWindow>();

            window.titleContent = new GUIContent(BatchProcessingWindowStr);
            window.Show();
        }

        private void Awake()
        {
            findPath = EditorPrefs.HasKey(FindPathKey) ? EditorPrefs.GetString(FindPathKey) : Application.dataPath;
            searchPath = EditorPrefs.HasKey(SearchPathKey) ? EditorPrefs.GetString(SearchPathKey) : Application.dataPath;

            renderQueueLabels.Add("Background", 0);
            renderQueueLabels.Add("Geometry", 1500);
            renderQueueLabels.Add("AlphaTest", 2225);
            renderQueueLabels.Add("Transparent", 2725);
            renderQueueLabels.Add("Overlay", 3500);
        }

        private void OnGUI()
        {
            GUILayout.Label(BaseSettingsStr, EditorStyles.boldLabel);

            index = EasyGUILayout.Tabs(index, ref tabPanelPosition, FormatSelectedGameObjectsStr, ReplaceGameObjectsWithPrefabStr, CategorizingReferencedAssetsStr, ManageRenderQueueStr, FindReferencesInPathStr);

            EditorGUILayout.BeginHorizontal();

            // View Panel Part
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            viewPanelPosition = GUILayout.BeginScrollView(viewPanelPosition);
            OnViewGUI(index);
            GUILayout.EndScrollView();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            // Control Panel Part
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            controlPanelPosition = GUILayout.BeginScrollView(controlPanelPosition);
            OnControlGUI(index);
            GUILayout.EndScrollView();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString(SearchPathKey, searchPath);
            EditorPrefs.SetString(FindPathKey, findPath);
            foreach (var kvp in movePaths)
            {
                EditorPrefs.SetString(kvp.Key, kvp.Value);
            }
        }

        private void OnViewGUI(int index)
        {
            EditorGUILayout.LabelField(ViewPanelStr, EditorStyles.boldLabel);
            if (index == 2 && HasMatchedAssets())
            {
                OnViewAssetsGUI(matchedAssets);
            }
            else if (index == 3)
            {
                OnViewRenderQueueGUI();
            }
            else if (index == 4 && HasFoundAssets())
            {
                OnViewAssetsGUI(foundAssets);
            }
            else
            {
                foreach (var obj in Selection.objects)
                {
                    EditorGUILayout.ObjectField(obj, typeof(Object), false);
                }
            }
        }

        private void OnControlGUI(int index)
        {
            EditorGUILayout.LabelField(ControlPanelStr, EditorStyles.boldLabel);
            if (index == 0)
            {
                if (GUILayout.Button(FormatSelectedGameObjectsNameStr))
                {
                    FormatUtility.FormatGameObjectsName(Selection.gameObjects);
                }
                if (GUILayout.Button(OrderSelectedGameObjectsStr))
                {
                    FormatUtility.OrderSelectedGameObjects();
                }
            }
            else if (index == 1)
            {
                prefabObject = EditorGUILayout.ObjectField(PrefabForReplaceStr, prefabObject, typeof(GameObject), false);

                EditorGUI.BeginDisabledGroup(prefabObject == null || PrefabUtility.GetPrefabType(prefabObject) != PrefabType.Prefab);
                if (GUILayout.Button(ReplaceGameObjectsWithPrefabStr))
                {
                    PrefabsUtility.ReplacingGameObjectsWithPrefab(prefabObject as GameObject, Selection.objects.Select(obj => obj as GameObject).Where(go => go != null).ToArray());
                }
                EditorGUI.EndDisabledGroup();
            }
            else if (index == 2)
            {
                if (HasMatchedAssets())
                {
                    OnControlMatchedAssetsGUI();
                }
                else
                {
                    OnControlAnalysisAssetGUI();
                }
            }
            else if (index == 3)
            {
                OnControlRenderQueueGUI();
            }
            else if (index == 4)
            {
                if (HasFoundAssets())
                {
                    OnControlFoundAssetsGUI();
                }
                else
                {
                    OnControlFindReferencesGUI();
                }
            }
        }

        private void OnMatchAssetsCompleted(List<Object> assetObjects)
        {
            matchedAssets = assetObjects.OrderBy(obj => obj.GetType().Name).ToArray();
            AssetsUtility.OnMatchAssetsCompleted -= OnMatchAssetsCompleted;
        }

        private bool HasMatchedAssets()
        {
            return (matchedAssets != null && matchedAssets.Length > 0);
        }

        private void OnViewAssetsGUI(Object[] assetObjects)
        {
            System.Type type, lastType = null;
            foreach (var assetObject in assetObjects)
            {
                type = assetObject.GetType();
                if (lastType != null && type != lastType)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
                if (lastType == null || type != lastType)
                {
                    EditorGUILayout.LabelField(type.Name);
                }
                EditorGUILayout.ObjectField(assetObject, type, false);
                lastType = type;
            }
        }

        private void OnControlAnalysisAssetGUI()
        {
            EditorGUILayout.BeginHorizontal();
            searchPath = EditorGUILayout.TextField(SearchPathStr, searchPath);
            if (GUILayout.Button(FolderStr, GUILayout.MaxWidth(50)))
            {
                var path = EditorUtility.OpenFolderPanel(SearchPathStr, searchPath, EmptyStr);
                if (!string.IsNullOrEmpty(path))
                {
                    searchPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.BeginDisabledGroup(!AssetsUtility.IsAnalysable(Selection.activeObject));
            if (GUILayout.Button(AnalysisObjectStr))
            {
                AssetsUtility.AnalysisAsset(Selection.activeObject, searchPath);
                AssetsUtility.OnMatchAssetsCompleted += OnMatchAssetsCompleted;
            }
            EditorGUI.EndDisabledGroup();
        }

        private void OnControlMatchedAssetsGUI()
        {
            string movePathKey;
            System.Type type, lastType = null;
            foreach (var assetObject in matchedAssets)
            {
                type = assetObject.GetType();
                if (lastType != null && type != lastType)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
                if (lastType == null || type != lastType)
                {
                    movePathKey = MovePathKey + type.Name;
                    if (!movePaths.ContainsKey(movePathKey))
                    {
                        if (EditorPrefs.HasKey(movePathKey))
                        {
                            movePaths.Add(movePathKey, EditorPrefs.GetString(movePathKey));
                        }
                        else
                        {
                            movePaths.Add(movePathKey, Application.dataPath);
                        }
                    }
                    EditorGUILayout.LabelField(string.Format(MovePathStr, type.Name));
                    EditorGUILayout.BeginHorizontal();
                    movePaths[movePathKey] = EditorGUILayout.TextField(movePaths[movePathKey]);
                    if (GUILayout.Button(FolderStr, GUILayout.MaxWidth(50)))
                    {
                        var path = EditorUtility.OpenFolderPanel(string.Format(MovePathStr, type.Name), movePaths[movePathKey], EmptyStr);
                        if (!string.IsNullOrEmpty(path))
                        {
                            movePaths[movePathKey] = path;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField(EmptyStr);
                }
                lastType = type;
            }

            if (GUILayout.Button(StartMoveStr))
            {
                moveAssetObjects.Clear();

                foreach (var assetObject in matchedAssets)
                {
                    type = assetObject.GetType();
                    movePathKey = MovePathKey + type.Name;

                    if (!moveAssetObjects.ContainsKey(movePaths[movePathKey]))
                    {
                        moveAssetObjects.Add(movePaths[movePathKey], new List<Object>());
                    }
                    moveAssetObjects[movePaths[movePathKey]].Add(assetObject);
                }

                AssetsUtility.OnMoveAssetsCompleted += DoMoveAssets;
                AssetsUtility.OnMoveAssetsCanceled += DoMoveAssets;

                foreach (var kvp in moveAssetObjects)
                {
                    AssetsUtility.MoveAssets(kvp.Key, kvp.Value.ToArray());
                    break;
                }
            }
            if (GUILayout.Button(CancelMoveStr))
            {
                CancelMoveAssets();
            }
        }

        private void DoMoveAssets()
        {
            foreach (var kvp in moveAssetObjects)
            {
                moveAssetObjects.Remove(kvp.Key);
                break;
            }
            foreach (var kvp in moveAssetObjects)
            {
                AssetsUtility.MoveAssets(kvp.Key, kvp.Value.ToArray());
                break;
            }
            if (moveAssetObjects.Count <= 0)
            {
                AssetsUtility.OnMoveAssetsCompleted -= DoMoveAssets;
                AssetsUtility.OnMoveAssetsCanceled -= DoMoveAssets;
                CancelMoveAssets();
            }
        }

        private void CancelMoveAssets()
        {
            matchedAssets = null;
            moveAssetObjects.Clear();
        }

        private void OnViewRenderQueueGUI()
        {
            var mats = FormatUtility.GetOrderedMaterialsFromActivatedScenes();
            var labels = renderQueueLabels.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in mats)
            {
                if (!renderQueueFoldouts.ContainsKey(kvp.Key))
                {
                    renderQueueFoldouts.Add(kvp.Key, false);
                }
                DrawRenderQueueLabel(labels, kvp.Key.renderQueue);
                EditorGUILayout.BeginHorizontal();
                Rect position;
                renderQueueFoldouts[kvp.Key] = EasyGUILayout.Foldout(renderQueueFoldouts[kvp.Key], GUIContent.none, 25, out position);
                EditorGUI.ObjectField(position, kvp.Key, typeof(Material), false);
                EditorGUILayout.EndHorizontal();
                if (renderQueueFoldouts[kvp.Key])
                {
                    EditorGUI.indentLevel += 2;
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        EditorGUILayout.ObjectField(string.Format(ElementStr, i), kvp.Value[i], typeof(GameObject), false);
                    }
                    EditorGUI.indentLevel -= 2;
                }
            }
        }

        private void OnControlRenderQueueGUI()
        {
            var mats = FormatUtility.GetOrderedMaterialsFromActivatedScenes();
            var labels = renderQueueLabels.OrderBy(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            foreach (var kvp in mats)
            {
                DrawRenderQueueLabel(labels, kvp.Key.renderQueue, EmptyStr);
                EditorGUI.BeginChangeCheck();
                var renderQueue = EditorGUILayout.DelayedIntField(RenderQueueStr, kvp.Key.renderQueue);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(kvp.Key, ChangeRenderQueueStr);
                    kvp.Key.renderQueue = renderQueue;
                    EditorUtility.SetDirty(kvp.Key);
                    break;
                }

                if (renderQueueFoldouts.ContainsKey(kvp.Key) && renderQueueFoldouts[kvp.Key])
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        EditorGUILayout.LabelField(EmptyStr);
                    }
                }
            }
        }

        private void DrawRenderQueueLabel(Dictionary<string, int> labels, int renderQueue, string content = null)
        {
            var b = true;
            while (b && labels.Count > 0)
            {
                foreach (var label in labels)
                {
                    if (renderQueue >= label.Value)
                    {
                        EditorGUILayout.LabelField(content != null ? content : label.Key);
                        EditorGUILayout.Space();
                        labels.Remove(label.Key);
                        break;
                    }
                    b = false;
                }
            }
        }

        private void OnControlFindReferencesGUI()
        {
            EditorGUILayout.BeginHorizontal();
            findPath = EditorGUILayout.TextField(FindPathStr, findPath);
            if (GUILayout.Button(FolderStr, GUILayout.MaxWidth(50)))
            {
                var path = EditorUtility.OpenFolderPanel(FindPathStr, findPath, EmptyStr);
                if (!string.IsNullOrEmpty(path))
                {
                    findPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button(FindReferencesStr))
            {
                AssetsUtility.FindReferencesInPath(Selection.activeObject, findPath);
                AssetsUtility.OnFindAssetsCompleted += OnFindAssetsCompleted;
            }
        }

        private void OnFindAssetsCompleted(List<Object> assetObjects)
        {
            foundAssets = assetObjects.OrderBy(obj => obj.GetType().Name).ToArray();
            AssetsUtility.OnFindAssetsCompleted -= OnFindAssetsCompleted;
        }

        private bool HasFoundAssets()
        {
            return (foundAssets != null && foundAssets.Length > 0);
        }

        private void OnControlFoundAssetsGUI()
        {
            if (GUILayout.Button(OkStr))
            {
                ClearFoundAssets();
            }
        }

        private void ClearFoundAssets()
        {
            foundAssets = null;
        }
    }
}
