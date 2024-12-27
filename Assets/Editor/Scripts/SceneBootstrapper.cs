using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class SceneBootstrapper
{
    // EditorPrefsキーの定義
    private const string _previousScene = "PreviousScene";
    private const string _shouldLoadBootstrap = "LoadBootstrapScene";

    // メニュー項目の定義
    private const string _loadBootstrapMenu = "Sekai/Load Bootstrap Scene On Play";
    private const string _dontLoadBootstrapMenu = "Sekai/Don't Load Bootstrap Scene On Play";

    // ブートストラップシーンのパスを取得
    private static string bootstrapScene => EditorBuildSettings.scenes[0].path;

    // 前回のシーンのパスを取得または設定
    private static string previousScene
    {
        get => EditorPrefs.GetString(_previousScene);
        set => EditorPrefs.SetString(_previousScene, value);
    }

    // ブートストラップシーンをロードするかどうかのフラグを取得または設定
    private static bool shouldLoadBootstrapScene
    {
        get => EditorPrefs.GetBool(_shouldLoadBootstrap, true);
        set => EditorPrefs.SetBool(_shouldLoadBootstrap, value);
    }

    // 静的コンストラクタでプレイモードの状態変更イベントを登録
    static SceneBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // プレイモードの状態が変更されたときに呼び出されるメソッド
    private static void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
    {
        if (!shouldLoadBootstrapScene)
        {
            return;
        }

        switch (playModeStateChange)
        {
            case PlayModeStateChange.ExitingEditMode:
                // エディットモードを終了する前に現在のシーンを保存し、ブートストラップシーンを開く
                previousScene = EditorSceneManager.GetActiveScene().path;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() && IsSceneInBuildSettings(bootstrapScene))
                {
                    EditorSceneManager.OpenScene(bootstrapScene);
                }
                break;

            case PlayModeStateChange.EnteredEditMode:
                // エディットモードに戻ったときに前回のシーンを再度開く
                if (!string.IsNullOrEmpty(previousScene))
                {
                    EditorSceneManager.OpenScene(previousScene);
                }
                break;
        }
    }

    // ブートストラップシーンをロードするメニュー項目
    [MenuItem(_loadBootstrapMenu)]
    private static void EnableBootstrapper()
    {
        shouldLoadBootstrapScene = true;
    }

    // ブートストラップシーンをロードするメニュー項目の有効化/無効化を制御
    [MenuItem(_loadBootstrapMenu, true)]
    private static bool ValidateEnableBootstrapper()
    {
        return !shouldLoadBootstrapScene;
    }

    // ブートストラップシーンをロードしないメニュー項目
    [MenuItem(_dontLoadBootstrapMenu)]
    private static void DisableBootstrapper()
    {
        shouldLoadBootstrapScene = false;
    }

    // ブートストラップシーンをロードしないメニュー項目の有効化/無効化を制御
    [MenuItem(_dontLoadBootstrapMenu, true)]
    private static bool ValidateDisableBootstrapper()
    {
        return shouldLoadBootstrapScene;
    }

    // シーンがビルド設定に含まれているかどうかを確認するメソッド
    private static bool IsSceneInBuildSettings(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
            return false;

        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.path == scenePath)
            {
                return true;
            }
        }

        return false;
    }
}
