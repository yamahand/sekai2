using Cysharp.Threading.Tasks;
using UnityEngine;

public class Boot : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnAppStart()
    {
        // ゲームマネージャー初期化
        GameManager.Instance.InitializeGame();
    }

    // Update is called once per frame
    void Update()
    {
        if( GameManager.Instance.isInitialized)
        {
            // ゲーム初期化完了後、このシーンをアンロード
            UnityEngine.SceneManagement.SceneManager.LoadScene("DebugSceneSelect");
        }
    }

}
