using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogAnomaly : MonoBehaviour, IAnomaly, IResetable
{
    private Color _originalFogColor;
    private FogMode _originalFogMode;
    private float _originalFogDensity;
    private bool _originalFogEnabled;
    private bool _hasTriggered = false;

    [Header("설정")]
    public Color FogColor = new Color(0.1f, 0.1f, 0.1f);
    public float TargetDensity = 0.2f;
    public float Speed = 0.5f;
    public AudioSource WindAudio;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        _originalFogEnabled = RenderSettings.fog;
        _originalFogColor = RenderSettings.fogColor;
        _originalFogDensity = RenderSettings.fogDensity;
        _originalFogMode = RenderSettings.fogMode;

        if (WindAudio != null) WindAudio.loop = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_hasTriggered)
        {
            _hasTriggered = true;
            StartCoroutine(FogSequence());
            StartCoroutine(PlayFogDialogue());
        }
    }

    public void TriggerAnomaly()
    {
        Debug.Log("연출형 이상 현상(Fog) 활성화");
        gameObject.SetActive(true);
    }

    public void ResetState()
    {
        StopAllCoroutines();

        RenderSettings.fog = _originalFogEnabled;
        RenderSettings.fogColor = _originalFogColor;
        RenderSettings.fogDensity = _originalFogDensity;
        RenderSettings.fogMode = _originalFogMode;

        if (WindAudio != null) WindAudio.Stop();

        if (EventManager.Instance != null) EventManager.Instance.ShowSubtitle("", 0f);

        _hasTriggered = false;
        gameObject.SetActive(false);
    }

    private IEnumerator FogSequence()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = _originalFogColor;
        RenderSettings.fogDensity = _originalFogEnabled ? _originalFogDensity : 0f;

        if (WindAudio != null) WindAudio.Play();

        float timer = 0f;
        float duration = 5.0f;

        while (timer < duration)
        {
            float t = timer / duration;

            RenderSettings.fogColor = Color.Lerp(_originalFogColor, FogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(_originalFogEnabled ? _originalFogDensity : 0f, TargetDensity, t);

            timer += Time.deltaTime;
            yield return null;
        }

        RenderSettings.fogColor = FogColor;
        RenderSettings.fogDensity = TargetDensity;
    }

    private IEnumerator PlayFogDialogue()
    {
        yield return new WaitForSeconds(1.0f);
        EventManager.Instance.ShowSubtitle("...갑자기 공기가 차가워졌어.", 3f);

        // 시야가 가려질 때쯤
        yield return new WaitForSeconds(3.0f);
        EventManager.Instance.ShowSubtitle("안개..? 앞이 제대로 보이지 않아...", 3f);

        // 고립감이 심해질 때
        yield return new WaitForSeconds(3.0f);
        EventManager.Instance.ShowSubtitle("마치... 세상에 나 혼자 남겨진 기분이야.", 4f);
    }
}
