using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEngine;

namespace GuruFramework
{
    public class GResBuilder: EditorWindow
    {
        public const string K_EDITOR_MENU_ROOT = "Tools";
        public const string K_RESOURCES_PATH = "ResourceG";
        public const string K_TITLE_NAME = "AAB资源管理";
        public const string K_ADDRESSABLE_ROOT = "Assets/AddressableAssetsData";
        public const string K_GRES_CACHE_ROOT = K_ADDRESSABLE_ROOT + "/GRes";
        public const string K_BUILD_DATA_BIN_NAME = "addressables_content_state.bin";

        public static string ActiveBuildTargetName => EditorUserBuildSettings.activeBuildTarget.ToString();
        public static string ResourcesPath => Path.GetFullPath(Path.Combine(Application.dataPath, K_RESOURCES_PATH));
        public static bool IsResPathExists => Directory.Exists(ResourcesPath);
        
        public static string AddressableRootPath => Path.GetFullPath(K_ADDRESSABLE_ROOT);
        public static bool IsAddressableRootExists => Directory.Exists(AddressableRootPath);
        
        public static string BuildDataBin => Path.Combine(AddressableRootPath, ActiveBuildTargetName, K_BUILD_DATA_BIN_NAME);
        public static bool IsBuildDataExists => File.Exists(BuildDataBin);
        
        public static string CachedBuildDataBin => Path.Combine(K_GRES_CACHE_ROOT, ActiveBuildTargetName, _activeProfileName, K_BUILD_DATA_BIN_NAME);
        public static bool IsCachedBuildDataExists => File.Exists(CachedBuildDataBin);

        public static string ServerDataPath => Path.GetFullPath(Path.Combine(Application.dataPath, $"../ServerData/{ActiveBuildTargetName}"));

        public static string LocalBuildPath =>
            Path.GetFullPath(Path.Combine(UnityEngine.AddressableAssets.Addressables.BuildPath, ActiveBuildTargetName));
        public static string RemoteBuildPath =>
            Path.GetFullPath(Settings.profileSettings.GetValueByName(Settings.activeProfileId, "RemoteBuildPath").Replace("[BuildTarget]",ActiveBuildTargetName ));
        
        public static string RemoteLoadPath => Settings.profileSettings.GetValueByName(Settings.activeProfileId, "RemoteLoadPath");

        private static GResBuilder _instance;
        public static GResBuilder Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = CreateWindow<GResBuilder>();
                    _instance.titleContent = new GUIContent(K_TITLE_NAME);
                    _instance.minSize = new Vector2(350, 400);
                }
                return _instance;
            }
        }

        private static AddressableAssetSettings _settings;
        static AddressableAssetSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
                }
                return _settings;
            }
            set => _settings = value;
        }

        private static string _activeProfileName;

        #region 编辑器功能

        [MenuItem(K_EDITOR_MENU_ROOT + "/ ["+K_TITLE_NAME+"]")]
        static void ShowEditor()
        {
            Instance.Close();
            Instance.Show();
        }
        

        #endregion
        
        #region 生命周期

        
        /// <summary>
        /// 初始化
        /// </summary>
        private void OnEnable()
        {


        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            if (!IsAddressableRootExists)
            {
                GUI_CreateAddressablesSettings();
                return;
            }
            
            if (!IsResPathExists)
            {
                GUI_BuildResPath();
                return;
            }
            // Settings 显示
            GUI_AddressableSettings();
            // 构建Bundles
            GUI_BuildBundles();
            // 扩展显示
            GUI_Extends();
        }
        

        #endregion

        #region 创建Addressables组件

        /// <summary>
        /// 创建Addressables配置及相关路径
        /// </summary>
        void GUI_CreateAddressablesSettings()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label($"[GKIT] Addressable配置需要初始化.\n路径: [{K_ADDRESSABLE_ROOT}]");
            GUILayout.EndHorizontal();

            this.GButtonGreen("建立新的资源文件夹", () =>
            {
                CreateAddressableSettings();
                AssetDatabase.Refresh();
            }, height: 60);

        }

        private void CreateAddressableSettings()
        {
            Debug.Log($"<color=#88ff00>开始创建Addressables相关配置...</color>");
            Settings = AddressableAssetSettingsDefaultObject.GetSettings(true); // 创建默认配置
        }

        

        #endregion

        #region 构建数据管理
        
        /// <summary>
        /// 检查构建数据
        /// </summary>
        void GUI_BuildBundles()
        {
            float h = 60;
            GUILayout.BeginVertical(new GUIStyle("box"));
            if (IsBuildDataExists)
            {
                this.GButton($"更新构建Bundles {_activeProfileName} {ActiveBuildTargetName}", () =>
                {
                    BuildBundles(false);
                }, Color.cyan, height: h);
                
                this.GButton($"全新构建Bundles {_activeProfileName} {ActiveBuildTargetName}", () =>
                {
                    BuildBundles(true);
                }, Color.green, height: h);
            }
            else
            {
                
                this.GColorUI(Color.gray, () =>
                {
                    GUILayout.Label("  请手动开始全新构建...");
                });
                GUILayout.Space(4);
                this.GButtonGreen($"全新构建Bundles {_activeProfileName} {ActiveBuildTargetName}", () =>
                {
                    BuildBundles(true);
                }, height: 60);
            }
            
            GUILayout.EndVertical();

            GUILayout.Space(4);
            
            // 数据管理
            GUILayout.BeginVertical(new GUIStyle("box"));
            
            GUILayout.BeginHorizontal();
            if (IsBuildDataExists)
            {
                // 当前构建数据存在
                this.GButton($"保存构建BIN文件 {_activeProfileName}", BackupBuildData, Color.yellow, height: 30);
            }
            
            if (IsCachedBuildDataExists)
            {
                // 缓存数据存在
                this.GButton($"加载构建BIN文件 {_activeProfileName}", RestoreBuildData, new Color(1f, 0.2f, 0.6f), height: 30);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(4);

            if (Directory.Exists(ServerDataPath))
            {
                GUILayout.BeginHorizontal();
                this.GButton("打开 RemoteBuildPath 目录", ()=>GUtils.Open(RemoteBuildPath) , Color.white, height: 30);
                this.GButton("打开 LocalBuildPath 目录", ()=>GUtils.Open(LocalBuildPath) , Color.white, height: 30);
                GUILayout.EndHorizontal();
                this.GButton("清除构建缓存 Clean All", CleanBuildCache, Color.red,  height: 30);
            }
            
            
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 备份现有构建数据
        /// </summary>
        private void BackupBuildData()
        {
            if (!File.Exists(BuildDataBin))
            {
                Debug.LogError("备份数据不存在, 请重新构建Bundles");
                return;
            }
            GUtils.CopyFile(BuildDataBin, CachedBuildDataBin);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("备份AA构建数据", $"构建数据文件 BIN 已备份!\n详见: {CachedBuildDataBin}", "好的");
        }
        
        /// <summary>
        /// 还原之前备份的构建数据
        /// </summary>
        private void RestoreBuildData()
        {
            if (!File.Exists(CachedBuildDataBin))
            {
                Debug.LogError("备份数据不存在, 请先备份现有的数据");
                return;
            }
            GUtils.CopyFile(CachedBuildDataBin, BuildDataBin);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("恢复AA构建数据", $"构建数据文件 BIN 已恢复!\n详见: {BuildDataBin}", "好的");
        }

        private void CleanBuildData()
        {
            if (File.Exists(BuildDataBin))
            {
                File.Delete(BuildDataBin);
                Debug.LogError($"<color=orange>已清除构建数据: {BuildDataBin}</color>");
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("清除AA构建数据", $"构建数据文件 BIN 已清除!", "好的");
            }
            else
            {
                EditorUtility.DisplayDialog("清除AA构建数据", $"无法找到数据文件 BIN ...", "好吧");
            }
        }

        private void CleanBuildCache()
        {
            CleanBuildAll();
            CleanServerData();
        }
        

        #endregion

        #region 路径管理
        
        /// <summary>
        /// 建立资源根目录
        /// </summary>
        void GUI_BuildResPath()
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("[GKIT] 依赖资源路径不存在, 需手动建立.");
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button($"建立新的资源文件夹[{K_RESOURCES_PATH}]", GUILayout.Height(60)))
            {
                Directory.CreateDirectory(ResourcesPath);
                AssetDatabase.Refresh();
            }
        }
        
        
        

        private void SetGroups()
        {
            DirectoryInfo dir = new DirectoryInfo(ResourcesPath);
            foreach (var d in dir.GetDirectories())
            {
                UpdateGroups(d);
            }

            foreach (var file in dir.GetFiles())
            {
                AddToGroup( Settings.DefaultGroup.Name, GetAssetPath(file));
            }
            
            EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }


        /// <summary>
        /// 创建Group
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public static AddressableAssetGroup CreateGroup(AddressableAssetSettings settings, string groupName)
        {
            return settings.CreateGroup(groupName, false, false, false,
                new List<AddressableAssetGroupSchema>(){settings.DefaultGroup.Schemas[0], settings.DefaultGroup.Schemas[1]});
        }
        
        /// <summary>
        /// 添加组件到组
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="assetPath"></param>
        /// <param name="simple"></param>
        public static void AddToGroup(string groupName, string assetPath)
        {
            AddressableAssetGroup group = Settings.FindGroup(groupName);
            if (null == group) group = CreateGroup(Settings, groupName);

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!string.IsNullOrEmpty(guid))
            {
                var entry = Settings.CreateOrMoveEntry(guid, group);
                int start = assetPath.IndexOf(K_RESOURCES_PATH, StringComparison.Ordinal) + K_RESOURCES_PATH.Length + 1;
                int end = assetPath.IndexOf(".", StringComparison.Ordinal);
                entry.address = assetPath.Substring(start, end- start);
                entry.SetLabel(groupName.ToLower(), true, true);
                Debug.Log($"Add Asset[{guid}]: {entry.address} -> {groupName}");
            }
        }

        public static string GetAssetPath(FileInfo file)
        {
            string path = file.FullName.Replace("//", "/").Replace("\\", "/");
            return path.Substring(path.IndexOf("Assets/", StringComparison.Ordinal));
        }
        
        private void UpdateGroups(DirectoryInfo dir)
        {
            if (dir.Exists)
            {
                var files = GetFilesInDir(dir);
                foreach (var file in files)
                {
                    AddToGroup(dir.Name, GetAssetPath(file));
                }
            }
        }
        
        

        private List<FileInfo> GetFilesInDir(DirectoryInfo dir)
        {
            List<FileInfo> list = new List<FileInfo>();
            if (dir.Exists)
            {
                var files = dir.GetFiles();
                var dirs = dir.GetDirectories();
                
                if (files.Length > 0)
                {
                    list.AddRange(files);
                }

                if (dirs.Length > 0)
                {
                    foreach (var d in dirs)
                    {
                        list.AddRange(GetFilesInDir(d));
                    }
                }
                
            }
            return list;
        }
        
        #endregion

        #region 加载模式

        
        private void GUI_AddressableSettings()
        {
            float hw = 120;
            GUILayout.Space(10);
            // 入口快捷方式
            GUILayout.BeginHorizontal("box");
            this.GButton("打开 [Addressable] 管理器", OpenAddressableAssetsWindow, Color.white, height:30);
            GUILayout.EndHorizontal();
            
            // 加载模式
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Script:",GUILayout.Width(hw));
            GUI_BuildScriptDropdown();
            GUILayout.EndHorizontal();
            
            // Profile 选择
            // GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Profile:", GUILayout.Width(hw));
            GUI_ProfileDropdown();
            this.GButton("管理", OpenProfileWindow, width: 80);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("box");
            GUILayout.Label($"RemoteLoadPath", GUILayout.Width(hw));
            GUILayout.TextField(RemoteLoadPath);
            GUILayout.EndHorizontal();
            // GUILayout.EndVertical();
            GUILayout.Space(10);
        }
        
        void OnSetActivePlayModeScript(object context)
        {
            Settings.ActivePlayModeDataBuilderIndex = (int)context;
            AssetDatabase.SaveAssets();
        }

        void OpenAddressableAssetsWindow()
        {
            var asm = Assembly.Load("Unity.Addressables.Editor");
            if (asm == null) return;
                
            var clz = asm.GetType("UnityEditor.AddressableAssets.GUI.AddressableAssetsWindow");
            if (clz == null) return;
            //Call AddressableAssetsWindow.Init();
            var m = clz.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            if (m != null) m.Invoke(clz, null);
        }


        void GUI_BuildScriptDropdown()
        {
            var guiMode = new GUIContent(Settings.GetDataBuilder(Settings.ActivePlayModeDataBuilderIndex).Name);
            Rect rMode = GUILayoutUtility.GetRect(guiMode, EditorStyles.toolbarDropDown);
            if (EditorGUI.DropdownButton(rMode, guiMode, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();
                for (int i = 0; i < Settings.DataBuilders.Count; i++)
                {
                    var m = Settings.GetDataBuilder(i);
                    if (m.CanBuildData<AddressablesPlayModeBuildResult>())
                        menu.AddItem(new GUIContent(m.Name), i == Settings.ActivePlayModeDataBuilderIndex, OnSetActivePlayModeScript, i);
                }
                menu.DropDown(rMode);
            }
        }
        
        

        void GUI_ProfileDropdown()
        {
            _activeProfileName = Settings.profileSettings.GetProfileName(Settings.activeProfileId);
            var style = GUI.skin.FindStyle("ToolbarButton");
            if (string.IsNullOrEmpty(_activeProfileName))
            {
                Settings.activeProfileId = null; //this will reset it to default.
                _activeProfileName = Settings.profileSettings.GetProfileName(Settings.activeProfileId);
            }
            var profileButton = new GUIContent(_activeProfileName);

            Rect r = GUILayoutUtility.GetRect(profileButton, style);
            if (EditorGUI.DropdownButton(r, profileButton, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                //GUIUtility.hotControl = 0;
                var menu = new GenericMenu();

                var nameList = Settings.profileSettings.GetAllProfileNames();

                foreach (var name in nameList)
                {
                    menu.AddItem(new GUIContent(name), name == _activeProfileName, SetActiveProfile, name);
                }
                // menu.AddSeparator(string.Empty);
                // menu.AddItem(new GUIContent("Manage Profiles"), false, OpenProfileWindow);
                menu.DropDown(r);
            }
        }
        
        void SetActiveProfile(object context)
        {
            var n = context as string;
            Settings.activeProfileId = Settings.profileSettings.GetProfileId(n);
            // AddressableAssetUtility.OpenAssetIfUsingVCIntegration(AddressableAssetSettingsDefaultObject.Settings);
        }

        void OpenProfileWindow()
        {
            // EditorWindow.GetWindow<ProfileWindow>().Show(true);
            
            var asm = Assembly.Load("Unity.Addressables.Editor");
            if (asm == null) return;
                
            var clz = asm.GetType("UnityEditor.AddressableAssets.GUI.ProfileWindow");
            if (clz == null) return;
            //Call AddressableAssetsWindow.Init();
            var m = clz.GetMethod("ShowWindow", BindingFlags.Static | BindingFlags.NonPublic);
            if (m != null) m.Invoke(clz, null);
        }

        #endregion
        
        #region Addressable 构建管理
        
        public void CleanServerData()
        {
            if (Directory.Exists(ServerDataPath))
            {
                DirectoryInfo root = new DirectoryInfo(ServerDataPath);
                FileInfo[] files = root.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    File.Delete(ServerDataPath + "/" + files[i].Name);
                }
                DirectoryInfo[] dirs = root.GetDirectories("*", SearchOption.AllDirectories);
                foreach (var d in dirs)
                {
                    Directory.Delete(d.FullName);
                }
            }
        }
        
        

        public void BuildBundles(bool makeNewBuild = false)
        {
            AddressableAssetSettings.CleanPlayerContent();
            CleanServerData();
            SetGroups();

            if (!makeNewBuild && File.Exists(BuildDataBin) ) 
            {
                UpdateExistBuild();
            }
            else
            {
                MakeNewBuild();
            }
            
        }
        

        public void MakeNewBuild()
        {
            AddressableAssetSettings.BuildPlayerContent();
            if(!IsCachedBuildDataExists && File.Exists(BuildDataBin)) BackupBuildData();
        }

        public void CleanBuildAll()
        {
            AddressableAssetSettings.CleanPlayerContent();
            BuildCache.PurgeCache(true);
        }


        public void UpdateExistBuild()
        {
            CleanBuildAll();
            
            // 查找构建过的缓存记录
            string path = BuildDataBin;

            if (!File.Exists(path))
                path = ContentUpdateScript.GetContentStateDataPath(true);
            
            if (File.Exists(path))
            {
                ContentUpdateScript.BuildContentUpdate(Settings, path);
            }
            else
            {
                if (EditorUtility.DisplayDialog("查找文件失败", "找不到上次打包的数据: \n{path}. \n 是否创建新构建?", "是的, 重新构建.", "不用"))
                {
                    MakeNewBuild();
                }
            }
            
        }
        
        
        

        #endregion
        
        #region 扩展显示

        private MethodInfo _extOnGUI = null;
        
        /// <summary>
        /// GUI扩展
        /// </summary>
        private void GUI_Extends()
        {
            if (_extOnGUI == null)
            {
                var clz = Type.GetType("GResBuilderExtends");
                if (clz != null)
                {
                    var inst = Activator.CreateInstance(clz);
                    if (inst != null)
                    {
                        _extOnGUI = clz.GetMethod("OnGUI", BindingFlags.Public | BindingFlags.Static);
                    }
                }
            }

            if (_extOnGUI != null) _extOnGUI.Invoke(null, null);

        }
        

        #endregion
    }
}