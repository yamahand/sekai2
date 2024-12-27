using UnityEngine;
using UnityEngine.EventSystems;

public class DialogFocusController : MonoBehaviour
{
    public void SetFirstForcus(GameObject firstButton)
    {
        _firstButton = firstButton;
    }

    void OnEnable()
    {
        // ダイアログが表示された時に最初のボタンにフォーカスを設定
        EventSystem.current?.SetSelectedGameObject(_firstButton);
        _lastSelected = _firstButton;
    }

    void Update()
    {
        // 現在選択されているオブジェクトをチェック
        if (EventSystem.current.currentSelectedGameObject != _lastSelected)
        {
            // 何も選択されていない、もしくは他の場所を選択した場合にフォーカスを戻す
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(_lastSelected);
            }
            else
            {
                // 現在の選択を保持
                _lastSelected = EventSystem.current.currentSelectedGameObject;
            }
        }
    }

    [SerializeField]
    private GameObject _firstButton;  // ダイアログ内で最初にフォーカスするボタン
    private GameObject _lastSelected;  // 最後に選択されていたボタン
}
