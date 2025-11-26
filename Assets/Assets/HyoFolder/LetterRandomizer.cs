using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterRandomizer : MonoBehaviour
{
    public List<Transform> letters = new List<Transform>();

    void Start()
    {
        StartCoroutine(ShuffleNextFrame());
    }

    IEnumerator ShuffleNextFrame()
    {
        yield return null;

        if (letters == null || letters.Count == 0)
        {
            var drags = GetComponentsInChildren<MonoBehaviour>(true);
            letters = new List<Transform>();
            foreach (var mb in drags)
            {
                if (mb.GetType().Name == "DraggableLetter")
                {
                    letters.Add(mb.transform);
                }
            }
        }

        List<float> xs = new List<float>(letters.Count);
        for (int i = 0; i < letters.Count; i++)
            xs.Add(GetX(letters[i]));

        for (int i = xs.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            float tmp = xs[i];
            xs[i] = xs[r];
            xs[r] = tmp;
        }

        for (int i = 0; i < letters.Count; i++)
        {
            SetX(letters[i], xs[i]);

            var mb = letters[i].GetComponent<MonoBehaviour>();
            if (mb != null && mb.GetType().Name == "DraggableLetter")
            {
                var t = mb.GetType();
                var parentField = t.GetField("originalParent");
                var posField = t.GetField("originalPosition");

                if (parentField != null)
                    parentField.SetValue(mb, letters[i].parent);
                if (posField != null)
                {
                    posField.SetValue(mb, letters[i].position);
                }
            }
            else
            {
                var d = letters[i].GetComponent<DraggableLetter>();
                if (d != null) { d.originalParent = letters[i].parent; d.originalPosition = letters[i].position; }
            }
        }
    }

    float GetX(Transform t)
    {
        var rt = t as RectTransform;
        if (rt != null) return rt.anchoredPosition.x;
        return t.position.x;
    }

    void SetX(Transform t, float x)
    {
        var rt = t as RectTransform;
        if (rt != null)
        {
            Vector2 a = rt.anchoredPosition;
            a.x = x;
            rt.anchoredPosition = a;
        }
        else
        {
            Vector3 p = t.position;
            p.x = x;
            t.position = p;
        }
    }
}
