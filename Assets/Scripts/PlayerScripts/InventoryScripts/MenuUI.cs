using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    public Button ContinueButton;
    public Button ExitButton;

    // Start is called before the first frame update
    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterMenuUI(this.gameObject);

        if (ContinueButton != null)
        {
            ContinueButton.onClick.RemoveAllListeners();
            ContinueButton.onClick.AddListener(GameManager.Instance.CloseMenu);
        }

        if (ExitButton != null)
        {
            ExitButton.onClick.RemoveAllListeners();
            ExitButton.onClick.AddListener(GameManager.Instance.Quit);
        }
    }
}
