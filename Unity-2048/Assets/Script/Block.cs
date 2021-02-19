using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    private Text text
    {
        get
        {
            if (_text == null) { _text = GetComponentInChildren<Text>(); }
            return _text;
        }
    }
    private Text _text;

    private float _appearSpeed = 5.0f;

    public void SetSize(float size)
    {
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    public void Set(int value)
    {
        text.text = value == 0 ? null : value.ToString();
    }

    public void AnimateAppear()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(ProcessAppear());
    }

    private IEnumerator ProcessAppear()
    {
        transform.localScale = Vector3.zero;
        float scale = 0.0f;
        while (transform.localScale.x < 0.9f)
        {
            scale += _appearSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}