using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightJamAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private Light _playerFlashlight;
    private AudioSource _audioSource;
    private bool _hasTriggered = false;

    public AudioClip FlickerSound;
    public AudioClip EerieSound;
    public float JamDuration = 5.0f;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.spatialBlend = 0f; // 0이면 2D(전체), 1이면 3D(거리감 있음)
        _audioSource.loop = false;      // 루프는 필요할 때만

        if (FlickerSound != null) FlickerSound.LoadAudioData();
        if (EerieSound != null) EerieSound.LoadAudioData();

    }

    private void FindFlashlight()
    {
        if (_playerFlashlight == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerFlashlight = player.GetComponentInChildren<Light>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;
            StartCoroutine(JamSequence());
        }
    }

    public void TriggerAnomaly()
    {
        Debug.Log("연출형 이상 현상(FlashlightJam) 활성화");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();
        FindFlashlight();

        if (_playerFlashlight != null) _playerFlashlight.enabled = true;
        if (_audioSource != null) _audioSource.Stop();

        GameManager.CanFlash = true;
        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private IEnumerator JamSequence()
    {
        FindFlashlight();
        if (_playerFlashlight == null) yield break;

        GameManager.CanFlash = false;

        if (_audioSource != null && FlickerSound != null)
        {
            _audioSource.clip = FlickerSound;
            _audioSource.Play();
        }

        for (int i = 0; i < 10; i++)
        {
            _playerFlashlight.enabled = !_playerFlashlight.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();

        _playerFlashlight.enabled = false;

        yield return new WaitForSeconds(1f);

        if (_audioSource != null && EerieSound != null)
        {
            _audioSource.clip = EerieSound;
            _audioSource.Play();
        }

        yield return new WaitForSeconds(JamDuration);

        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();

        if (_audioSource != null && FlickerSound != null)
        {
            _audioSource.clip = FlickerSound;
            _audioSource.Play();
        }

        for (int i = 0; i < 10; i++)
        {
            _playerFlashlight.enabled = !_playerFlashlight.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();

        GameManager.CanFlash = true;
        _playerFlashlight.enabled = true;
    }
}
