using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NoteUI : MonoBehaviour
{
    public static NoteUI Instance;

    private int _currentPage = 0;
    private bool _isReading = false;
    private List<string> _currentNote;

    public GameObject NotePannel;
    public Text LeftPageText;
    public Text RightPageText;
    public Button PrevButton;
    public Button NextButton;
    public Button CloseButton;

    public bool IsReading => _isReading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (PrevButton != null) PrevButton.onClick.AddListener(PrevPage);
        if (NextButton != null) NextButton.onClick.AddListener(NextPage);
        if (CloseButton != null) CloseButton.onClick.AddListener(CloseNote);
        if (NotePannel != null) NotePannel.SetActive(false);
    }

    private void UpdateNoteDisplay()
    {
        if (_currentPage < _currentNote.Count) LeftPageText.text = _currentNote[_currentPage];
        else LeftPageText.text = "";

        if (_currentPage + 1 < _currentNote.Count) RightPageText.text = _currentNote[_currentPage + 1];
        else RightPageText.text = "";

        if (PrevButton != null) PrevButton.gameObject.SetActive(_currentPage > 0);
        if (NextButton != null) NextButton.gameObject.SetActive(_currentPage + 2 < _currentNote.Count);
    }

    public void ShowNote(List<string> pages)
    {
        _currentNote = pages;
        _isReading = true;
        GameManager.IsPlayerStop = true;
        NotePannel.SetActive(true);
        _currentPage = 0;
        UpdateNoteDisplay();
        Cursor.lockState = CursorLockMode.None;     // 정지해도 마우스 커서 고정 해제
        Cursor.visible = true;
    }

    public void CloseNote()
    {
        _isReading = false;
        NotePannel.SetActive(false);
        GameManager.IsPlayerStop = false;
        Cursor.lockState = CursorLockMode.Locked;   // 게임 시작 시, 마우스 커서 숨기기
        Cursor.visible = false;
    }

    public void NextPage()
    {
        if (_currentNote != null && _currentPage + 2 < _currentNote.Count)
        {
            _currentPage += 2;
            UpdateNoteDisplay();
        }
    }

    public void PrevPage()
    {
        if (_currentPage - 2 >= 0)
        {
            _currentPage -= 2;
            UpdateNoteDisplay();
        }
    }
}
