using UnityEngine;

public class SelectSave : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnClick()
    {
        // ダイアログを表示する
        DialogWindowManager.Instance.ShowDialog(
            DialogWindowManager.DialogType.YesNo,
            "セーブしますか？",
            () =>
        {
            Debug.Log("セーブしました");
        },
            () =>
        {
            Debug.Log("セーブしませんでした");
        });
    }
}
