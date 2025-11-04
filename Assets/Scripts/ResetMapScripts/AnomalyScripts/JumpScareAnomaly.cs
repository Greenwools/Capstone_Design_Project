using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScareAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private AudioSource _audioSource;
    private bool _hasTriggered = false;

    public GameObject FlickeringLightObject;
    public GameObject DoppleModel;
    public DoorInteraction[] DoorsLock;
    public Transform PlayerTransform;

    public AudioClip JumpScareSound;
    public AudioClip HeartBeatSound;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;

        if (PlayerTransform == null) PlayerTransform = GameObject.FindWithTag("Player").transform;
    }

    public void TriggerAnomaly()
    {
        Debug.Log("연출형 이상 현상(JumpScareAnomaly) 활성화");

        gameObject.SetActive(true);

        if (FlickeringLightObject != null) FlickeringLightObject.SetActive(true);
        if (DoppleModel != null) DoppleModel.SetActive(true);

        foreach (DoorInteraction door in DoorsLock)
        {
            if (door != null) door.enabled = false;
        }
    }

    public void ResetState()
    {
        if (FlickeringLightObject != null) FlickeringLightObject.SetActive(false);
        if (DoppleModel != null) DoppleModel.SetActive(false);

        foreach (DoorInteraction door in DoorsLock)
        {
            if (door != null) door.enabled = true;
        }

        _audioSource.Stop();
        StopAllCoroutines();
        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;
            StartCoroutine(JumpscareEvent());
        }
    }

    private IEnumerator JumpscareEvent()
    {
        Debug.Log("점프스케어 발생");

        GameManager.IsPlayerStop = true;

        if (DoppleModel != null && PlayerTransform != null)
        {
            Vector3 targetPos = PlayerTransform.position + PlayerTransform.forward * 1.0f;
            targetPos.y = PlayerTransform.position.y;
            DoppleModel.transform.position = targetPos;

            DoppleModel.transform.LookAt(PlayerTransform);
        }

        _audioSource.Stop();
        _audioSource.PlayOneShot(JumpScareSound);
        _audioSource.PlayOneShot(HeartBeatSound);

        DecreaseSansity();

        yield return new WaitForSeconds(1.5f);

        GameManager.IsPlayerStop = false;

        if (DoppleModel != null) DoppleModel.SetActive(false);
    }

    private void DecreaseSansity()
    {

    }
}
