using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameManager : MonoBehaviour
{
    private static GameManagerData _loadedDataBuffer;
    private static bool _dataLoaded = false;

    public static GameManager Instance;

    public static int LoopCount = 0;
    public static int CurrentChapter = 1;
    public static bool IsPlayerStop = false;                // 플레이어 행동 제어
    public static bool CanFlash = true;
    public static bool CanSprint = true;
    public static bool IsAnomaly = false;
    public static bool HasBackpack = false;

    [SerializeField] private GameObject _menuUI;
    [SerializeField] private GameObject _inventoryUI;
    [SerializeField] private EventManager _eventManager;
    [SerializeField] private AnomalyManager _anomalyManager;
    [SerializeField] private ToolTipManager _toolTipManager;
    [SerializeField] private ItemSpawnManager _itemSpawnManager;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _endingTransform;

    private Coroutine _soundCoroutine;
    private bool _isInventoryOpen = false;
    private bool _isPause = false;
    private int _normalLoopCount = 0;

    public AudioSource _audioSource;
    public AudioClip InventorySoundClip;

    public List<string> ExperiencedAnomalies = new List<string>();
    public List<string> CollectedKeyItemsLog = new List<string>();

    public float InventorySoundPitch = 1.2f;
    public float OpenSoundStartTime = 0f;
    public float OpenSoundEndTime = 0.9f;
    public float CloseSoundStartTime = 1.4f;
    public float CloseSoundEndTime = 2.2f;
    public bool IsMedicineUsed = false;
    public bool IsEndingReady = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _audioSource = GetComponent<AudioSource>();

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (_dataLoaded) ApplyLoadedData();

        else InitializeNewGame();

        _dataLoaded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsPlayerStop) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (NoteUI.Instance != null && NoteUI.Instance.IsReading)
            {
                NoteUI.Instance.CloseNote();
                return;
            }

            if (_isInventoryOpen) ToggleInventory();

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

    private void InitializeNewGame()
    {
        Debug.Log("새 게임 시작");
        LoopCount = 0;
        CurrentChapter = 1;
        IsAnomaly = false;
        IsPlayerStop = true;
        HasBackpack = false;
        CanSprint = false;

        if (PlayerSanity.Instance != null) PlayerSanity.Instance.InitializeSanity();
    }

    private void ApplyLoadedData()
    {
        Debug.Log($"로드된 데이터 적용 : Loop {LoopCount}, Chapter {CurrentChapter}");

        IsPlayerStop = false;
        CanSprint = true;

        if (_loadedDataBuffer != null)
        {
            if (PlayerSanity.Instance != null)
                PlayerSanity.Instance.LoadSanity(_loadedDataBuffer.CurrentSanity);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.LoadInventory(_loadedDataBuffer.InventoryItemNames);

            if (CameraManager.Instance != null) CameraManager.Instance.SetXRotation(_loadedDataBuffer.CameraXRot);

            if (_loadedDataBuffer.ExperiencedAnomalies != null)
                ExperiencedAnomalies = new List<string>(_loadedDataBuffer.ExperiencedAnomalies);

            if (_loadedDataBuffer.CollectedKeyItems != null)
                CollectedKeyItemsLog = new List<string>(_loadedDataBuffer.CollectedKeyItems);

            StartCoroutine(ApplyPlayerPositionAfterRegistration());
        }

        if (LoopCount >= 2)
        {
            GameObject[] mainLights = GameObject.FindGameObjectsWithTag("MainLight");
            foreach (GameObject light in mainLights) light.SetActive(false);
        }
    }

    private void SaveGameData()
    {
        GameManagerData data = new GameManagerData(this, GetPlayerTransform());
        SaveSystem.SaveGame(data);
    }

    public static void LoadGameDataFromTitle()
    {
        GameManagerData data = SaveSystem.LoadGame();
        if (data != null)
        {
            LoopCount = data.LoopCount;
            CurrentChapter = data.CurrentChapter;
            HasBackpack = data.HasBackpack;

            _loadedDataBuffer = data;
            _dataLoaded = true;
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
        //IsPlayerStop = _isInventoryOpen;
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

    public void DecideNextLoopState()
    {
        CompleteLoop();

        ResetAllObjects();

        if (CurrentChapter >= 4)
        {
            IsAnomaly = false;
            SaveGameData();
            return;
        }

        if (LoopCount == 1) CanSprint = true;

        if (_itemSpawnManager != null) _itemSpawnManager.SpawnItem();

        if (LoopCount < 2)
        {
            IsAnomaly = false;
            _normalLoopCount = 0;
        }

        else
        {
            if (_normalLoopCount >= 2) IsAnomaly = true;

            else IsAnomaly = (Random.value > 0.5f);
        }

        if (IsAnomaly) _normalLoopCount = 0;
        else _normalLoopCount++;

        if (IsAnomaly && _anomalyManager != null) _anomalyManager.TriggerRandomAnomaly();

        SaveGameData();
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

    public void NextChapeter()
    {
        CurrentChapter++;
        Debug.Log("스토리 진행 후 현재 챕터 : " + CurrentChapter);
    }

    public void MoveToEndingRoom()
    {
        
    }

    public bool IsUIOpen()
    {
        return _isPause || _isInventoryOpen;
    }

    public Transform GetPlayerTransform()
    {
        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) _playerTransform = playerObj.transform;
        }

        return _playerTransform;
    }

    public void RegisterMenuUI(GameObject menu)
    {
        _menuUI = menu;
        _menuUI.SetActive(false);
    }

    public void RegisterInventoryUI(GameObject inventory)
    {
        _inventoryUI = inventory;
        _inventoryUI.SetActive(false);
    }

    public void RegisterPlayer(Transform player)
    {
        _playerTransform = player;
    }

    public void RegisterAnomalyManager(AnomalyManager manager)
    {
        _anomalyManager = manager;
    }

    public void RegisterAnomaly(string anomalyName)
    {
        if (!ExperiencedAnomalies.Contains(anomalyName))
        {
            ExperiencedAnomalies.Add(anomalyName);
            Debug.Log($"[System] 새로운 이상 현상 기록됨: {anomalyName}");

            if (AchievementManager.Instance != null)
                AchievementManager.Instance.UnlockAchievement(anomalyName);
        }
    }

    public void RegisterKeyItem(string itemName)
    {
        if (!CollectedKeyItemsLog.Contains(itemName))
        {
            CollectedKeyItemsLog.Add(itemName);
            Debug.Log($"[System] 중요 아이템 획득 기록됨: {itemName}");
        }
    }

    public void RegisterItemSpawnManager(ItemSpawnManager manager)
    {
        _itemSpawnManager = manager;
    }

    public void RegisterToolTipManager(ToolTipManager manager)
    {
        _toolTipManager = manager;
    }

    public void RegisterEventManager(EventManager manager)
    {
        _eventManager = manager;
    }

    private IEnumerator ApplyPlayerPositionAfterRegistration()
    {
        while (_playerTransform == null)
        {
            yield return null;
        }

        if (_loadedDataBuffer.PlayerPosition != null)
        {
            Vector3 pos = new Vector3(_loadedDataBuffer.PlayerPosition[0], _loadedDataBuffer.PlayerPosition[1], _loadedDataBuffer.PlayerPosition[2]);
            Quaternion rot = new Quaternion(_loadedDataBuffer.PlayerRotation[0], _loadedDataBuffer.PlayerRotation[1], _loadedDataBuffer.PlayerRotation[2], _loadedDataBuffer.PlayerRotation[3]);

            CharacterController cc = _playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            _playerTransform.position = pos;
            _playerTransform.rotation = rot;
            if (cc != null) cc.enabled = true;
        }

        _loadedDataBuffer = null;
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
