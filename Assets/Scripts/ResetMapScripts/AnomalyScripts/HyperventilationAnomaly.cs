using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HyperventilationAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private LensDistortion _lensDistortion;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;

    private HeadBob _headBob;
    private bool _hasTriggered = false;
    private float _orginalWalkIntensity;
    private Coroutine _breathingCoroutine;

    public Volume PostProcessVolume;
    public float MaxLensDistortion = -0.5f;
    public float MaxChromaticAberration = 1.0f;
    public float MaxVignetteIntensity = 0.6f;
    public float EffectDuration = 60f;
    public float HeadBobMultiplier = 2.5f;

    public AudioSource BreathingAudio;
    public AudioSource HeartBeatAudio;
    public AudioClip[] BreathingSounds;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        Camera mainCamera = Camera.main;

        if (mainCamera != null ) _headBob = mainCamera.GetComponent<HeadBob>();

        if (PostProcessVolume != null)
        {
            PostProcessVolume.profile.TryGet(out _lensDistortion);
            PostProcessVolume.profile.TryGet(out _chromaticAberration);
            PostProcessVolume.profile.TryGet(out _vignette);
        }

        if (BreathingAudio != null) BreathingAudio.loop = true;
        if (HeartBeatAudio != null) HeartBeatAudio.loop = true;
    }

    public void TriggerAnomaly()
    {
        Debug.Log("과호흡 이상 현상 발생");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        if (_breathingCoroutine != null) StopCoroutine(_breathingCoroutine); 
        if (BreathingAudio != null) BreathingAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();

        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
        if (_chromaticAberration != null) _chromaticAberration.intensity.value = 0f;
        if (_vignette != null) _vignette.intensity.value = 0f;

        if (_headBob != null) if (_orginalWalkIntensity > 0) _headBob.WalkHeadBobIntensity = _orginalWalkIntensity;

        GameManager.CanSprint = true;
        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;
            StartCoroutine(PanicEffectSequence());
        }
    }

    private IEnumerator PanicEffectSequence()
    {
        if (_headBob == null || _lensDistortion == null || _chromaticAberration == null)
        {
            Debug.LogError("필수 컴포넌트 연결 필요");
            yield break;
        }

        GameManager.CanSprint = false;

        _orginalWalkIntensity = _headBob.WalkHeadBobIntensity;

        if (HeartBeatAudio != null) HeartBeatAudio.Play();
        _breathingCoroutine = StartCoroutine(PlayBreathingSounds());

        float timer = 0f;
        while (timer < EffectDuration) 
        {
            // 시간에 따라 효과가 서서히 강해졌다가 약해지도록 하는 수식 (깃허브 공개 코드 참고)
            float curve = Mathf.Sin(timer / EffectDuration * Mathf.PI);

            _lensDistortion.intensity.value = MaxLensDistortion * curve * 1.5f;
            _chromaticAberration.intensity.value = MaxChromaticAberration * curve * 1.5f;
            _vignette.intensity.value = MaxVignetteIntensity * curve * 1.5f;

            _headBob.WalkHeadBobIntensity = _orginalWalkIntensity + (_orginalWalkIntensity * HeadBobMultiplier * curve);

            timer += Time.deltaTime;
            yield return null;
        }

        ResetState();
    }

    private IEnumerator PlayBreathingSounds()
    {
        if (BreathingSounds.Length == 0 || BreathingAudio == null) yield break;

        while (true)
        {
            AudioClip clip = BreathingSounds[Random.Range(0, BreathingSounds.Length)];
            BreathingAudio.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }
}
