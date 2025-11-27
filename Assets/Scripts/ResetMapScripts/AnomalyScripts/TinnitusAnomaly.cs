using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;


public class TinnitusAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private AudioSource _audioSource;
    private Volume _globalVolume;
    private Vignette _vignette;
    private ChromaticAberration _chromaticAberration;
    private LensDistortion _lensDistortion;

    private float _originalVignette;
    private float _originalChromatic;
    private float _originalDistortion;
    private float _originalMixerVolume;
    private bool _hasTriggered = false;

    public AudioMixer TargetMixer;
    public string ExposedParamName = "MasterSFXVolume";
    public float MuffledVolume = -25.0f;

    public AudioClip RingingSound;
    public float Duration = 15.0f;
    [Range(0f, 1f)] public float MaxVolume = 0.8f;

    public float TargetVignette = 0.8f;
    public float TargetChromatic = 1.2f;
    public float TargetDistortion = -0.2f;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.spatialBlend = 0f;
        _audioSource.loop = true;

        if (RingingSound != null) RingingSound.LoadAudioData();

        _globalVolume = FindObjectOfType<Volume>();
        if (_globalVolume != null)
        {
            _globalVolume.profile.TryGet(out _vignette);
            _globalVolume.profile.TryGet(out _chromaticAberration);
            _globalVolume.profile.TryGet(out _lensDistortion);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;
            StartCoroutine(TinnitusSequence());
        }
    }

    public void TriggerAnomaly()
    {
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        if (_vignette != null) _vignette.intensity.value = _originalVignette;
        if (_chromaticAberration != null) _chromaticAberration.intensity.value = _originalChromatic;
        if (_lensDistortion != null) _lensDistortion.intensity.value = _originalDistortion;
        if (TargetMixer != null) TargetMixer.SetFloat(ExposedParamName, 0f);
        if (_audioSource != null) _audioSource.Stop();

        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private IEnumerator TinnitusSequence()
    {
        if (_audioSource == null || RingingSound == null) yield break;

        if (_vignette != null) _originalVignette = _vignette.intensity.value;
        if (_chromaticAberration != null) _originalChromatic = _chromaticAberration.intensity.value;
        if (_lensDistortion != null) _originalDistortion = _lensDistortion.intensity.value;
        if (TargetMixer != null) TargetMixer.GetFloat(ExposedParamName, out _originalMixerVolume);

        _audioSource.clip = RingingSound;
        _audioSource.volume = 0f;
        _audioSource.Play();

        float timer = 0f;
        float fadeInTime = 2.0f;

        while (timer < fadeInTime)
        {
            float t = timer / fadeInTime;

            _audioSource.volume = Mathf.Lerp(0f, MaxVolume, t);

            if (_vignette != null) _vignette.intensity.value = Mathf.Lerp(_originalVignette, TargetVignette, t);
            if (_chromaticAberration != null) _chromaticAberration.intensity.value = Mathf.Lerp(_originalChromatic, TargetChromatic, t);
            if (_lensDistortion != null) _lensDistortion.intensity.value = Mathf.Lerp(_originalDistortion, TargetDistortion, t);

            if (TargetMixer != null)
            {
                float currentVol = Mathf.Lerp(_originalMixerVolume, MuffledVolume, t);
                TargetMixer.SetFloat(ExposedParamName, currentVol);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (_vignette != null) _vignette.intensity.value = TargetVignette;
        if (TargetMixer != null) TargetMixer.SetFloat(ExposedParamName, MuffledVolume);

        yield return new WaitForSeconds(Duration - fadeInTime - 2.0f);

        timer = 0f;
        float fadeOutTime = 5.0f;

        while (timer < fadeOutTime)
        {
            float t = timer / fadeInTime;

            _audioSource.volume = Mathf.Lerp(MaxVolume, 0f, t);

            if (_vignette != null) _vignette.intensity.value = Mathf.Lerp(TargetVignette, _originalVignette, t);
            if (_chromaticAberration != null) _chromaticAberration.intensity.value = Mathf.Lerp(TargetChromatic, _originalChromatic, t);
            if (_lensDistortion != null) _lensDistortion.intensity.value = Mathf.Lerp(TargetDistortion, _originalDistortion, t);

            if (TargetMixer != null)
            {
                float currentVol = Mathf.Lerp(MuffledVolume, _originalMixerVolume, t);
                TargetMixer.SetFloat(ExposedParamName, currentVol);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        _audioSource.Stop();
        if (_vignette != null) _vignette.intensity.value = _originalVignette;
        if (TargetMixer != null) TargetMixer.SetFloat(ExposedParamName, _originalMixerVolume);
    }
}
