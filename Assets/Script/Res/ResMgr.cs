using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Game
{
    public class ResMgr : MonoBehaviour
    {

        private static ResMgr _instance;

        public static ResMgr Instance => _instance;
       

        private void Awake()
        {
            _instance = this;
            _cachedMaps = new Dictionary<string, ResMap>(10);
        }

 
        #region Addressable

        /// <summary>
        /// 设置好的预加载组
        /// </summary>
        private string[] _preloadGroups = new string[]
        {
            "common",
            "game",
            //TODO 可以向后扩展需要预加载的组
        };

        private Dictionary<string, ResMap> _cachedMaps;
        private Dictionary<string, Sprite> _cacheSprites;

        public AssetReference assetSprite;

        public void StartBigSpriteLoad(Action onComplete)
        {
            Addressables.LoadAssetsAsync<Sprite>(new List<object> {"island_back_ground"}, null, Addressables.MergeMode.Union)
                .Completed += handle =>
            {
                for (int i = 0; i < handle.Result.Count; i++)
                {
                    Sprite sprite = handle.Result[i];
                    _cacheSprites[sprite.name] = sprite;
                }

                onComplete?.Invoke();
            };
        }

        /// <summary>
        /// 预加载内置组
        /// </summary>
        /// <param name="onComplete"></param>
        public IEnumerator StartPreLoad(Action onComplete, Action<float> onLoading = null)
        {
            int count = _preloadGroups.Length;
            for (int i = 0; i < count; i++)
            {
                var key = _preloadGroups[i];
                var handle = Addressables.LoadAssetsAsync<Object>(key, null);
                yield return handle;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("star-----> 加载资源 = group = " + key);
                    var map = new ResMap(key, handle.Result);
                    _cachedMaps[key] = map;
                }
                else
                {
                    Debug.Log($"<color=red>加载错误 [{handle.Status}]: {key}</color>");
                }

                Debug.Log("star-----> 加载资源 = " + i);
                onLoading?.Invoke((float) i / count);
            }

            onComplete?.Invoke();
        }


        /// <summary>
        /// 异步加载Group
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public void LoadGroupAsync(string key, Action<bool> callback)
        {
            Addressables.LoadAssetsAsync<Object>(key, null).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result is IList<Object> list)
                {
                    var map = new ResMap(key, list);
                    _cachedMaps[key] = map;
                    callback?.Invoke(true);
                }
                else
                {
                    Debug.Log($"<color=red>加载错误 [{handle.Status}]: {key}</color>");
                    callback?.Invoke(false);
                }
            };
        }

        /// <summary>
        /// 组是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool MapExists(string name)
        {
            return _cachedMaps.ContainsKey(name);
        }


        /// <summary>
        /// 从缓存加载组件
        /// </summary>
        /// <param name="key"></param>
        /// <param name="path"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject LocalLoad<TObject>(string key, string path) where TObject : Object
        {
            if (MapExists(key))
            {
                return _cachedMaps[key].Find<TObject>(path);
            }
            return default;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="address"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public TObject LocalLoad<TObject>(string address) where TObject : Object
        {
            AddressToKeyAndPath(address, out var key, out var path);
            return LocalLoad<TObject>(key, path);
        }

        /// <summary>
        /// 直接实例化对象
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetInstance<T>(string address, Transform parent = null) where T : Component
        {
            var prefab = LocalLoad<GameObject>(address);
            if (prefab != null)
            {
                var obj = Object.Instantiate(prefab, parent);
                return obj.GetComponent<T>();
            }
            return null;
        }


        public Sprite LocalBigSprite(string name)
        {
            if (_cacheSprites.ContainsKey(name))
            {
                return _cacheSprites[name];
            }

            return default;
        }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address"></param>
        /// <param name="group"></param>
        /// <param name="path"></param>
        private void AddressToKeyAndPath(string address, out string key, out string path)
        {
            key = "";
            path = "";
            int start = 0;
            int end = address.IndexOf("/", StringComparison.Ordinal);
            key = address.Substring(start, end);
            start = address.LastIndexOf("/", StringComparison.Ordinal) + 1;
            end = address.Length - start;
            path = address.Substring(start, end);
        }

        /// <summary>
        /// 加载图集
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public SpriteAtlas LoadSpriteAtlas(string address)
        {
            return LocalLoad<SpriteAtlas>(address);
        }

        /// <summary>
        /// 从Atlas内加载Sprite
        /// </summary>
        /// <param name="atlasAddress"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Sprite LoadSpriteFromAtlas(string atlasAddress, string name)
        {
            var atlas = LoadSpriteAtlas(atlasAddress);
            if (atlas != null)
            {
                return atlas.GetSprite(name);
            }

            return null;
        }

        public Texture2D LoadTexture2D(string address)
        {
            return LocalLoad<Texture2D>(address);
        }


        public GameObject LoadObject(string address)
        {
            return LocalLoad<GameObject>(address);
        }


        public Sprite LoadSprite(string address)
        {
            var tx = LoadTexture2D(address);
            if (tx != null)
            {
                return Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), Vector2.zero);
            }
            return null;
        }



        #endregion
            
            


        
        
        


    }
    
    #region 本地资源缓存

    /// <summary>
    /// 资源Map
    /// 对应每个已加载的Group
    /// </summary>
    internal class ResMap
    {
        public string name;
        public List<Object> objects;

        public ResMap(string key, IList<Object> raw = null)
        {
            name = key;
            if (null != raw)
            {
                objects = raw.ToList();
            }
            else
            {
                objects = new List<Object>();
            }
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Find<T>(string name)where T: Object
        {
            return (T) objects.Find(c => c.name == name && (T) c != null);
        }
    }

    #endregion
}