using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class Entry : MonoBehaviour
{

    private UIRoot UI;
    private ResMgr Res;
    
    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("-------------- Entry Start ---------------");
        DontDestroyOnLoad(gameObject);

        Res = gameObject.AddComponent<ResMgr>();
        UI = UIRoot.Instance;
        
        StartCoroutine(nameof(StartPreLoading));
    }


    IEnumerator StartPreLoading()
    {
        // 开始预加载
        UI.LoadingValue(0);
        yield return Addressables.InitializeAsync();
        Debug.Log("-------------- StartPreLoading ---------------");
        yield return Res.StartPreLoad(OnPreloadComplete, UI.LoadingValue);
        yield return null;
    }

    private void OnPreloadComplete()
    {
        UI.ShowLoading(false);
        Debug.Log($"<color=#88ff00>===== 预加载结束 =====</color>");
        // 显示主界面
        var gameView = UI.LoadView<GameView>(Const.ADDRESS_GAME_VIEW);

        // LoadGameView();


    }

    private void LoadGameView()
    {
        StartCoroutine(nameof(OnTestLoading));

    }

    private IEnumerator OnTestLoading()
    {
        string addr = "common/atlas_common";
        var handle = Addressables.LoadAssetAsync<SpriteAtlas>(addr);
        yield return handle;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"-------- Load {addr} Success -----------");
        }
        addr = "game/game_view";
        var ao2 = Addressables.LoadAssetAsync<GameObject>(addr);
        yield return ao2;
        if (ao2.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"-------- Load {addr} Success -----------");
            GameObject prefab = ao2.Result as GameObject;
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, UI.transform);
                var ui = go.GetComponent<GameView>();
                    
            }
            
        }
        
        
    }
    
    
    
}
