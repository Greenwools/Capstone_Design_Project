using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.Rendering.UI;

public class PlayerViewInteraction : MonoBehaviour
{
    private Camera _mainCamera;
    private Light _flashLight;
    private GameObject[] _mainLights;
    private bool _CheckedLockedDoor = false;
    private bool _isTutorialNoteRead = false;
    private bool _isEndingPhase = false;
    private Image Crosshair;

    private AudioSource _audioSource;
    public AudioClip[] AudioClips;
    public float FlashSoundPitch = 1.2f;

    public float InteractionDistance = 3f;
    public LayerMask InteractionLayerMask;

    public Transform PlayerTransform;
    public Transform SpawnTransform;
    public Transform EndingSpawnPoint;

    public Image FadeImage;
    public float FadeDuration = 1f;

    public GameObject CrosshairPannel;
    public GameObject StairBlockWall;
    public GameObject TutorialNote;
    public GameObject SecondNote;
    public GameObject OnDeskObject;
    public GameObject DirectionalLight;

    public Item TutorialNoteItem;
    public Item KeyItem;
    public Item DiagnosisItem;
    public Item WatchItem;
    public Item MedicineItem;

    public ChapterCutScene Chapter1Cut;
    public ChapterCutScene Chapter2Cut;
    public string EndingScene = "EndingScene";
    public Text EndingText;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = GetComponent<Camera>();
        _flashLight = GetComponent<Light>();
        _audioSource = GetComponent<AudioSource>();
        Crosshair = CrosshairPannel.GetComponent<Image>();
        _isTutorialNoteRead = false;
        _mainLights = GameObject.FindGameObjectsWithTag("MainLight");

        if (DirectionalLight != null) DirectionalLight.SetActive(false);

        if (_audioSource == null )
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        if (GameManager.Instance != null && PlayerTransform != null)
            GameManager.Instance.RegisterPlayer(PlayerTransform);

        if (GameManager.LoopCount <= 1)
        {
            if (GameManager.LoopCount == 0 && OnDeskObject != null) OnDeskObject.SetActive(true);

            if (GameManager.LoopCount == 1)
            {
                GameManager.CanSprint = true;
                if (OnDeskObject != null) OnDeskObject.SetActive(!GameManager.HasBackpack);
            }
            if (StairBlockWall != null) StairBlockWall.SetActive(true);
            if (TutorialNote != null) TutorialNote.SetActive(false);
            if (SecondNote != null) SecondNote.SetActive(false);
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
        if (CrosshairPannel != null)
        {
            bool isMenuOpen = GameManager.Instance.IsUIOpen();

            bool isFading = (EventManager.Instance != null && EventManager.Instance.FadePanel != null && EventManager.Instance.FadePanel.activeSelf);

            bool shouldShow = !isMenuOpen && !isFading;

            if (!_isEndingPhase)
            {
                if (CrosshairPannel.activeSelf != shouldShow) CrosshairPannel.SetActive(shouldShow);
            }
        }

        if (GameManager.IsPlayerStop || GameManager.Instance.IsUIOpen()) return;

        if (GameManager.LoopCount == 2 && !_isTutorialNoteRead)
        {
            if (NoteUI.Instance != null && !NoteUI.Instance.IsReading && InventoryManager.Instance.HasItem(TutorialNoteItem))
            {
                _isTutorialNoteRead = true;
                StartCoroutine(GetTutorialNote());
            }
        }

        if (GameManager.LoopCount >= 2 && !GameManager.Instance.IsUIOpen() && Input.GetMouseButtonDown(1))
        {
            if (_flashLight != null && GameManager.CanFlash)
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
                if (GameManager.CurrentChapter == 4 && GameManager.Instance.IsMedicineUsed && !GameManager.Instance.IsEndingReady)
                    return;

                if (hit.collider.tag == "Backpack")
                {
                    if (!GameManager.HasBackpack) StartCoroutine(BackpackPickupEvent(hit.collider.gameObject));
                    return;
                }

                ItemPickup itemPickup = hit.collider.GetComponent<ItemPickup>();
                if (itemPickup != null)
                {
                    HandleItemPickup(itemPickup);
                    return;
                }

                DoorInteraction door = hit.collider.GetComponent<DoorInteraction>();
                if (door != null && door.enabled) 
                { 
                    if (GameManager.LoopCount == 0 && !GameManager.HasBackpack)
                    {
                        EventManager.Instance.ShowSubtitle("짐이랑 가방을 챙기고 돌아가야지. 그냥 갈 수는 없어..", 2f);
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

                    if (_isEndingPhase)
                    {
                        StartCoroutine(GoToEndingScene());
                        return;
                    }

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

    private void HandleItemPickup(ItemPickup pickup)
    {
        Item item = pickup.item;
        if (item == TutorialNoteItem && NoteUI.Instance != null) NoteUI.Instance.ShowNote(((NoteItem)item).pages);

        else if (item == KeyItem) EventManager.Instance.ShowSubtitle("이건 우리 집 열쇠잖아..? 이게 왜 여기에 있고 왜 이리 녹슬어 있는 거지?", 4f);

        else if (item == DiagnosisItem) EventManager.Instance.ShowSubtitle("이건.. 진단서인가? 내 이름이 적혀있어..", 4f);

        else if (item == WatchItem) StartCoroutine(Chapter3PanicSequence());

        pickup.Pickup();
    }

    private void CheckChoice(string objName)
    {
        if (GameManager.Instance.IsEndingReady && (objName.Contains("LeftExit"))) 
        {
            StartCoroutine(GoToEndingSequence());
            return;
        }

        bool hasRequiredItems = InventoryManager.Instance.HasAllRequiredItemsForCurrentChapter();

        if (hasRequiredItems && objName.Contains("LeftExit"))
        {
            TriggerStoryEvent();
            return;
        }

        if (!GameManager.IsAnomaly)
        {
            if (objName.Contains("LeftExit_Door"))
            {
                _audioSource.pitch = 1.2f;
                _audioSource.PlayOneShot(AudioClips[2]);
                StartCoroutine(TeleportSequence(false, false));
            }

            else if (objName.Contains("Stairs"))
            {
                _audioSource.pitch = 1.45f;
                _audioSource.PlayOneShot(AudioClips[3]);
                DecreaseSanity();
                StartCoroutine(TeleportSequence(false, false));
            }
        }

        else
        {
            if (objName.Contains("Stairs"))
            {
                _audioSource.pitch = 1.45f;
                _audioSource.PlayOneShot(AudioClips[3]);
                StartCoroutine(TeleportSequence(false, false));
            }

            else if (objName.Contains("LeftExit_Door"))
            {
                DecreaseSanity();
                StartCoroutine(TeleportSequence(false, true));
            }
        }
    }

    private void TriggerStoryEvent()
    {
        Debug.Log("필수 아이템 전부 입수 -> " + GameManager.CurrentChapter + "챕터 스토리 시작");

        StartCoroutine(TeleportSequence(true, false));
    }

    private void DecreaseSanity()
    {
        Debug.Log("정신력 감소");
        PlayerSanity.Instance.DecreaseSanity(20f);
    }

    private IEnumerator TeleportSequence(bool isStoryEvent, bool isHorror)
    {
        GameManager.IsPlayerStop = true;

        if (isStoryEvent || (GameManager.CurrentChapter == 3 && GameManager.Instance.IsMedicineUsed))
        {
            if (_flashLight != null) _flashLight.enabled = false;
        }

        yield return StartCoroutine(EventManager.Instance.Fade(false, FadeDuration));

        if (isStoryEvent)
        {
            if (GameManager.CurrentChapter == 1 && Chapter1Cut != null) yield return StartCoroutine(Chapter1Cut.PlayCutscene());
            else if (GameManager.CurrentChapter == 2 && Chapter2Cut != null) yield return StartCoroutine(Chapter2Cut.PlayCutscene());

            GameManager.Instance.NextChapeter();
        }

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = SpawnTransform.position;
        PlayerTransform.rotation = SpawnTransform.rotation;
        cc.enabled = true;

        //if (CameraManager.Instance != null) CameraManager.Instance.SetXRotation(0f);

        GameManager.Instance.DecideNextLoopState();

        bool isMedicineRoute = (GameManager.CurrentChapter == 4 && GameManager.Instance.IsMedicineUsed);
        
        if (isMedicineRoute)
        {
            foreach (GameObject light in _mainLights) light.SetActive(true);
            GameManager.IsAnomaly = false;
            if (StairBlockWall != null) StairBlockWall.SetActive(true);

            GameManager.CanSprint = false;
        }

        else if (GameManager.LoopCount == 2)
        {
            if (TutorialNote != null) TutorialNote.SetActive(true);
            if (StairBlockWall != null) StairBlockWall.SetActive(false);
            foreach (GameObject light in _mainLights) light.SetActive(false);
            EventManager.Instance.UpdateObjective("");
        }

        yield return StartCoroutine(EventManager.Instance.Fade(true, FadeDuration));

        if (isMedicineRoute) StartCoroutine(Chapter3MonologueSequence());

        else if (isStoryEvent)
        {
            yield return new WaitForSeconds(1f);
            EventManager.Instance.UpdateObjective("");

            if (GameManager.CurrentChapter == 2)
            {
                if (SecondNote != null) SecondNote.SetActive(true);
                GameManager.IsPlayerStop = true;
                EventManager.Instance.ShowSubtitle("... 방금 그건...", 2f);
                yield return new WaitForSeconds(2f);
                EventManager.Instance.ShowSubtitle("잊은 줄 알았는데... 왜 갑자기 이런 걸 보여주는 거지?", 3f);
                yield return new WaitForSeconds(3f);
                EventManager.Instance.ShowSubtitle(".....", 2f);
                yield return new WaitForSeconds(2f);
                GameManager.IsPlayerStop = false;
            }

            else if (GameManager.CurrentChapter == 3)
            {
                GameManager.IsPlayerStop = true;
                EventManager.Instance.ShowSubtitle("...그날 받아온 약, 뜯지도 않고 가방 안에 넣어놨었지.", 3f);
                yield return new WaitForSeconds(3f);
                EventManager.Instance.ShowSubtitle("난 환자가 아니라고 생각했으니까..", 3f);
                yield return new WaitForSeconds(3f);
                EventManager.Instance.ShowSubtitle(".....", 2f);
                yield return new WaitForSeconds(2f);
                EventManager.Instance.ShowSubtitle("그때 솔직하게 인정하고 치료받았다면... 지금과 달라졌을까?", 3f);
                yield return new WaitForSeconds(3f);
                GameManager.IsPlayerStop = false;
            }

            EventManager.Instance.ShowSubtitle("...일단은 계속 나아가 보자.", 2f);
        }

        else if (GameManager.LoopCount == 1)
        {
            EventManager.Instance.ShowSubtitle("...어라? 난 분명 문을 열고 밖으로 나왔는데", 3f);
            yield return new WaitForSeconds(3f);
            EventManager.Instance.ShowSubtitle("어째서 다시 건물 내부로 들어온 거지..?", 3f);
            yield return new WaitForSeconds(3f);
            EventManager.Instance.ShowSubtitle("..뭔가 이상해, 얼른 나가야겠어.", 3f);
            yield return new WaitForSeconds(2f);

            EventManager.Instance.UpdateObjective("들어온 문 확인하기");
        }

        else if (GameManager.LoopCount == 2)
        {
            yield return new WaitForSeconds(1f);

            EventManager.Instance.ShowSubtitle("..또 건물로 들어와진 거야?", 2f);
            yield return new WaitForSeconds(2f);

            EventManager.Instance.ShowSubtitle("대체 뭐가 어떻게 되어가고 있는 거지?", 2f);
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

            yield return new WaitForSeconds(1f);

            EventManager.Instance.ShowSubtitle("..잠깐, 저기 바닥에 뭔가 떨어져 있는데 뭐지?", 2f);
            EventManager.Instance.UpdateObjective("바닥에 떨어진 노트 확인하기");

            yield return new WaitForSeconds(2f);
            EventManager.Instance.ShowNotification("마우스 우클릭을 통해 손전등을 On/Off 할 수 있습니다.", 2f);

            yield return new WaitForSeconds(1f);
        }

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
        EventManager.Instance.ShowSubtitle("이제 건물 밖으로 나가자.", 3f);

        yield return new WaitForSeconds(0.5f);
        EventManager.Instance.UpdateObjective("왼쪽 출입구로 나가기");

        yield return StartCoroutine(EventManager.Instance.Fade(true, 1f));

        EventManager.Instance.ShowNotification("Tab 키를 눌러 가방(인벤토리)을 열 수 있습니다.", 2f);

        yield return new WaitForSeconds(2f);

        GameManager.IsPlayerStop = false;
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

    private IEnumerator GetTutorialNote()
    {
        yield return new WaitForSeconds(0.5f);
        EventManager.Instance.ShowSubtitle("이 노트... 쉽게 믿기 힘들지만..", 2f);
        yield return new WaitForSeconds(2f);
        EventManager.Instance.ShowSubtitle("지금 상황을 보면 어느 정도는 믿어야겠지.", 2f);
        yield return new WaitForSeconds(2f);
        EventManager.Instance.ShowSubtitle("우선 여기서 나가는 것만 생각하자.", 2f);

        EventManager.Instance.UpdateObjective("이상 현상 찾기");
        yield return new WaitForSeconds(2f);
        EventManager.Instance.UpdateObjective("");
    }

    private IEnumerator Chapter3PanicSequence()
    {
        GameManager.IsPlayerStop = true;
        EventManager.Instance.ShowSubtitle("이 시계... 분명 그 날, 내가 넘어지면서...", 3f);
        yield return new WaitForSeconds(3f);

        if (PlayerSanity.Instance != null) PlayerSanity.Instance.StartPanicEffect(999f, 3.0f, -0.5f, 1.0f, 0.7f, true);

        EventManager.Instance.ShowSubtitle("윽.. 또 그 때처럼..", 2f);
        yield return new WaitForSeconds(2f);

        GameManager.IsPlayerStop = false;

        EventManager.Instance.ShowSubtitle("허억... 헉... 수, 숨이... 안 쉬어져...", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("가슴이... 터질 것 같아... 누가 좀...", 3f);
        yield return new WaitForSeconds(4f);

        EventManager.Instance.UpdateObjective("증상 진정시키기");

        EventManager.Instance.ShowSubtitle("하아.. 하아.. 진정.. 진정해야 해..", 3f);
        yield return new WaitForSeconds(3f);

        EventManager.Instance.ShowSubtitle("아... 맞다... 가방... 가방 안에...", 3f);
        yield return new WaitForSeconds(3f);

        EventManager.Instance.ShowSubtitle("그때 받았던... 제발 있어라...!", 2f);

        // 약 아이템 지급 및 히든 슬롯 개방
        if (InventoryUI.Instance != null) InventoryUI.Instance.UnlockHiddenTab();
        InventoryManager.Instance.Add(MedicineItem);
    }

    private IEnumerator Chapter3MonologueSequence()
    {
        yield return new WaitForSeconds(2f);
        EventManager.Instance.ShowSubtitle("어두웠던 복도에... 불이 다시 켜졌어.", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("원래대로 돌아온 건가..?", 3f);
        yield return new WaitForSeconds(3f);

        EventManager.Instance.ShowSubtitle("...", 2f);
        yield return new WaitForSeconds(2f);
        EventManager.Instance.ShowSubtitle("사실은 알고 있었어. 내가 아프다는 건..", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("아프다는 걸 인정하면... 남들에게 뒤처질까 봐, 약해 보일까 봐... 시선을 돌렸었어.", 4f);
        yield return new WaitForSeconds(4f);
        EventManager.Instance.ShowSubtitle("그렇게 미련하게 도망치다 보니, 내 시간은 그 망가진 시계처럼 멈춰버렸던 거야.", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("이제는 받아들여야 해. 나는 도움이 필요한 상태였어.", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("하지만 치료받고 노력하면... 다시 멈춘 시간을 움직일 수 있을거야.", 3f);

        GameManager.Instance.IsEndingReady = true;
        EventManager.Instance.UpdateObjective("복도 끝 문으로 나가기");
    }

    private IEnumerator GoToEndingSequence()
    {
        GameManager.IsPlayerStop = true;
        EventManager.Instance.UpdateObjective("");
        CrosshairPannel.SetActive(false);

        yield return StartCoroutine(EventManager.Instance.Fade(false, 2.0f));

        if (EventManager.Instance.FadePanel != null)
            EventManager.Instance.FadePanel.GetComponent<Image>().color = Color.black;

        EventManager.Instance.FadePanel.SetActive(true);

        EndingText.text = "...문을 열자, 차가운 밤공기가 느껴졌다. 드디어 건물 밖으로 나온 것이다.";
        EndingText.gameObject.SetActive(true);
        yield return new WaitForSeconds(4f);

        EndingText.text = "서둘러 집으로 돌아온 곧바로 내가 겪었던 일에 대해 찾아보았다.";
        yield return new WaitForSeconds(4f);
        EndingText.text = "한 때 떠올랐던 도시 괴담에 대한 내용은 많이 있지 않았지만, 알게된 것도 있었다.";
        yield return new WaitForSeconds(4f);
        EndingText.text = "도시 괴담 따위가 아니었다. 그곳은 내 불안이 만들어낸 세계였다. \n\n탈출구는 도망치는 것이 아니라, 그 불안을 있는 그대로 '인정'하는 데 있었다.";
        yield return new WaitForSeconds(4f);
        EndingText.text = "나는 몇 번이고 같은 자리를 맴돌며, 애써 외면했던 나 자신의 아픔을 마주 보았다. \n\n내가 나를 받아들였을 때, 비로소 시간은 다시 흐르기 시작했다.";
        yield return new WaitForSeconds(5f);
        EndingText.text = "그 이후로 시간이 흘러 프로젝트 발표를 하는 날이 왔다.";
        yield return new WaitForSeconds(3f);
        EndingText.text = "걱정했던 것과 달리 발표는 순조롭게 진행했으며 마음 한켠으로 홀가분한 마음이 들었다.";
        yield return new WaitForSeconds(4f);
        EndingText.text = "그리고 나는 언젠가 그 공간에서 다짐했던 것을 실행하기 위해 발걸음을 옮겼다.";
        yield return new WaitForSeconds(3f);
        EndingText.gameObject.SetActive(false);

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = EndingSpawnPoint.position;
        PlayerTransform.rotation = EndingSpawnPoint.rotation;
        cc.enabled = true;
        if (CameraManager.Instance != null) CameraManager.Instance.SetXRotation(0f);

        _isEndingPhase = true;
        GameManager.IsAnomaly = false;
        GameManager.CanSprint = false;
        CrosshairPannel.SetActive(true);

        DirectionalLight.SetActive(true);

        yield return StartCoroutine(EventManager.Instance.Fade(true, 2.0f));

        EventManager.Instance.ShowSubtitle("후우.. 괜찮아. 별 것 아니잖아.", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.ShowSubtitle("자, 들아가자.", 3f);
        yield return new WaitForSeconds(3f);
        EventManager.Instance.UpdateObjective("문 열고 들어가기");

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator GoToEndingScene()
    {
        GameManager.IsPlayerStop = true;

        Image fadeImg = EventManager.Instance.FadePanel.GetComponent<Image>();
        fadeImg.color = new Color(1, 1, 1, 0);

        EventManager.Instance.FadePanel.SetActive(true);
        float timer = 0f;
        float duration = 3f;
        while (timer < duration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / duration);
            fadeImg.color = new Color(1, 1, 1, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImg.color = Color.white;

        EventManager.Instance.SubtitleText.color = Color.black;
        EventManager.Instance.ShowSubtitle("괜찮아. 다 잘 될 거야.", 4f);
        yield return new WaitForSeconds(4f);

        SceneManager.LoadScene(EndingScene);
    }
}
