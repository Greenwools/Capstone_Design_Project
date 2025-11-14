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
    private Coroutine _wobbleCoroutine;

    private bool _isLow = false;
    private float _currentSanity;
    private float _defaultFOV;

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

        if (PostProcessVolume != null)
        {
            PostProcessVolume.profile.TryGet(out _lensDistortion);
            PostProcessVolume.profile.TryGet(out _vignette);
        }
    }

    private void CheckSanityState()
    {
        if (_currentSanity <= SanityThreshold && !_isLow)
        {
            _isLow = true;
            GameManager.CanSprint = false;
            StartCoroutine(EnableLowSanityEffects());
        }

        else if (_currentSanity > SanityThreshold && _isLow)
        {
            _isLow = false;
            GameManager.CanSprint = true;
            StartCoroutine(EnableLowSanityEffects());
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

    private IEnumerator EnableLowSanityEffects()
    {
        if (_lensDistortion != null) _wobbleCoroutine = StartCoroutine(WobbleEffect());

        float timer = 0f;
        while (timer < 1f && _isLow)
        {
            MyCamera.fieldOfView = Mathf.Lerp(_defaultFOV, LowSanityFOV, timer / 1f);
            _vignette.intensity.value = Mathf.Lerp(0f, LowSanityVignetteIntensity, timer / 1f);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DisableLowSanityEffects()
    {
        if (_wobbleCoroutine != null) StopCoroutine(_wobbleCoroutine);
        if (_lensDistortion != null) _lensDistortion.intensity.value = 0f;

        float currentFov = MyCamera.fieldOfView;
        float currentVignette = _vignette.intensity.value;
        float timer = 0f;
        while (timer < 1f && !_isLow)
        {
            MyCamera.fieldOfView = Mathf.Lerp(currentFov, _defaultFOV, timer / 1f);
            _vignette.intensity.value = Mathf.Lerp(currentVignette, 0f, timer / 1f);
            timer += Time.deltaTime;
            yield return null;
        }

        MyCamera.fieldOfView = _defaultFOV;
        if (_vignette != null) _vignette.intensity.value = 0f;
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
