using Cysharp.Threading.Tasks;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            // エディター専用の処理
#if UNITY_EDITOR
            UnityEngine.SceneManagement.SceneManager.LoadScene("DebugSceneSelect");
#else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
#endif
        }
    }
}
