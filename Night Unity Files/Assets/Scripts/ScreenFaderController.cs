﻿using DG.Tweening;
using Game.Characters;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFaderController : MonoBehaviour
{
    private static Image _faderImage;
    private static CanvasGroup _faderCanvas, _textCanvas;
    private static TextMeshProUGUI _text;
    private static Sequence _sequence;

    private void Awake()
    {
        _faderCanvas = GetComponent<CanvasGroup>();
        _faderImage = GetComponent<Image>();
        _faderCanvas.alpha = 1f;
        _faderImage.color = Color.black;
        _textCanvas = gameObject.FindChildWithName<CanvasGroup>("Text Canvas");
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
        bool isCombat = SceneManager.GetActiveScene().name == "Combat";
        float duration = isCombat ? 3f : 0.5f;
        float pause = isCombat ? 1f : 0.5f;
        _sequence = DOTween.Sequence();
        _sequence.AppendInterval(pause);
        _sequence.Append(_faderCanvas.DOFade(0, duration));
        _sequence.SetUpdate(UpdateType.Normal, true);
    }

    private void Start()
    {
        if (CombatManager.Instance() == null) return;
        string text = CharacterManager.CurrentRegion().Name;
        if (text == "") return;
        _text.text = text;
        _textCanvas.alpha = 1;
    }

    public static void ShowText(string text)
    {
        _text.text = text;
        _textCanvas.alpha = 1;
        _faderCanvas.alpha = 1;
        _faderImage.color = new Color(1, 1, 1, 0f);
        _faderCanvas.DOFade(0f, 5f);
    }

    public static Sequence FadeIn(float duration)
    {
        ResetFader();
        _faderImage.SetAlpha(1f);
        _sequence = DOTween.Sequence();
        _sequence.Append(_faderCanvas.DOFade(1, duration).SetUpdate(UpdateType.Normal, true));
        return _sequence;
    }

    private static void ResetFader()
    {
        _textCanvas.alpha = 0;
        _text.text = "";
        _sequence?.Kill();
        Color resetColor = Color.black;
        resetColor.a = _faderImage.color.a;
        _faderImage.color = resetColor;
    }

    public static void FlashWhite(float duration, Color to)
    {
        ResetFader();
        _faderImage.color = Color.white;
        _faderCanvas.alpha = 1;
        _faderImage.DOColor(to, duration).SetUpdate(UpdateType.Normal, true);
    }
}