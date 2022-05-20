using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        // yield return Addressables.InitializeAsync();
        Debug.Log("-------------- StartPreLoading ---------------");
        yield return Res.StartPreLoad(OnReloadComplete, UI.LoadingValue);
    }

    private void OnReloadComplete()
    {
        UI.ShowLoading(false);
        Debug.Log($"<color=#88ff00>===== 预加载结束 =====</color>");
        // 显示主界面
        var gameView = UI.LoadView<GameView>(Const.ADDRESS_GAME_VIEW);
    }
}
