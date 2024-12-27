using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class BootScene : MonoBehaviour
{
    // ���݂̃V�[������ۑ�����ϐ�
    private static string _previousSceneKey = "BootScene_PreviousScene";

    // �ÓI�R���X�g���N�^�ŃC�x���g���X�i�[��o�^
    static BootScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    // UnityEditor�̃��j���[��Boot���j���[��ǉ�
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
        // ���݂̃V�[����ۑ�
        string currentScenePath = SceneManager.GetActiveScene().path;
        EditorPrefs.SetString(_previousSceneKey, currentScenePath);

        // Boot�V�[�������[�h
        if (System.IO.File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);

            // Play���[�h���J�n
            EditorApplication.isPlaying = true;
        }
        else
        {
            Debug.LogError($"�V�[���t�@�C����������܂���: {scenePath}");
        }
    }

    private static void ReturnToPreviousScene()
    {
        string previousScene = EditorPrefs.GetString(_previousSceneKey, string.Empty);
        if (!string.IsNullOrEmpty(previousScene))
        {
            // ���̃V�[���ɖ߂�
            EditorSceneManager.OpenScene(previousScene);
        }
    }

    // Play���[�h�̏�Ԃ��ύX���ꂽ�Ƃ��ɌĂ΂�郁�\�b�h
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // Play���[�h���I�������Ƃ�
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            ReturnToPreviousScene();
            EditorPrefs.DeleteKey(_previousSceneKey);
        }
    }
}

