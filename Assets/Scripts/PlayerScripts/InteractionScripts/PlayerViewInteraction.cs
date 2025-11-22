using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class PlayerViewInteraction : MonoBehaviour
{
    private Camera _mainCamera;
    private Light _flashLight;
    private bool _CheckedLockedDoor = false;

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

    public ChapterCutScene Chapter1Cut;
    public ChapterCutScene Chapter2Cut;
    public ChapterCutScene Chapter3Cut;
    public string EndingScene = "EndingScene";

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
                    if (itemPickup.item is NoteItem noteItem)
                    {
                        if (NoteUI.Instance != null) NoteUI.Instance.ShowNote(noteItem.pages);
                    }

                    itemPickup.Pickup();
                    return;
                }

                DoorInteraction door = hit.collider.GetComponent<DoorInteraction>();
                if (door != null && door.enabled) 
                { 
                    if (GameManager.LoopCount == 0 && !GameManager.HasBackpack)
                    {
                        EventManager.Instance.ShowSubtitle("짐이랑 가방을 챙기고 돌아가야지. 그냥 갈 수는 없지..", 2f);
                        return;
                    }

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

                    if (GameManager.LoopCount == 1 && !_CheckedLockedDoor && objectName.Contains("RightExit"))
                    {
                        StartCoroutine(LockedDoorSequence());
                        return;
                    }

                    else if (GameManager.LoopCount == 1 && _CheckedLockedDoor && objectName.Contains("RightExit"))
                    {
                        _audioSource.PlayOneShot(AudioClips[5], 6f);
                        return;
                    }

                    if (objectName.Contains("LeftExit") || objectName.Contains("Stairs")) CheckChoice(objectName);
                }
            }
        }
    }

    private void CheckChoice(string objName)
    {
        bool hasRequiredItems = InventoryManager.Instance.HasAllRequiredItemsForCurrentChapter();

        if (hasRequiredItems)
        {
            if (objName.Contains("LeftExit"))
            {
                TriggerStoryEvent();
                return;
            }
        }

        if (!GameManager.IsAnomaly)
        {
            if (objName.Contains("LeftExit"))
            {
                _audioSource.pitch = 1.2f;
                _audioSource.PlayOneShot(AudioClips[2]);
                StartCoroutine(TeleportPlayer());
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

        StartCoroutine(PlayStorySequence());
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

        if (GameManager.LoopCount == 1)
        {
            EventManager.Instance.ShowSubtitle("...어라? 난 분명 문을 열고 밖으로 나왔는데", 3f);
            yield return new WaitForSeconds(3f);
            EventManager.Instance.ShowSubtitle("어째서 다시 건물 내부로 들어온 거지..?", 3f);
            yield return new WaitForSeconds(1.5f);

            EventManager.Instance.UpdateObjective("들어온 문 확인하기");
        }

        else if (GameManager.LoopCount == 2)
        {
            yield return new WaitForSeconds(1f);

            EventManager.Instance.ShowSubtitle("..또 건물로 들어와진 거야?", 2f);
            yield return new WaitForSeconds(2f);

            EventManager.Instance.ShowSubtitle("게다가 이번에는 불도 전부 꺼져 있어서 아무것도 안 보이잖아..", 3f);
            yield return new WaitForSeconds(3f);

            EventManager.Instance.ShowSubtitle("스마트폰 손전등을 켜야겠어.", 2f);
            yield return new WaitForSeconds(1.5f);

            if (_flashLight != null)
            {
                _flashLight.enabled = true;
                _audioSource.pitch = FlashSoundPitch;
                _audioSource.PlayOneShot(AudioClips[0]);
            }

            yield return new WaitForSeconds(1.5f);

            EventManager.Instance.ShowSubtitle("..잠깐, 저기 바닥에 뭔가 떨어져 있는데 뭐지?", 2f);
            EventManager.Instance.UpdateObjective("바닥에 떨어진 노트 확인하기");

            yield return new WaitForSeconds(2f);
            EventManager.Instance.ShowNotification("마우스 우클릭을 통해 손전등을 On/Off 할 수 있습니다.", 2f);

            yield return new WaitForSeconds(0.5f);
        }

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

        yield return new WaitForSeconds(1.5f);

        EventManager.Instance.ShowNotification("Tab 키를 눌러 가방(인벤토리)을 열 수 있습니다.", 3f);

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator PlayStorySequence()
    {
        if (GameManager.CurrentChapter == 1 && Chapter1Cut != null) yield return StartCoroutine(Chapter1Cut.PlayCutscene());
        else if (GameManager.CurrentChapter == 2 && Chapter2Cut != null) yield return StartCoroutine(Chapter2Cut.PlayCutscene());
        else if (GameManager.CurrentChapter == 3 && Chapter3Cut != null) yield return StartCoroutine(Chapter3Cut.PlayCutscene());
        else if (GameManager.CurrentChapter >= 4)
        {
            GameManager.IsPlayerStop = true;
            yield return StartCoroutine(EventManager.Instance.Fade(false, 2f));

            EventManager.Instance.FadePanel.SetActive(true);
            EventManager.Instance.ShowSubtitle("...그 이후 나는 무사히 건물 밖으로 나와 귀가할 수 있었고,\n다음 날 무사히 발표를 끝마쳤다.", 4f);
            yield return new WaitForSeconds(2f);
            EventManager.Instance.ShowSubtitle("꿈이라도 꿨던 것일까 싶으면서도 생생했던 기억은 \n시간이 꽤 흐른 지금도 어제 일처럼 선명하게 기억이 난다.", 4f);
            yield return new WaitForSeconds(2f);
            EventManager.Instance.ShowSubtitle("", 4f);
            yield return new WaitForSeconds(2f);

            SceneManager.LoadScene(EndingScene);
            yield break;
        }

        GameManager.Instance.NextChapeter();
        StartCoroutine(TeleportPlayer());
    }

    private IEnumerator LockedDoorSequence()
    {
        GameManager.IsPlayerStop = true;
        _CheckedLockedDoor = true;

        _audioSource.PlayOneShot(AudioClips[5], 3f);

        EventManager.Instance.ShowSubtitle("문이 잠겨있어.. 안 열려.", 6f);
        yield return new WaitForSeconds(3f);

        EventManager.Instance.ShowSubtitle("..어쩔 수 없지만 다시 반대쪽으로 나가야겠어.", 3f);
        yield return new WaitForSeconds(3f);

        GameManager.IsPlayerStop = false;

        GameManager.CanSprint = true;

        EventManager.Instance.ShowNotification("Left Shift를 누르면 달릴 수 있습니다.", 3f);
        EventManager.Instance.UpdateObjective("왼쪽 출입구로 다시 나가기");
    }
}
