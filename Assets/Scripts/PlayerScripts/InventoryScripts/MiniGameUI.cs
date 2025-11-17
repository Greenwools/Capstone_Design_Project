using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniGameUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (MiniGameManager.Instance != null)
            MiniGameManager.Instance.RegisterMiniGameUI(this.gameObject);
    }
}
