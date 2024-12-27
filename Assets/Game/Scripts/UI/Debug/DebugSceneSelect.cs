using UnityEngine;
using UnityEngine.UIElements;

public class DebugSceneSelect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("button_title").clicked += () =>
        {
            // タイトルシーンをロード
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
        };
        root.Q<Button>("button_exp").clicked += () =>
        {
            // ゲームシーンをロード
            UnityEngine.SceneManagement.SceneManager.LoadScene("Exploration");
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
