using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public class UIRoot: MonoBehaviour
    {

        [SerializeField] private UILoadingBar _loadingBar;
        [SerializeField] private RectTransform _root;
        
        private ResMgr Res => ResMgr.Instance;

        private static UIRoot _instance;
        public static UIRoot Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = Instantiate(Resources.Load<GameObject>("UI/UIRoot"));
                    if (go != null)
                    {
                        _instance = go.GetComponent<UIRoot>();
                    }
                }

                return _instance;
            }
        }




        private void Awake()
        {
            gameObject.name = "UIRoot";
            DontDestroyOnLoad(gameObject);
            ShowLoading(false);
        }


        public void ShowLoading(bool flag = true)
        {
            _loadingBar.gameObject.SetActive(flag);
        }

        public void LoadingValue(float p)
        {
            ShowLoading();
            _loadingBar.Value = p;
        }
        
        /// <summary>
        /// 打开UI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T OpenView<T>(string address ) where T : Component
        {
            var ui = Res.GetInstance<T>(address, _root);
            if (ui != null)
            {
                ui.name = typeof(T).Name;
                return ui;
            }
            return null;
            
        }
        
    }
}