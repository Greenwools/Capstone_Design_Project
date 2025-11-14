using OccaSoftware.Buto.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FloodingAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private PlayerController _playerController;
    private bool _anomalyTriggered = false;
    private Vector3 _initialWaterPosition;
    private LensDistortion _lensDistortion;
    private Coroutine _wobbleCoroutine;

    public GameObject WaterVolumeObject;
    public Volume PostProcessVolume;
    public Image WaterOverlayUI;
    public Image FadeImage;
    public Transform PlayerTransform;
    public Transform SpawnTransform;

    public float TargetWaterHeight = 3.0f;
    public float TimeToDrown = 30.0f;
    public float WobbleSpeed = 2f;
    public float WobbleIntensity = 0.3f;

    public AudioSource WaterAudio;
    public AudioSource BreathingAudio;
    public AudioSource HeartBeatAudio;
    public AudioSource DrownAudio;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _playerController = FindObjectOfType<PlayerController>();

        if (PlayerTransform == null) PlayerTransform = _playerController.transform;
        if (SpawnTransform == null) SpawnTransform = GameObject.Find("SpawnPoint").transform;
        if (FadeImage == null) FadeImage = GameObject.Find("FadeImage").GetComponent<Image>();
        if (WaterOverlayUI == null) WaterOverlayUI = GameObject.Find("WaterOverlay").GetComponent<Image>();
        if (PostProcessVolume == null) PostProcessVolume = FindObjectOfType<Volume>();

        if (WaterVolumeObject != null) _initialWaterPosition = WaterVolumeObject.transform.position;

        if (PostProcessVolume != null) PostProcessVolume.profile.TryGet(out _lensDistortion);

        if (WaterAudio == null) WaterAudio = GetComponent<AudioSource>();
        WaterAudio.playOnAwake = false;
        WaterAudio.loop = true;

        if (BreathingAudio != null) BreathingAudio.loop = true;
        if (HeartBeatAudio != null) HeartBeatAudio.loop = true;
    }

    public void TriggerAnomaly()
    {
        Debug.Log("연출형 이상 현상(FloodingAnomaly) 발생");
        gameObject.SetActive(true);

        if (WaterAudio != null) WaterAudio.clip.LoadAudioData();
        if (BreathingAudio != null) BreathingAudio.clip.LoadAudioData();
        if (HeartBeatAudio != null) HeartBeatAudio.clip.LoadAudioData();
    }

    public void ResetState()
    {
        StopAllCoroutines();
        if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);

        if (WaterAudio != null) WaterAudio.clip.UnloadAudioData();
        if (BreathingAudio != null) BreathingAudio.clip.UnloadAudioData();
        if (HeartBeatAudio != null) HeartBeatAudio.clip.UnloadAudioData();

        WaterAudio.Stop();
        if (BreathingAudio != null) BreathingAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();

        if (WaterVolumeObject != null)
        {
            WaterVolumeObject.transform.position = _initialWaterPosition;
            WaterVolumeObject.SetActive(false);
        }

        if (WaterOverlayUI != null) WaterOverlayUI.gameObject.SetActive(false);
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;

        if (_playerController != null) _playerController.IsInWater = false;
        _anomalyTriggered = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_anomalyTriggered)
        {
            _anomalyTriggered = true;
            StartCoroutine(FloodSequence());
            StartCoroutine(DrownTimer());
        }
    }

    private IEnumerator FloodSequence()
    {
        WaterAudio.Play();
        if (BreathingAudio != null) BreathingAudio.Play();
        if (HeartBeatAudio != null) HeartBeatAudio.Play();

        if (_playerController != null) _playerController.IsInWater = true;

        if (WaterVolumeObject != null)
        {
            WaterVolumeObject.SetActive(true);
            Vector3 startPosition = _initialWaterPosition;
            Vector3 targetPosition = new Vector3(_initialWaterPosition.x, TargetWaterHeight, _initialWaterPosition.z);
            float elapsedTime = 0f;

            Transform cameraTransform = _playerController.GetComponentInChildren<Camera>().transform;
            bool submerged = false;

            while (elapsedTime < TimeToDrown)
            {
                WaterVolumeObject.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / TimeToDrown);

                float currentWaterSurfaceY = WaterVolumeObject.transform.position.y + (WaterVolumeObject.transform.localScale.y / 2f);

                if (_playerController != null) _playerController.CurrentWaterLevel = currentWaterSurfaceY;

                if (!submerged && cameraTransform.position.y + 0.25f < currentWaterSurfaceY)
                {
                    submerged = true;
                    if (WaterOverlayUI != null) WaterOverlayUI.gameObject.SetActive(true);
                    if (BreathingAudio != null) BreathingAudio.Stop();
                    if (_lensDistortion != null) _wobbleCoroutine = StartCoroutine(WobbleEffect());
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }
        }
    }

    private IEnumerator WobbleEffect()
    {
        float noiseOffsetX = Random.Range(0f, 100f);
        float noiseOffsetY = Random.Range(0f, 100f);

        while (true)
        {
            float noise = (Mathf.PerlinNoise(noiseOffsetX + Time.time * WobbleSpeed, noiseOffsetY) * 2f) - 1f;

            _lensDistortion.intensity.value = noise * WobbleIntensity;
            yield return null;
        }
    }

    private IEnumerator DrownTimer()
    {
        yield return new WaitForSeconds(TimeToDrown);
        Debug.Log("시간 초과, 이상 현상 돌파 실패");

        GameManager.IsPlayerStop = true;

        WaterAudio.Stop();
        if (BreathingAudio != null) BreathingAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();
        if (DrownAudio != null) DrownAudio.Play();
        if (WaterOverlayUI != null) WaterOverlayUI.gameObject.SetActive(false);
        if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;

        DecreaseSanity();

        float fadeDuration = 1f;

        yield return StartCoroutine(EventManager.Instance.Fade(false, fadeDuration));

        yield return new WaitForSeconds(0.5f);

        CharacterController cc = PlayerTransform.GetComponent<CharacterController>();
        cc.enabled = false;
        PlayerTransform.position = SpawnTransform.position;
        PlayerTransform.rotation = SpawnTransform.rotation;
        cc.enabled = true;

        yield return StartCoroutine(EventManager.Instance.Fade(true, fadeDuration));

        GameManager.Instance.DecideNextLoopState();
        GameManager.IsPlayerStop = false;
    }

    private void DecreaseSanity()
    {
        PlayerSanity.Instance.DecreaseSanity(20f);
    }
}
