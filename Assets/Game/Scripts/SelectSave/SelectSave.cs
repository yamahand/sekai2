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
        // �_�C�A���O��\������
        DialogWindowManager.Instance.ShowDialog(
            DialogWindowManager.DialogType.YesNo,
            "�Z�[�u���܂����H",
            () =>
        {
            Debug.Log("�Z�[�u���܂���");
        },
            () =>
        {
            Debug.Log("�Z�[�u���܂���ł���");
        });
    }
}
