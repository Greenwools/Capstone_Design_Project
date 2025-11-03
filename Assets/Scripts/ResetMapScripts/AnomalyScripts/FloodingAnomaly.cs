using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloodingAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private PlayerController _playerController;
    private bool _anomalyTriggered = false;
    private Material _waterMaterial;
    private float _initialWaterLevel;

    public GameObject WaterVolume;
    public float DrownWaterHeight = 3.0f;
    public float TimeToDrown = 30.0f;

    public AudioSource WaterAudio;
    public AudioSource BreathingAudio;
    public AudioSource HeartBeatAudio;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
        _playerController = FindObjectOfType<PlayerController>();

        if (WaterVolume != null) 
        { 
            Renderer waterRenderer = WaterVolume.GetComponent<Renderer>();
            _waterMaterial = waterRenderer.material;

            _initialWaterLevel = -1f;
            _waterMaterial.SetFloat("_waterLevel", _initialWaterLevel);
        }

        if (WaterAudio == null) WaterAudio = GetComponent<AudioSource>();
        WaterAudio.playOnAwake = false;
        WaterAudio.loop = true;

        if (BreathingAudio != null) BreathingAudio.loop = true;
        if (HeartBeatAudio != null) HeartBeatAudio.loop = true;
    }

    public void TriggerAnomaly()
    {
        Debug.Log("연출형 이상 현상(FloodingAnomaly) 발생");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        WaterAudio.Stop();
        if (BreathingAudio != null) BreathingAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();

        if (WaterVolume != null)
        {
            _waterMaterial.SetFloat("_waterLevel", _initialWaterLevel);
            WaterVolume.SetActive(false);
        }

        if (_playerController != null) _playerController.IsInWater = false;
        _anomalyTriggered = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_anomalyTriggered)
        {
            _anomalyTriggered = true;
            StartCoroutine(FloodSequence());
        }
    }

    private IEnumerator FloodSequence()
    {
        WaterAudio.Play();
        if (BreathingAudio != null) BreathingAudio.Play();
        if (HeartBeatAudio != null) HeartBeatAudio.Play();

        if (_playerController != null) _playerController.IsInWater = true;

        if (WaterVolume != null)
        {
            WaterVolume.SetActive(true);
            float currentHeight = _initialWaterLevel;
            float elapsedTime = 0f;

            while (elapsedTime < TimeToDrown) 
            {
                currentHeight = Mathf.Lerp(_initialWaterLevel, DrownWaterHeight, elapsedTime / TimeToDrown);

                _waterMaterial.SetFloat("_waterLevel", currentHeight);

                if (_playerController != null) _playerController.CurrentWaterLevel = currentHeight;

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _waterMaterial.SetFloat("_waterLevel", DrownWaterHeight);
        }
    }
}
