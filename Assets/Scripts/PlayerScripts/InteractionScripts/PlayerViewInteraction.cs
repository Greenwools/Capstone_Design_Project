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

        GameManager.LoopCount = 0;
        GameManager.IsAnomaly = false;
        if (StairBlockWall != null ) StairBlockWall.SetActive(true);
        if (TutorialNote != null ) TutorialNote.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.IsPlayerStop) return;

        if (GameManager.LoopCount >= 2 && Input.GetMouseButtonDown(1))
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
                if (door != null) 
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
                DecreaseSansity();
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
                DecreaseSansity();
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

    private void DecreaseSansity()
    {
        Debug.Log("정신력 감소");
    }

    private IEnumerator TeleportPlayer()
    {
        GameManager.IsPlayerStop = true;        // 이동하는 동안 플레이어 정지

        float timer = 0f;

        while (timer < FadeDuration) {
            FadeImage.color = new Color(0, 0, 0, timer / FadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        FadeImage.color = new Color(0, 0, 0, 1);

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
        }

        timer = 0f;
        while (timer < FadeDuration) {
            FadeImage.color = new Color(0, 0, 0, 1 - (timer / FadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        FadeImage.color = new Color(0, 0, 0, 0);

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator HorrorEventAndTeleport()
    {
        GameManager.IsPlayerStop = true;

        FadeImage.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        FadeImage.color = Color.black;
        yield return new WaitForSeconds(0.5f);

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
        }

        float timer = 0f;
        while (timer < FadeDuration)
        {
            FadeImage.color = new Color(0, 0, 0, 1 - (timer / FadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        FadeImage.color = new Color(0, 0, 0, 0);

        GameManager.IsPlayerStop = false;
    }

    private IEnumerator BackpackPickupEvent(GameObject backpack)
    {
        GameManager.IsPlayerStop = true;

        float timer = 0f;
        while (timer < FadeDuration)
        {
            FadeImage.color = new Color(0, 0, 0, timer / FadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        FadeImage.color = new Color(0, 0, 0, 1);

        _audioSource.pitch = 0.8f;
        _audioSource.PlayOneShot(AudioClips[4]);

        GameManager.HasBackpack = true;
        Destroy(backpack);

        if (OnDeskObject != null) OnDeskObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        timer = 0f;
        while (timer < FadeDuration)
        {
            FadeImage.color = new Color(0, 0, 0, 1 - (timer / FadeDuration));
            timer += Time.deltaTime;
            yield return null;
        }
        FadeImage.color = new Color(0, 0, 0, 0);

        GameManager.IsPlayerStop = false;
    }
}
