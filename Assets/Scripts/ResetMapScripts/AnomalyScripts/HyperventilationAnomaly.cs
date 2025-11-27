using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HyperventilationAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private Coroutine _breathingCoroutine;
    private bool _hasTriggered = false;

    public AudioSource BreathingAudio;
    public AudioSource HeartBeatAudio;
    public AudioClip[] BreathingSounds;

    public float EffectDuration = 60f;
    public float HeadBobMultiplier = 2.5f;
    public float MaxLensDistortion = -0.5f;
    public float MaxChromaticAberration = 1.0f;
    public float MaxVignetteIntensity = 0.6f;
    public float VertigoIntensity = 5.0f;
    public float VertigoSpeed = 2.0f;

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

        if (CameraManager.Instance != null) CameraManager.Instance.AddedZRot = 0f;

        if (PlayerSanity.Instance != null)
        {
            PlayerSanity.Instance.ResetAllEffects();
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.ShowSubtitle("", 0f);
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
                MaxChromaticAberration, MaxVignetteIntensity, true);

            if (HeartBeatAudio != null) HeartBeatAudio.Play();
            _breathingCoroutine = StartCoroutine(PlayBreathingSounds());

            StartCoroutine(VertigoSequence());
            StartCoroutine(PlayPanicDialogue());
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

    private IEnumerator VertigoSequence()
    {
        if (CameraManager.Instance == null) yield break;

        float timer = 0f;
        while (timer < EffectDuration)
        {
            float zRotation = Mathf.Sin(Time.time * VertigoSpeed) * VertigoIntensity;

            CameraManager.Instance.AddedZRot = zRotation;

            timer += Time.deltaTime;
            yield return null;
        }

        CameraManager.Instance.AddedZRot = 0f;
    }

    private IEnumerator PlayPanicDialogue()
    {
        EventManager.Instance.ShowSubtitle("어..? 갑자기 머리가 왜 이러지..?", 3f);

        yield return new WaitForSeconds(3.0f);
        EventManager.Instance.ShowSubtitle("바닥이... 울렁거려... 중심을 못 잡겠어...", 4f);

        yield return new WaitForSeconds(4.0f);
        EventManager.Instance.ShowSubtitle("허억... 윽... 숨이... 안 쉬어져...", 3f);

        yield return new WaitForSeconds(4.0f);
        EventManager.Instance.ShowSubtitle("어서 이동해야 해..", 3f);
    }
}
