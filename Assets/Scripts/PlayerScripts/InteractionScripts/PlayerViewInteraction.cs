using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerViewInteraction : MonoBehaviour
{
    private Camera _mainCamera;
    private Light _flashLight;

    private AudioSource _audioSource;
    public AudioClip[] AudioClips;
    public float FlashSoundPitch = 1.2f;

    public float InteractionDistance = 3f;
    public LayerMask InteractionLayerMask;

    public Transform PlayerTransform;
    public Transform SpawnTransform;

    public Image FadeImage;
    public float FadeDuration = 1f;

    public Item TutorialNoteItem;
    public GameObject StairBlockWall;
    public GameObject TutorialNote;
    public GameObject OnDeskObject;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GetComponent<Camera>();
        _flashLight = GetComponent<Light>();
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null )
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        if (GameManager.Instance != null && PlayerTransform != null)
            GameManager.Instance.RegisterPlayer(PlayerTransform);

        if (GameManager.LoopCount <= 1)
        {
            if (GameManager.LoopCount == 0)
            {
                if (OnDeskObject != null) OnDeskObject.SetActive(true);
            }
            if (GameManager.LoopCount == 1)
            {
                GameManager.CanSprint = true;
                if (OnDeskObject != null) OnDeskObject.SetActive(!GameManager.HasBackpack);
            }
            if (StairBlockWall != null) StairBlockWall.SetActive(true);
            if (TutorialNote != null) TutorialNote.SetActive(false);
        }

        else
        {
            GameManager.CanSprint = true;
            if (OnDeskObject != null) OnDeskObject.SetActive(!GameManager.HasBackpack);
            if (StairBlockWall != null) StairBlockWall.SetActive(false);
            if (TutorialNote != null)
            {
                bool hasNote = InventoryManager.Instance.HasItem(TutorialNoteItem);
                TutorialNote.SetActive(GameManager.LoopCount == 2 && !hasNote);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsPlayerStop || GameManager.Instance.IsUIOpen()) return;

        if (GameManager.LoopCount >= 2 && !GameManager.Instance.IsUIOpen() && Input.GetMouseButtonDown(1))
        {
            if (_flashLight != null)
            {
                _flashLight.enabled = !_flashLight.enabled;
                _audioSource.pitch = FlashSoundPitch;

                if (_flashLight.enabled) _audioSource.PlayOneShot(AudioClips[0]);

                else _audioSource.PlayOneShot(AudioClips[1]);
            }
        }

        // 메인 카메라 정중앙에서 앞쪽으로 raycast == 플레이어가 바라보는 방향으로 raycast
        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, InteractionDistance, InteractionLayerMask))
        {
            if (Input.GetKeyDown(KeyCode.F)) 
            { 
                if (hit.collider.tag == "Backpack")
                {
                    if (!GameManager.HasBackpack) StartCoroutine(BackpackPickupEvent(hit.collider.gameObject));
                    return;
                }

                ItemPickup itemPickup = hit.collider.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    if (InventoryManager.Instance.Add(itemPickup.item)) Destroy(hit.collider.gameObject);
                    return;
                }

                DoorInteraction door = hit.collider.GetComponent<DoorInteraction>();
                if (door != null && door.enabled) 
                { 
                    door.ToggleDoor();
                    return;
                }

                CabinetInteraction cabinet = hit.collider.GetComponent<CabinetInteraction>();
                if (cabinet != null)
                {
                    cabinet.ToggleCabinet();
                    return;
                }

                if (hit.collider.tag == "Interaction")
                {
                    string objectName = hit.collider.name;

                    if (objectName.Contains("LeftExit") || objectName.Contains("Stairs")) CheckChoice(objectName);
                }
            }
        }
    }

    private void CheckChoice(string objName)
    {
        if (!GameManager.IsAnomaly)
        {
            if (objName.Contains("LeftExit"))
            {
                bool hasRequiredItems = InventoryManager.Instance.HasAllRequiredItemsForCurrentChapter();

                if (hasRequiredItems) TriggerStoryEvent();

                else
                {
                    _audioSource.pitch = 1.2f;
                    _audioSource.PlayOneShot(AudioClips[2]);
                    StartCoroutine(TeleportPlayer());
                }
            }

            else if (objName.Contains("Stairs"))
            {
                _audioSource.pitch = 1.45f;
                _audioSource.PlayOneShot(AudioClips[3]);
                DecreaseSanity();
                StartCoroutine(TeleportPlayer());
            }
        }

        else
        {
            if (objName.Contains("Stairs"))
            {
                _audioSource.pitch = 1.45f;
                _audioSource.PlayOneShot(AudioClips[3]);
                StartCoroutine(TeleportPlayer());
            }

            else if (objName.Contains("LeftExit"))
            {
                DecreaseSanity();
                StartCoroutine(HorrorEventAndTeleport());
            }
        }
    }

    private void TriggerStoryEvent()
    {
        Debug.Log("필수 아이템 전부 입수 -> " + GameManager.CurrentChapter + "챕터 스토리 시작");

        GameManager.Instance.NextChapeter();

        StartCoroutine(TeleportPlayer());
    }

    private void DecreaseSanity()
    {
        Debug.Log("정신력 감소");
        PlayerSanity.Instance.DecreaseSanity(20f);
    }

    private IEnumerator TeleportPlayer()
    {
        GameManager.IsPlayerStop = true;        // 이동하는 동안 플레이어 정지

        yield return StartCoroutine(EventManager.Instance.Fade(false, FadeDuration));

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = SpawnTransform.position;
        PlayerTransform.rotation = SpawnTransform.rotation;
        cc.enabled = true;

        GameManager.Instance.DecideNextLoopState();

        if (GameManager.LoopCount == 2)
        {
            if (TutorialNote != null) TutorialNote.SetActive(true);
            if (StairBlockWall != null) StairBlockWall.SetActive(false);

            GameObject[] mainLights = GameObject.FindGameObjectsWithTag("MainLight");
            foreach (GameObject light in mainLights) light.SetActive(false);

            EventManager.Instance.UpdateObjective("");
        }

        yield return StartCoroutine(EventManager.Instance.Fade(true, FadeDuration));

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator HorrorEventAndTeleport()
    {
        GameManager.IsPlayerStop = true;

        yield return StartCoroutine(EventManager.Instance.Fade(false, FadeDuration));

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = SpawnTransform.position;
        PlayerTransform.rotation = SpawnTransform.rotation;
        cc.enabled = true;

        GameManager.Instance.DecideNextLoopState();

        yield return StartCoroutine(EventManager.Instance.Fade(true, FadeDuration));

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator BackpackPickupEvent(GameObject backpack)
    {
        GameManager.IsPlayerStop = true;

        yield return StartCoroutine(EventManager.Instance.Fade(false, 1f));

        _audioSource.pitch = 0.8f;
        _audioSource.PlayOneShot(AudioClips[4]);

        GameManager.HasBackpack = true;

        if (OnDeskObject != null) OnDeskObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        EventManager.Instance.UpdateObjective("왼쪽 출입구로 나가기");
        EventManager.Instance.ShowSubtitle("이제 건물 밖으로 나가자.", 3f);

        yield return StartCoroutine(EventManager.Instance.Fade(true, 1f));

        GameManager.IsPlayerStop = false;
    }
}
