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

    private Image image
    {
        get
        {
            if (_image == null) { _image = GetComponent<Image>(); }
            return _image;
        }
    }
    private Image _image;

    private CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null) { _canvasGroup = GetComponent<CanvasGroup>(); }
            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup;

    private const float AppearSpeed = 8f;

    public void SetSize(float size)
    {
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    public void Set(int value)
    {
        int index = (int)Mathf.Log(value, 2) - 1;
        text.text = value.ToString();
        text.color = index < 2 ? Config.TextColor1 : Config.TextColor2;
        image.color = Config.BlockColors[index % Config.BlockColors.Length];
    }

    public void Reset()
    {
        canvasGroup.alpha = 1.0f;
        transform.localScale = Vector3.one;
    }

    public void AnimateAppear()
    {
        StopAllCoroutines();
        StartCoroutine(ProcessAppear());
    }

    private IEnumerator ProcessAppear()
    {
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0;
        float value = 0.0f;
        while (transform.localScale.x < 0.9f)
        {
            value += AppearSpeed * Time.deltaTime;
            canvasGroup.alpha += AppearSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * value;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}