using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CabinetInteraction : MonoBehaviour, IResetable
{
    private Vector3 _cabinetClosePosition;
    private bool _isCabinetOpen = false;
    private bool _isCabinetMove = false;

    private AudioSource _audioSource;
    public AudioClip[] CabinetSounds;
    public float CabinetSoundPitch = 1.2f;

    public Vector3 CabinetOpenOffset = new Vector3(0.5f, 0, 0);
    public float CabinetSlideSpeed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        _cabinetClosePosition = transform.position;
        _audioSource = GetComponent<AudioSource>();

        if (_audioSource != null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
    }

    public bool IsOpen()
    {
        if (_isCabinetOpen) return true;
        else return false;
    }

    public void ToggleCabinet()
    {
        if (_isCabinetMove) return;

        StartCoroutine(SlideCabinet());
    }

    public void ResetState()
    {
        StopAllCoroutines();
        _audioSource.Stop();
        transform.position = _cabinetClosePosition;
        _isCabinetOpen = false;
        _isCabinetMove = false;
    }

    private IEnumerator SlideCabinet()
    {
        _isCabinetMove = true;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition;
        Vector3 worldOffset = transform.rotation * CabinetOpenOffset;

        AudioClip clip;
        if (_isCabinetOpen)
        {
            clip = CabinetSounds[0];
            _audioSource.pitch = CabinetSoundPitch;
            targetPosition = _cabinetClosePosition;
        }

        else
        {
            clip = CabinetSounds[1];
            _audioSource.pitch = 1f;
            targetPosition = _cabinetClosePosition + worldOffset;
        }

        _audioSource.PlayOneShot(clip);

        float time = 0;

        while (time < 1)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, time);
            time += Time.deltaTime * CabinetSlideSpeed;
            yield return null;
        }

        transform.position = targetPosition;
        _isCabinetOpen = !_isCabinetOpen;
        _isCabinetMove = false;
    }
}
