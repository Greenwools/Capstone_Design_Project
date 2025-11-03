using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScareAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private Vector3 _doppleInitialPos;
    private Quaternion _doppleInitialRot;
    private bool _hasTriggered = false;

    public GameObject FlickeringLightObject;
    public GameObject DoppleModel;
    public DoorInteraction[] DoorsLock;
    public Transform PlayerTransform;

    public AudioSource JumpScareAudio;
    public AudioSource HeartBeatAudio;
    public AudioClip JumpScareSound;
    public AudioClip HeartBeatSound;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (PlayerTransform == null) PlayerTransform = GameObject.FindWithTag("Player").transform;

        if (DoppleModel != null)
        {
            _doppleInitialPos = DoppleModel.transform.position;
            _doppleInitialRot = DoppleModel.transform.rotation;
        }
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
        if (DoppleModel != null)
        {
            DoppleModel.transform.position = _doppleInitialPos;
            DoppleModel.transform.rotation = _doppleInitialRot;
            DoppleModel.SetActive(false);
        }

        foreach (DoorInteraction door in DoorsLock)
        {
            if (door != null) door.enabled = true;
        }

        if (JumpScareAudio != null) JumpScareAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();

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
            DoppleModel.transform.LookAt(PlayerTransform);
            Vector3 targetPos = PlayerTransform.position + PlayerTransform.forward * 1.0f;
            targetPos.y = PlayerTransform.position.y - 0.75f;
            DoppleModel.transform.position = targetPos;

            
        }

        if (JumpScareAudio != null) JumpScareAudio.PlayOneShot(JumpScareSound);

        DecreaseSansity();

        yield return new WaitForSeconds(0.5f);

        if (HeartBeatAudio != null) HeartBeatAudio.PlayOneShot(HeartBeatSound);

        yield return new WaitForSeconds(1.0f);

        GameManager.IsPlayerStop = false;

        if (DoppleModel != null) DoppleModel.SetActive(false);
    }

    private void DecreaseSansity()
    {

    }
}
