using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance;

    private AudioSource _audioSource;
    private Coroutine _fadeCoroutine;

    [Header("음원 리스트")]
    public AudioClip MainAmbient;
    public AudioClip StoryMusic;
    public AudioClip EndingMusic;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.loop = true;
        _audioSource.spatialBlend = 0f;
    }

    public void PlayBGM(AudioClip clip, float fadeDuration = 2.0f, float targetVolume = 0.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(CrossFadeMusic(clip, fadeDuration, targetVolume));
    }

    public void StopBGM(float fadeDuration = 2.0f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOut(fadeDuration));
    }

    private IEnumerator CrossFadeMusic(AudioClip newClip, float duration, float targetVolume)
    {
        
        if (_audioSource.clip == newClip && _audioSource.isPlaying)
        {
            yield break;
        }

        if (_audioSource.isPlaying)
        {
            float startVol = _audioSource.volume;
            float timer = 0f;
            while (timer < duration / 2)
            {
                timer += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(startVol, 0f, timer / (duration / 2));
                yield return null;
            }
            _audioSource.Stop();
        }

        if (newClip != null)
        {
            _audioSource.clip = newClip;
            _audioSource.Play();

            float timer = 0f;
            while (timer < duration / 2)
            {
                timer += Time.deltaTime;
                _audioSource.volume = Mathf.Lerp(0f, targetVolume, timer / (duration / 2));
                yield return null;
            }
            _audioSource.volume = targetVolume;
        }
    }

    private IEnumerator FadeOut(float duration)
    {
        float startVol = _audioSource.volume;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _audioSource.volume = Mathf.Lerp(startVol, 0f, timer / duration);
            yield return null;
        }
        _audioSource.Stop();
        _audioSource.clip = null;
    }
}
