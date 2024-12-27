using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogWindowYesNo : MonoBehaviour
{
    private void OnEnable()
    {
        // �_�C�A���O���\�����ꂽ�Ƃ��ɁA�t�H�[�J�X���擾����
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
        // �_�C�A���O��\������
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        // �_�C�A���O���\���ɂ���
        gameObject.SetActive(false);
    }

    public void SetMessage(string message)
    {
        // ���b�Z�[�W��ݒ肷��
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    // Yes�{�^���̃N���b�N�C�x���g�Ƀ��X�i�[��ǉ����郁�\�b�h
    public void AddYesButtonListener(UnityAction action)
    {
        if (_yesButton != null)
        {
            _yesButton.onClick.AddListener(() => StartCoroutine(OnButtonClicked(action)));
        }
    }

    // No�{�^���̃N���b�N�C�x���g�Ƀ��X�i�[��ǉ����郁�\�b�h
    public void AddNoButtonListener(UnityAction action)
    {
        if (_noButton != null)
        {
            _noButton.onClick.AddListener(() => StartCoroutine(OnButtonClicked(action)));
        }
    }

    private IEnumerator OnButtonClicked(UnityAction action)
    {
        // �A�N�V���������s
        action.Invoke();

        // �A�j���[�V�����̏I����҂i�����ł͉���0.5�b�҂j
        yield return new WaitForSeconds(0.5f);

        // �_�C�A���O���\���ɂ���
        Hide();
    }

    // TextMesh Pro�I�u�W�F�N�g
    [SerializeField]
    private TextMeshProUGUI _messageText = null;
    // Yes�{�^��
    [SerializeField]
    private Button _yesButton = null;
    // No�{�^��
    [SerializeField]
    private Button _noButton = null;
}
