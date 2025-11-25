using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance;

    private Item _itemUsed;
    private GameObject _checkPannel;
    private Slider _checkSlider;
    private RectTransform _successZone;
    private bool _checkRunning = false;
    private float _successMin, _successMax;

    public int TotalSuccessCount = 3;
    public float SliderSpeed = 1.2f;
    public float SuccessZoneWidth = 0.2f;

    public AudioSource Audio;
    public AudioClip CueSound;
    public AudioClip FinalSuccessSound;
    public AudioClip FailSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Audio = GetComponent<AudioSource>();
    }

    private void OnSkillCheckResult(bool success)
    {
        if (success)
        {
            Debug.Log("성공 (정신력 회복)");
            Audio.PlayOneShot(FinalSuccessSound);

            if (_itemUsed != null)
            {
                InventoryManager.Instance.Remove(_itemUsed);
                _itemUsed = null;
            }

            PlayerSanity.Instance.RestoreSanity(30f);
        }

        else
        {
            Debug.Log("실패");
            Audio.PlayOneShot(FailSound);

            if (_itemUsed != null)
            {
                InventoryManager.Instance.Remove(_itemUsed);
                _itemUsed = null;
            }
        }
    }

    public void StartSkillCheck(Item item)
    {
        if (_checkRunning) return;
        this._itemUsed = item;
        StartCoroutine(SkillCheckSequence());
    }
    public void RegisterMiniGameUI(GameObject mini)
    {
        _checkPannel = mini;
        _checkSlider = mini.GetComponentInChildren<Slider>();

        if (_checkSlider != null)
        {
            Transform zoneTransform = _checkSlider.transform.Find("Success_Zone");

            if (zoneTransform != null) _successZone = zoneTransform.GetComponent<RectTransform>();
        }

        _checkPannel.gameObject.SetActive(false);
    }

    private IEnumerator SkillCheckSequence()
    {
        _checkRunning = true;
        _checkPannel.SetActive(true);
        int successCount = 0;

        GameManager.IsPlayerStop = true;

        _successZone.localScale = new Vector3(0.8f, 0.375f, 0.8f);

        while (successCount < TotalSuccessCount)
        {
            Audio.PlayOneShot(CueSound);

            float startZone = Random.Range(0f, 1.0f - SuccessZoneWidth);
            float endZone = startZone + SuccessZoneWidth;

            _successMin = startZone;
            _successMax = endZone;

            _successZone.anchorMin = new Vector2(startZone, 0);
            _successZone.anchorMax = new Vector2(endZone, 1);
            _successZone.offsetMin = Vector2.zero;
            _successZone.offsetMax = Vector2.zero;

            bool inputPressed = false;
            float timer = 0f;
            float currentSliderValue = 0f;

            while (true)
            {
                if (Time.timeScale == 0)
                {
                    yield return null;
                    continue;
                }

                timer += Time.unscaledDeltaTime / SliderSpeed;

                currentSliderValue = Mathf.PingPong(timer, 1f);
                _checkSlider.value = currentSliderValue;

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    inputPressed = true;
                    break;
                }

                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.5f);

            if (inputPressed && currentSliderValue >= _successMin && currentSliderValue <= _successMax)
            {
                successCount++;

                if (successCount >= TotalSuccessCount) break;

                yield return new WaitForSecondsRealtime(0.5f);
            }

            else
            {
                OnSkillCheckResult(false);
                _checkPannel.SetActive(false);
                _checkRunning = false;
                GameManager.IsPlayerStop = false;
                yield break;
            }
        }

        OnSkillCheckResult(true);
        _checkPannel.SetActive(false);
        _checkRunning = false;

        GameManager.IsPlayerStop = false;
    }
}
