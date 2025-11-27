using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelChairAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private bool _isMoving = false;
    private AudioSource _audioSource;

    public AudioClip RollingSound;
    public GameObject WheelchairModel; 
    public Transform StartPoint;       
    public Transform EndPoint;         
    public float MoveSpeed = 1.5f;     

    void Awake()
    {
        _audioSource = WheelchairModel.GetComponent<AudioSource>();
        if (_audioSource == null ) _audioSource = WheelchairModel.AddComponent<AudioSource>();

        _audioSource.loop = true;
        _audioSource.spatialBlend = 1.0f;
        _audioSource.dopplerLevel = 0f;
        _audioSource.minDistance = 1.0f;
        _audioSource.maxDistance = 15.0f;
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        if (RollingSound != null) RollingSound.LoadAudioData();

        if (WheelchairModel != null)
        {
            _initialPosition = WheelchairModel.transform.position;
            _initialRotation = WheelchairModel.transform.rotation;
            WheelchairModel.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!_isMoving || WheelchairModel == null || EndPoint == null) return;

        float step = MoveSpeed * Time.deltaTime;
        WheelchairModel.transform.position = Vector3.MoveTowards(WheelchairModel.transform.position, EndPoint.position, step);

        if (Vector3.Distance(WheelchairModel.transform.position, EndPoint.position) < 0.1f)
        {
            _isMoving = false;
            if (_audioSource != null) _audioSource.Stop();
        }
    }

    public void TriggerAnomaly()
    {
        if (WheelchairModel == null || StartPoint == null || EndPoint == null) return;

        Debug.Log("휠체어 이동 이상 현상 발생");

        gameObject.SetActive(true);
        WheelchairModel.SetActive(true);
        WheelchairModel.transform.position = StartPoint.position;
        WheelchairModel.transform.LookAt(EndPoint);

        if (RollingSound != null)
        {
            _audioSource.clip = RollingSound;
            _audioSource.Play();
        }

        _isMoving = true;
    }

    public void ResetState()
    {
        _isMoving = false;

        if (_audioSource != null) _audioSource.Stop();

        if (WheelchairModel != null)
        {
            WheelchairModel.transform.position = _initialPosition;
            WheelchairModel.transform.rotation = _initialRotation;
            WheelchairModel.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}
