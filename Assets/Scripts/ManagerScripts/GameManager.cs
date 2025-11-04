using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static int LoopCount = 0;
    public static int CurrentChapter = 1;
    public static bool IsPlayerStop = false;                // 플레이어 행동 제어
    public static bool IsAnomaly = false;
    public static bool HasBackpack = false;

    [SerializeField] private GameObject _menuUI;
    [SerializeField] private GameObject _inventoryUI;
    [SerializeField] private AnomalyManager _anomalyManager;
    [SerializeField] private ToolTipManager _toolTipManager;
    private bool _isPause = false;
    private bool _isInventoryOpen = false;

    private Coroutine _soundCoroutine;
    private AudioSource _audioSource;
    public AudioClip InventorySoundClip;
    public float InventorySoundPitch = 1.2f;

    public float OpenSoundStartTime = 0f;
    public float OpenSoundEndTime = 0.9f;
    public float CloseSoundStartTime = 1.4f;
    public float CloseSoundEndTime = 2.2f;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isInventoryOpen)
            {

                ToggleInventory();
            }

            else
            {
                if (_isPause) CloseMenu();

                else OpenMenu();
            }
        }

        if (HasBackpack && !_isPause && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))) 
        {
            ToggleInventory();
        }
    }

    public void OpenMenu()
    {
        _menuUI.SetActive(true);
        Time.timeScale = 0f;        // 정지
        _isPause = true;
        Cursor.lockState = CursorLockMode.None;     // 정지해도 마우스 커서 고정 해제
        Cursor.visible = true;                      // 조작은 해야 하니 마우스 커서는 보이게
    }

    public void CloseMenu()
    {
        _menuUI.SetActive(false);
        Time.timeScale = 1f;        // 정지 해제
        _isPause = false;
        Cursor.lockState = CursorLockMode.Locked;   // 게임 시작 시, 마우스 커서 숨기기
        Cursor.visible= false;
    }

    public void ToggleInventory()
    {
        if (_soundCoroutine != null) StopCoroutine(_soundCoroutine);
        _audioSource.Stop();

        _isInventoryOpen = !_isInventoryOpen;

        _inventoryUI.SetActive(_isInventoryOpen);
        IsPlayerStop = _isInventoryOpen;
        _audioSource.pitch = InventorySoundPitch;

        if (_isInventoryOpen)
        {
            _soundCoroutine = StartCoroutine(PlaySoundSegment(InventorySoundClip, OpenSoundStartTime, OpenSoundEndTime));
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        else
        {
            _soundCoroutine = StartCoroutine(PlaySoundSegment(InventorySoundClip, CloseSoundStartTime, CloseSoundEndTime));

            if (_toolTipManager != null) _toolTipManager.HideToolTip();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ResetAllObjects()
    {
        IResetable[] resetableObjects = FindObjectsOfType<MonoBehaviour>().OfType<IResetable>().ToArray();

        foreach (IResetable obj in resetableObjects)
        {
            obj.ResetState();
        }

        Debug.Log(resetableObjects.Length + "개의 오브젝트 초기화");
    }

    public void CompleteLoop()
    {
        LoopCount++;
        Debug.Log("누적 루프 횟수 : " + LoopCount);
    }

    public void DecideNextLoopState()
    {
        CompleteLoop();

        if (LoopCount < 2) IsAnomaly = false;

        else IsAnomaly = (Random.value > 0.1f);

        ResetAllObjects();

        _anomalyManager.TriggerRandomAnomaly();
    }

    public void NextChapeter()
    {
        CurrentChapter++;
        Debug.Log("스토리 진행 후 현재 챕터 : " + CurrentChapter);
    }
    
    private IEnumerator PlaySoundSegment(AudioClip clip, float startTime, float endTime)
    {
        _audioSource.clip = clip;
        _audioSource.time = startTime;
        _audioSource.Play();

        yield return new WaitForSeconds(endTime - startTime);

        _audioSource.Stop();
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
