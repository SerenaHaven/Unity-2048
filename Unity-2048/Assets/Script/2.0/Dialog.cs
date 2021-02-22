using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    private Button _buttonYes;
    private Button _buttonNo;
    private Text _textMessage;

    private Action _onYes;
    private Action _onNo;

    void Awake()
    {
        _buttonYes = transform.Find("BG/BG/Buttons/ButtonYes").GetComponent<Button>();
        _buttonYes.onClick.AddListener(() => { if (_onYes != null) { _onYes(); } });

        _buttonNo = transform.Find("BG/BG/Buttons/ButtonNo").GetComponent<Button>();
        _buttonNo.onClick.AddListener(() => { if (_onNo != null) { _onNo(); } });
        _buttonNo.onClick.AddListener(Hide);

        _textMessage = transform.Find("BG/BG/TextMessage").GetComponent<Text>();
    }

    public void Show(string message, Action onYes = null, Action onNo = null)
    {
        _textMessage.text = message;
        gameObject.SetActive(true);
        _onYes = onYes;
        _onNo = onNo;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        _onYes = null;
        _onNo = null;
    }
}