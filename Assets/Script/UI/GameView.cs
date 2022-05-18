using System;
using System.Collections.Generic;
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
        [SerializeField] private Image _image;
        [SerializeField] private Button _btnLoad1;
        [SerializeField] private Button _btnLoad2;
        [SerializeField] private Button _btnLoad3;
        [SerializeField] private Image _islandBG;

        private void Awake()
        {
            _islandBG.gameObject.SetActive(false);
            _btnLoad1.onClick.AddListener(LoadObject1);
            _btnLoad2.onClick.AddListener(LoadObject2);
            _btnLoad3.onClick.AddListener(LoadObject3);
            
            Debug.Log("-------------- GameView Awake ---------------");
        }
        
        /// <summary>
        /// 检测组是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        private void LoadGroup(string key, Action callback)
        {
            if (ResMgr.Instance.MapExists(key))
            {
                callback?.Invoke(); 
            }
            else
            {
                ResMgr.Instance.LoadGroupAsync(key, success =>
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
            var sp = ResMgr.Instance.LoadSpriteFromAtlas("remote/atlas_shop", "ic_shop_more");
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
            var obj = ResMgr.Instance.GetInstance<TestObject>("remote/test_object", transform);
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
            LoadGroup("island_bg", SetIslandBG);
        }
        
        /// <summary>
        /// 加载岛屿背景, 直接加载位图
        /// </summary>
        private void SetIslandBG()
        {
            var address = "island_bg/img_map_island_1";
            var sp = ResMgr.Instance.LoadSprite(address);
            if (sp != null)
            {
                _islandBG.gameObject.SetActive(true);
                _islandBG.sprite = sp;
                _islandBG.SetNativeSize();
                // var sn = _islandBG.material.shader.name;
                // _islandBG.material.shader = Shader.Find(sn); // 重新载入Shader
            }
        }

    }
}