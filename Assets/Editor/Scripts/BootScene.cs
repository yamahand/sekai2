using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class BootScene : MonoBehaviour
{
    // 現在のシーン名を保存する変数
    private static string _previousSceneKey = "BootScene_PreviousScene";

    // 静的コンストラクタでイベントリスナーを登録
    static BootScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // UnityEditorのメニューにBootメニューを追加
    [MenuItem("BootScene/Boot")]
    private static void BootScene_Boot()
    {
        Boot("Assets/Game/Scenes/Boot.unity");
    }
    [MenuItem("BootScene/Title")]
    private static void BootScene_Title()
    {
        Boot("Assets/Game/Scenes/Title.unity");
    }

    private static void Boot(string scenePath)
    {
        // 現在のシーンを保存
        string currentScenePath = SceneManager.GetActiveScene().path;
        EditorPrefs.SetString(_previousSceneKey, currentScenePath);

        // Bootシーンをロード
        if (System.IO.File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);

            // Playモードを開始
            EditorApplication.isPlaying = true;
        }
        else
        {
            Debug.LogError($"シーンファイルが見つかりません: {scenePath}");
        }
    }

    private static void ReturnToPreviousScene()
    {
        string previousScene = EditorPrefs.GetString(_previousSceneKey, string.Empty);
        if (!string.IsNullOrEmpty(previousScene))
        {
            // 元のシーンに戻る
            EditorSceneManager.OpenScene(previousScene);
        }
    }

    // Playモードの状態が変更されたときに呼ばれるメソッド
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Playモードが終了したとき
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            ReturnToPreviousScene();
            EditorPrefs.DeleteKey(_previousSceneKey);
        }
    }
}

