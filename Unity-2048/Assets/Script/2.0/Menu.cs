using System;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private Button _button4;
    private Button _button5;
    private Button _button6;
    private Button _buttonClose;
    private Button _buttonQuit;

    public Action onButton4;
    public Action onButton5;
    public Action onButton6;
    public Action onButtonQuit;

    private Animator _animator;

    void Awake()
    {
        _buttonClose = transform.Find("BG/ButtonClose").GetComponent<Button>();
        _buttonClose.onClick.AddListener(() => { Hide(); });

        _button4 = transform.Find("BG/BG/Button4").GetComponent<Button>();
        _button4.onClick.AddListener(() => { if (onButton4 != null) { onButton4.Invoke(); } });

        _button5 = transform.Find("BG/BG/Button5").GetComponent<Button>();
        _button5.onClick.AddListener(() => { if (onButton5 != null) { onButton5.Invoke(); } });

        _button6 = transform.Find("BG/BG/Button6").GetComponent<Button>();
        _button6.onClick.AddListener(() => { if (onButton6 != null) { onButton6.Invoke(); } });

        _buttonQuit = transform.Find("BG/BG/ButtonQuit").GetComponent<Button>();
        _buttonQuit.onClick.AddListener(() => { if (onButtonQuit != null) { onButtonQuit.Invoke(); } });

        _animator = GetComponent<Animator>();
        _animator.enabled = false;
    }

    public void Show(bool withClose = true)
    {
        _buttonClose.gameObject.SetActive(withClose);
        _animator.enabled = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}