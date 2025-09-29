using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuUI;

    private bool isPause = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause) CloseMenu();

            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        _menuUI.SetActive(true);
        Time.timeScale = 0f;        // ����
        isPause = true;
        Cursor.lockState = CursorLockMode.None;     // �����ص� ���콺 Ŀ�� ���� ����
        Cursor.visible = true;                      // ������ �ؾ� �ϴ� ���콺 Ŀ���� ���̰�
    }

    public void CloseMenu()
    {
        _menuUI.SetActive(false);
        Time.timeScale = 1f;        // ���� ����
        isPause = false;
        Cursor.lockState = CursorLockMode.Locked;   // ���� ���� ��, ���콺 Ŀ�� �����
        Cursor.visible= false;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
