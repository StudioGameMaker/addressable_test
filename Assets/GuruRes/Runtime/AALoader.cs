using System;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GuruFramework
{
    
    
    /// <summary>
    /// Addressable加载器
    /// </summary>
    public class AALoader
    {


        /// <summary>
        /// 异步加载单个资源
        /// </summary>
        /// <param name="address"></param>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public static void LoadAssetAsync<T>(string address, Action<T> callback)
        {
            Addressables.LoadAssetAsync<T>(address).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    callback?.Invoke(handle.Result);
                }
                else
                {
                    callback?.Invoke(default);
                }
            };
        }

        /// <summary>
        /// 异步加载单个资源
        /// </summary>
        /// <param name="address"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static AsyncOperationHandle<T> LoadAssetTask<T>(string address)
        {
            return Addressables.LoadAssetAsync<T>(address);
        }
        
    }
}