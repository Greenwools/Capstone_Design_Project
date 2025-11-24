using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerSanity : MonoBehaviour
{
    public static PlayerSanity Instance;

    private LensDistortion _lensDistortion;
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private Coroutine _wobbleCoroutine;
    private HeadBob _headBob;
    private Coroutine _effectCoroutine;
    private Coroutine _restoreCoroutine;

    private bool _isLow = false;
    private bool _isPanicEffectRunning = false;
    private float _currentSanity;
    private float _defaultFOV;
    private float _originalWalkIntensity;

    public Camera MyCamera;
    public Volume PostProcessVolume;

    public float MaxSanity = 100f;
    public float SanityThreshold = 40f;
    public float LowSanityFOV = 50f;
    public float WobbleSpeed = 1.0f;
    public float WobbleIntensity = 0.7f;
    public float LowSanityVignetteIntensity = 0.6f;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        _currentSanity = MaxSanity;
        _defaultFOV = MyCamera.fieldOfView;

        if (MyCamera != null) _headBob = MyCamera.GetComponent<HeadBob>();

        if (_headBob != null) _originalWalkIntensity = _headBob.WalkHeadBobIntensity;

        if (PostProcessVolume != null)
        {
            PostProcessVolume.profile.TryGet(out _lensDistortion);
            PostProcessVolume.profile.TryGet(out _vignette);
            PostProcessVolume.profile.TryGet(out _chromaticAberration);
        }
    }

    public void CheckSanityState()
    {
        if (_isPanicEffectRunning) return;

        if (_currentSanity <= SanityThreshold)
        {
            GameManager.CanSprint = false;

            if (!_isLow)
            {
                _isLow = true;
                if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
                _effectCoroutine = StartCoroutine(ApplySanityEffects(true));
            }
        }

        else
        {
            GameManager.CanSprint = true;

            if (_isLow)
            {
                _isLow = false;
                if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
                _effectCoroutine = StartCoroutine(ApplySanityEffects(false));
            }
        }
    }

    public void DecreaseSanity(float amount)
    {
        _currentSanity -= amount;
        if (_currentSanity < 0) _currentSanity = 0;
        Debug.Log("정신력 감소. 현재 : " + _currentSanity);
        CheckSanityState();
    }

    public void RestoreSanity(float amount)
    {
        _currentSanity += amount;
        if (_currentSanity > MaxSanity) _currentSanity = MaxSanity;
        Debug.Log("정신력 회복. 현재 : " + _currentSanity);
        CheckSanityState();
    }

    public void RestoreSanityGradually(float amount, float duration)
    {
        if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
        if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
        _isPanicEffectRunning = false;

        if (_restoreCoroutine != null) StopCoroutine(_restoreCoroutine);
        _restoreCoroutine = StartCoroutine(RestoreSanityRoutine(amount, duration));
    }

    public void StartPanicEffect(float duration, float headbobMultiplier, float lensDistortion, float chromaticAberration, float vignette, bool isSustain = false)
    {
        if (_effectCoroutine != null) StopCoroutine(_effectCoroutine);
        _effectCoroutine = StartCoroutine(PanicEffectSequence(duration, headbobMultiplier, lensDistortion, chromaticAberration, vignette, isSustain));
    }

    public void ResetAllEffects()   // 다른 코드에서 호출용
    {
        StopAllCoroutines();
        _effectCoroutine = null;
        _wobbleCoroutine = null;
        _isPanicEffectRunning = false;

        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;
        if (_vignette != null) _vignette.intensity.value = 0f;
        if (_chromaticAberration != null) _chromaticAberration.intensity.value = 0f;
        if (_headBob != null) _headBob.WalkHeadBobIntensity = _originalWalkIntensity;

        _isLow = false;

        CheckSanityState();
    }

    public float GetCurrentSanity()
    {
        return _currentSanity;
    }

    public void LoadSanity(float loadedSanity)
    {
        _currentSanity = loadedSanity;
        Debug.Log("정신력 로드 완료 : " + _currentSanity);

        CheckSanityState();
    }

    public void InitializeSanity()
    {
        _currentSanity = MaxSanity;

        if (MyCamera != null) _headBob = MyCamera.GetComponent<HeadBob>();
        if (_headBob != null) _originalWalkIntensity = _headBob.WalkHeadBobIntensity;

        if (PostProcessVolume != null)
        {
            PostProcessVolume.profile.TryGet(out _lensDistortion);
            PostProcessVolume.profile.TryGet(out _vignette);
            PostProcessVolume.profile.TryGet(out _chromaticAberration);
        }
    }

    private IEnumerator ApplySanityEffects(bool enable)
    {
        if (_lensDistortion != null && _wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);

        float timer = 0f;
        float startVignette = _vignette.intensity.value;
        float targetVignette = enable ? LowSanityVignetteIntensity : 0f;
        float startLens = _lensDistortion.intensity.value;
        float startChromatic = _chromaticAberration.intensity.value;
        float startWalkHeadBob = _headBob.WalkHeadBobIntensity;

        while (timer < 1f)
        {
            _vignette.intensity.value = Mathf.Lerp(startVignette, targetVignette, timer / 1f);
            _lensDistortion.intensity.value = Mathf.Lerp(startLens, 0f, timer / 1f);
            _chromaticAberration.intensity.value = Mathf.Lerp(startChromatic, 0f, timer / 1f);
            _headBob.WalkHeadBobIntensity = Mathf.Lerp(startWalkHeadBob, _originalWalkIntensity, timer / 1f);

            timer += Time.deltaTime;
            yield return null;
        }

        if (enable) _wobbleCoroutine = StartCoroutine(WobbleEffect());

        else
        {
            _vignette.intensity.value = 0f;
            _lensDistortion.intensity.value = 0f;
            _chromaticAberration.intensity.value = 0f;
            _headBob.WalkHeadBobIntensity = _originalWalkIntensity;
        }
    }

    private IEnumerator RestoreSanityRoutine(float targetAmount, float duration)
    {
        float startSanity = _currentSanity;
        float endSanity = Mathf.Min(_currentSanity + targetAmount, MaxSanity);
        float timer = 0f;

        float startVignette = (_vignette != null) ? _vignette.intensity.value : 0f;
        float startLens = (_lensDistortion != null) ? _lensDistortion.intensity.value : 0f;
        float startChromatic = (_chromaticAberration != null) ? _chromaticAberration.intensity.value : 0f;
        float startWalkBob = (_headBob != null) ? _headBob.WalkHeadBobIntensity : _originalWalkIntensity;

        while (timer < duration)
        {
            float t = timer / duration;

            _currentSanity = Mathf.Lerp(startSanity, endSanity, t);

            if (_vignette != null) _vignette.intensity.value = Mathf.Lerp(startVignette, 0f, t);
            if (_lensDistortion != null) _lensDistortion.intensity.value = Mathf.Lerp(startLens, 0f, t);
            if (_chromaticAberration != null) _chromaticAberration.intensity.value = Mathf.Lerp(startChromatic, 0f, t);
            if (_headBob != null) _headBob.WalkHeadBobIntensity = Mathf.Lerp(startWalkBob, _originalWalkIntensity, t);

            timer += Time.deltaTime;
            yield return null;
        }

        _currentSanity = endSanity;
        
        ResetAllEffects();
    }

    private IEnumerator PanicEffectSequence(float duration, float headbobMultiplier, float lensDistortion, float chromaticAberration, float vignette, bool isSustain)
    {
        _isPanicEffectRunning = true;
        GameManager.CanSprint = false;

        float timer = 0f;
        float rampUpTime = (isSustain) ? 3.0f : duration;
        float wobbleSpeed = 10.0f;
        float wobbleMagnitude = 0.3f;
        float noiseOffset = Random.Range(0f, 100f);

        while (timer < duration)
        {
            float blendFactor = 0f;

            if (isSustain)
            {
                blendFactor = Mathf.Clamp01(timer / rampUpTime);
            }

            else
            {
                blendFactor = Mathf.Sin(timer / duration * Mathf.PI);
            }

            float noiseVal = (Mathf.PerlinNoise(Time.time * wobbleSpeed, noiseOffset) * 2f) - 1f;
            float rapidWobble = noiseVal * wobbleMagnitude;
            float currentLens = Mathf.Lerp(0f, lensDistortion + rapidWobble, blendFactor);

            _lensDistortion.intensity.value = currentLens;

            if (_chromaticAberration != null) _chromaticAberration.intensity.value = Mathf.Lerp(0f, chromaticAberration, blendFactor);
            _vignette.intensity.value = Mathf.Lerp(0f, vignette, blendFactor);

            _headBob.WalkHeadBobIntensity = _originalWalkIntensity + (_originalWalkIntensity * headbobMultiplier * blendFactor);

            timer += Time.deltaTime;
            yield return null;
        }

        if (!isSustain)
        {
            _isPanicEffectRunning = false;
            CheckSanityState();
        }
    }

    private IEnumerator WobbleEffect()
    {
        float noiseOffsetX = Random.Range(0f, 100f);

        while (true)
        {
            float noise = (Mathf.PerlinNoise(noiseOffsetX + Time.time * WobbleSpeed, 0f) * 2f) - 1f;
            _lensDistortion.intensity.value = noise * WobbleIntensity;
            yield return null;
        }
    }
}
