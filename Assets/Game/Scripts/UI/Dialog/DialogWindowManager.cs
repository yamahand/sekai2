using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

public class DialogWindowManager : Singleton<DialogWindowManager>
{
    public enum DialogType
    {
        YesNo,
    }

    public void Initialize()
    {
        Addressables
            .LoadAssetAsync<GameObject>("Assets/Game/Prefabs/UI/DialogWindow/DialogWindowYesNo.prefab") // アドレスを文字列で指定
            .Completed += op =>
            {
                // 結果を取得してインスタンス化
                // 本来はエラーハンドリングなど必要
                GameObject dialogWindowObject = Instantiate(op.Result, transform);
                _dialogWindowYesNo = dialogWindowObject.GetComponent<DialogWindowYesNo>();

                if (_dialogWindowYesNo == null)
                {
                    Debug.LogError("DialogWindowYesNoコンポーネントが見つかりません。");
                }
                else { 
                    _dialogWindowYesNo.gameObject.SetActive(false);
                }
            };
    }

    public void ShowDialog(DialogType dialogType, string text, UnityAction button1 = null, UnityAction button2 = null)
    {
        switch (dialogType)
        {
            case DialogType.YesNo:
                _dialogWindowYesNo.SetMessage(text);
                _dialogWindowYesNo.AddYesButtonListener(button1);
                _dialogWindowYesNo.AddNoButtonListener(button2);
                _dialogWindowYesNo.Show();
                break;
        }
    }

    [SerializeField]
    private DialogWindowYesNo _dialogWindowYesNo;
}
