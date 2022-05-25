using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SRDebugger;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game
{
    public class GameView: MonoBehaviour
    {
        [SerializeField] private Button _btnDebug;
        
        [SerializeField] private Image _image;
        [SerializeField] private Button _btnLoad1;
        [SerializeField] private Button _btnLoad2;
        [SerializeField] private Button _btnLoad3;
        [SerializeField] private Button _btnLoadPack1;
        [SerializeField] private Button _btnLoadPack2;
        [SerializeField] private Image _islandBG;
        //-------- 下载面板 -------
        [SerializeField] private GameObject _loadInfo;
        [SerializeField] private Text _txtLoadInfo;
        [SerializeField] private Text _txtLoadProgress;
        [SerializeField] private Button _btnCloseLoad;
        
        public const string K_GROUP_ISLANDBG_NAME = "island_bg";
        
        private ResMgr Res = ResMgr.Instance;


        #region 初始化

        

        
        private void Awake()
        {
            SRDebug.Init(); // 初始化
            
            _islandBG.gameObject.SetActive(false);
            _btnLoad1.onClick.AddListener(LoadObject1);
            _btnLoad2.onClick.AddListener(LoadObject2);
            _btnLoad3.onClick.AddListener(LoadObject3);
            _btnLoadPack1.onClick.AddListener(OnClickPack1);
            _btnLoadPack2.onClick.AddListener(OnClickPack2);
            _btnCloseLoad.onClick.AddListener(OnCloseLoadInfo);
            _btnDebug.onClick.AddListener(OnOpenConsole);

            Debug.Log("-------------- GameView Awake ---------------");
            OnCloseLoadInfo(); // 关闭面板
            
            // 检查资源组是否已下载
            _btnLoad3.gameObject.SetActive(false);
            Res.CheckGroupIsDownloaded(K_GROUP_ISLANDBG_NAME, (loaded, size) =>
            {
                Debug.Log($"--- Loaded Size: {size}");
                _btnLoad3.gameObject.SetActive(loaded);
            });
        }

        private void OnOpenConsole()
        {
            SRDebug.Instance.ShowDebugPanel(DefaultTabs.Console);
        }
        
        /// <summary>
        /// 检测组是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        private void LoadGroup(string key, Action callback)
        {
            if (Res.MapExists(key))
            {
                callback?.Invoke(); 
            }
            else
            {
                // 直接加载组内资源, 并缓存
                Res.LoadGroupAssetsAsync(key, success =>
                {
                    if (success)
                    {
                        callback?.Invoke(); 
                    }
                    else
                    {
                        Debug.LogError($"加载Bundle {key} 失败");
                    }
                });
            }
          
        }


        #endregion

        #region 基础功能

        

   

        /// <summary>
        /// 加载物件1
        /// </summary>
        private void LoadObject1()
        {
            Debug.Log($"---- LoadObject1 ----");
            // 先判断Group是否存在, 不存在的话需要先加载
            LoadGroup("remote", SetIconSprite);
        }

        private void SetIconSprite()
        {
            var sp =Res.LoadSpriteFromAtlas("remote/atlas_shop", "ic_shop_more");
            if (sp != null)
            {
                _image.sprite = sp;
            }
        }



        private void LoadObject2()
        {
            Debug.Log($"---- LoadObject2 ----");
            LoadGroup("remote", LoadTestObject);
        }
        
        /// <summary>
        /// 加载测试物件
        /// </summary>
        private void LoadTestObject()
        {
            var obj = Res.GetInstance<TestObject>("remote/test_object", transform);
            if (obj == null)
            {
                Debug.LogError("--- 加载 TestObject 失败");
                return;
            }
            obj.transform.localPosition = new Vector3(800, 400, 0);

        }

        private void LoadObject3()
        {
            Debug.Log($"---- LoadObject3 ----");
            LoadGroup(K_GROUP_ISLANDBG_NAME, SetIslandBG);
        }
        
        /// <summary>
        /// 加载岛屿背景, 直接加载位图
        /// </summary>
        private void SetIslandBG()
        {
            var address = "island_bg/bg";
            var sp = Res.LoadSprite(address);
            if (sp != null)
            {
                _islandBG.gameObject.SetActive(true);
                _islandBG.sprite = sp;
                _islandBG.SetNativeSize();
                // var sn = _islandBG.material.shader.name;
                // _islandBG.material.shader = Shader.Find(sn); // 重新载入Shader
            }
        }
        
        #endregion

        #region 资源加载，下载，更新

        /// <summary>
        /// 点击Pack1 按钮
        /// </summary>
        private void OnClickPack1()
        {
            // 检查资源组是否已经下载
            Res.CheckGroupIsDownloaded(K_GROUP_ISLANDBG_NAME, (loaded, size) =>
            {
                if (loaded)
                {
                    Debug.Log($"---- {K_GROUP_ISLANDBG_NAME} 已经下载 -----");
                    _btnLoad3.gameObject.SetActive(true);
                }
                else
                {
                    // 若没有下载，则直接打开下载面板
                    _loadInfo.gameObject.SetActive(true);
                    _txtLoadProgress.text = "";
                    _txtLoadInfo.text = $"下载Group: {K_GROUP_ISLANDBG_NAME}\n下载Size: {(0.000001f * size)}mb";
                    StartCoroutine(Res.PreloadGroup(K_GROUP_ISLANDBG_NAME,
                        succ =>
                        {
                            _txtLoadProgress.text = $"completed";
                            if (succ)
                            {
                                Debug.Log($"下载完成");
                                _txtLoadInfo.text = "下载完成";
                                _btnLoad3.gameObject.SetActive(true);
                            }
                            else
                            {
                                Debug.Log($"下载失败");
                                _txtLoadInfo.text = "下载失败";
                            }
                        },
                        progress =>
                        {
                            _txtLoadProgress.text = $"{(progress * 100):F}%";
                        }));
                }
            });
        }

        private void OnCloseLoadInfo()
        {
            _loadInfo.gameObject.SetActive(false);
        }
        
        
        //--------------------- 手动触发检查资源列表更新 ------------------------------
        
        /// <summary>
        /// 更新Catalog
        /// </summary>
        private void OnClickPack2()
        {
            _txtLoadProgress.text = "";
            Res.CheckCatalogUpdate(() =>
            {
                if(_loadInfo.activeSelf) _loadInfo.SetActive(false);
            },
            msg =>
            {
                if(!_loadInfo.activeSelf) _loadInfo.SetActive(true);
                _txtLoadInfo.text = msg;
            });
        }
        
        #endregion                                

    }
}
