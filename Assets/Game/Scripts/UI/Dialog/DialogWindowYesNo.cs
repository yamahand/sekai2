using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogWindowYesNo : MonoBehaviour
{
    private void OnEnable()
    {
        // ダイアログが表示されたときに、フォーカスを取得する
        if (_yesButton)
        {
            _yesButton.Select();
        }
    }

    private void OnDisable()
    {
        _yesButton?.onClick.RemoveAllListeners();
        _noButton?.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        // ダイアログを表示する
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        // ダイアログを非表示にする
        gameObject.SetActive(false);
    }

    public void SetMessage(string message)
    {
        // メッセージを設定する
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    // Yesボタンのクリックイベントにリスナーを追加するメソッド
    public void AddYesButtonListener(UnityAction action)
    {
        if (_yesButton != null)
        {
            _yesButton.onClick.AddListener(() => StartCoroutine(OnButtonClicked(action)));
        }
    }

    // Noボタンのクリックイベントにリスナーを追加するメソッド
    public void AddNoButtonListener(UnityAction action)
    {
        if (_noButton != null)
        {
            _noButton.onClick.AddListener(() => StartCoroutine(OnButtonClicked(action)));
        }
    }

    private IEnumerator OnButtonClicked(UnityAction action)
    {
        // アクションを実行
        action.Invoke();

        // アニメーションの終了を待つ（ここでは仮に0.5秒待つ）
        yield return new WaitForSeconds(0.5f);

        // ダイアログを非表示にする
        Hide();
    }

    // TextMesh Proオブジェクト
    [SerializeField]
    private TextMeshProUGUI _messageText = null;
    // Yesボタン
    [SerializeField]
    private Button _yesButton = null;
    // Noボタン
    [SerializeField]
    private Button _noButton = null;
}
