﻿using System;
using DG.Tweening;
using SamsHelper.Input;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonController : MonoBehaviour, IInputListener
{
    private Image _glowImage;
    private TextMeshProUGUI _text;
    private InputAxis _listeningAxis = InputAxis.Cover;
    private Action _callback;

    public void Awake()
    {
        _glowImage = gameObject.FindChildWithName<Image>("Glow");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Close Text");
    }

    public void SetInputAxis(InputAxis axis)
    {
        _listeningAxis = axis;
    }

    public void SetCallback(Action callback)
    {
        _callback = callback;
    }

    public void Enable()
    {
        InputHandler.RegisterInputListener(this);
    }

    public void Disable()
    {
        InputHandler.UnregisterInputListener(this);
    }

    public void SetText(string text)
    {
        _text.SetText(text);
    }

    public void Activate()
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
        if (axis != _listeningAxis || isHeld) return;
        Activate();
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}