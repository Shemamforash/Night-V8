using System;
using DG.Tweening;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CloseButtonController : MonoBehaviour, IInputListener, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Button _button;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _glowImage;
    
    private InputAxis _targetAxis = InputAxis.Menu;
    private Action _callback;
    private bool _usingFireInput;

    public void Awake()
    {
        if (!_usingFireInput) UseDefaultInput();
        else _targetAxis = InputAxis.Sprint;
    }


    public void SetOnClick(UnityAction a)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(a);
    }

    public void SetCallback(Action callback)
    {
        _callback = callback;
    }

    public void UseFireInput()
    {
        _targetAxis = InputAxis.Sprint;
        _buttonText.SetText("SPC");
        _usingFireInput = true;
    }

    private void UseDefaultInput()
    {
        _targetAxis = InputAxis.Menu;
        if (_buttonText == null) Debug.Log("null");
        _buttonText.SetText("ESC");
        _usingFireInput = false;
    }

    public void Enable()
    {
        InputHandler.RegisterInputListener(this);
    }

    public void Disable()
    {
        InputHandler.UnregisterInputListener(this);
    }

    private void Activate()
    {
        Flash();
        _callback?.Invoke();
    }

    public void Flash()
    {
        _glowImage.color = Color.white;
        _glowImage.DOFade(0f, 0.5f);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != _targetAxis || isHeld) return;
        Activate();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _glowImage.DOFade(0.5f, 0.25f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _glowImage.DOFade(0f, 0.25f);
    }
}