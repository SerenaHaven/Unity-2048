using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BlockView : MonoBehaviour
{
    private Text text
    {
        get
        {
            if (_text == null) { _text = transform.Find("Text").GetComponent<Text>(); }
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

    public int row;
    public int column;
    public int value;
    public int nextRow;
    public int nextColumn;
    public int nextValue;

    public bool merged
    {
        get { return nextValue != 0 && value != nextValue; }
    }

    public bool moved
    {
        get { return row != nextRow || column != nextColumn; }
    }

    public void SetSize(float size)
    {
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size);
        (transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
    }

    private void Set()
    {
        int index = (int)Mathf.Log(value, 2) - 1;
        text.text = value.ToString();
        text.color = index < 2 ? Config.TextColor1 : Config.TextColor2;
        image.color = Config.BlockColors[index % Config.BlockColors.Length];
    }

    public void Apply()
    {
        row = nextRow;
        column = nextColumn;
        value = nextValue;
        Set();
    }

    public void Set(int row, int column, int value)
    {
        this.row = row;
        this.column = column;
        this.value = value;
        Set();
    }

    public void SetNext(int nextRow, int nextColumn, int nextValue)
    {
        this.nextRow = nextRow;
        this.nextColumn = nextColumn;
        this.nextValue = nextValue;
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
            value += Config.BlockAppearSpeed * Time.deltaTime;
            canvasGroup.alpha += Config.BlockAppearSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * value;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}