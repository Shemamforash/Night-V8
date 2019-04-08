using DG.Tweening;
using SamsHelper.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CloseButtonController : ControlTypeChangeListener, IInputListener
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _glowImage;

    private InputAxis _targetAxis = InputAxis.Cancel;

    public void Start()
    {
        _button.targetGraphic = _glowImage;
        switch (_targetAxis)
        {
            case InputAxis.Cancel:
                UseDefaultInput();
                break;
            case InputAxis.Accept:
                UseAcceptInput();
                break;
            case InputAxis.Sprint:
                UseSpaceInput();
                break;
        }

        SetOnControllerInputChange(UpdateText);
    }

    private void UpdateText()
    {
        string text = InputHandler.GetBindingForKey(_targetAxis);
        if (text.Length > 4 && _targetAxis == InputAxis.Cancel) text = "C";
        if (_targetAxis == InputAxis.Sprint) text = text.Contains("Left") ? "LBMP" : "SPC";
        _buttonText.SetText(text);
    }

    public void SetOnClick(UnityAction a)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(a);
    }

    public void UseAcceptInput()
    {
        _targetAxis = InputAxis.Accept;
        UpdateText();
    }

    public void UseSpaceInput()
    {
        _targetAxis = InputAxis.Sprint;
        UpdateText();
    }

    private void UseDefaultInput()
    {
        _targetAxis = InputAxis.Cancel;
        UpdateText();
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
        ExecuteEvents.Execute(_button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }

    public Button Button() => _button;

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
}