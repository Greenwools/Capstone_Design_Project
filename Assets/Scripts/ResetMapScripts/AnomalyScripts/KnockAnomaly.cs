using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private AudioSource _audioSource;
    private Quaternion _initialDoorRotation;
    private bool _isPlaying = false;

    public Transform DoorModel;
    public AudioClip KnockSound;
    public float KnockInterval = 3.0f;
    public float ShakeIntensity = 2.0f;
    public float ShakeDuration = 0.5f;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null) _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.spatialBlend = 1.0f;
        _audioSource.minDistance = 2.0f;
        _audioSource.maxDistance = 15.0f;
        _audioSource.loop = false;

        if (KnockSound != null) KnockSound.LoadAudioData();
        if (DoorModel != null) _initialDoorRotation = DoorModel.localRotation;
    }

    public void TriggerAnomaly()
    {
        gameObject.SetActive(true);

        if (!_isPlaying)
        {
            _isPlaying = true;
            StartCoroutine(KnockLoop());
        }
    }

    public void ResetState()
    {
        StopAllCoroutines();

        if (_audioSource != null) _audioSource.Stop();

        if (DoorModel != null) DoorModel.localRotation = _initialDoorRotation;

        _isPlaying = false;
        gameObject.SetActive(false);
    }

    private IEnumerator KnockLoop()
    {
        while (true)
        {
            if (_audioSource != null && KnockSound != null)
                _audioSource.PlayOneShot(KnockSound);

            float timer = 0f;
            while (timer < ShakeDuration)
            {
                if (DoorModel != null)
                {
                    float randomAngle = Random.Range(-ShakeIntensity, ShakeIntensity);
                    DoorModel.localRotation = _initialDoorRotation * Quaternion.Euler(0, randomAngle, 0);
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (DoorModel != null) DoorModel.localRotation = _initialDoorRotation;

            yield return new WaitForSeconds(KnockInterval + Random.Range(-0.5f, 1.0f));
        }
    }
}
