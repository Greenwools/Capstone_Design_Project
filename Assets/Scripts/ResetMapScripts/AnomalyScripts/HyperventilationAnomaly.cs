using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HyperventilationAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private bool _hasTriggered = false;
    private Coroutine _breathingCoroutine;

    public float EffectDuration = 60f;
    public float HeadBobMultiplier = 2.5f;
    public float MaxLensDistortion = -0.5f;
    public float MaxChromaticAberration = 1.0f;
    public float MaxVignetteIntensity = 0.6f;

    public AudioSource BreathingAudio;
    public AudioSource HeartBeatAudio;
    public AudioClip[] BreathingSounds;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;

        if (BreathingAudio != null) BreathingAudio.loop = true;
        if (HeartBeatAudio != null) HeartBeatAudio.loop = true;
    }

    public void TriggerAnomaly()
    {
        Debug.Log("과호흡 이상 현상 발생");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        if (_breathingCoroutine != null) StopCoroutine(_breathingCoroutine); 
        if (BreathingAudio != null) BreathingAudio.Stop();
        if (HeartBeatAudio != null) HeartBeatAudio.Stop();

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.ResetAllEffects();
        }

        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;

            PlayerSanity.Instance.StartPanicEffect(EffectDuration, HeadBobMultiplier, MaxLensDistortion,
                MaxChromaticAberration, MaxVignetteIntensity);

            if (HeartBeatAudio != null) HeartBeatAudio.Play();
            _breathingCoroutine = StartCoroutine(PlayBreathingSounds());
        }
    }

    private IEnumerator PlayBreathingSounds()
    {
        if (BreathingSounds.Length == 0 || BreathingAudio == null) yield break;

        while (true)
        {
            AudioClip clip = BreathingSounds[Random.Range(0, BreathingSounds.Length)];
            BreathingAudio.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }
}
