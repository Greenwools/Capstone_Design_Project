using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTriggerAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private int _currentPlayCount = 0;

    public AudioSource Audio;
    public AudioClip AudioSound;

    public float MaxPlayCount = 2;
    public float VolumeScale = 1.0f;

    void Awake()
    {
        Collider col = GetComponent<Collider>();

        if (col != null) col.isTrigger = true;

        else Debug.LogError(gameObject.name + " : Collider 필요");
    }

    public void TriggerAnomaly()
    {
        Debug.Log("공포 소리 이상 현상 발생");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        if (Audio != null) Audio.Stop();

        _currentPlayCount = 0;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (_currentPlayCount < MaxPlayCount)
            {
                if (Audio != null && AudioSound != null)
                {
                    Debug.Log("플레이어가 " + gameObject.name + " 트리거에 접촉");
                    Audio.PlayOneShot(AudioSound);
                    _currentPlayCount++;
                }
            }
        }
    }
}
