using System.Collections.Generic;
using DG.Tweening;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : MonoBehaviour, IInputListener
{
    private List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();
    private int _lastText = -1;

    public void Awake()
    {
        InputHandler.SetCurrentListener(this);
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            _texts.Add(transform.GetChild(i).GetComponent<TextMeshProUGUI>());
        }

        FadeInText(0);
    }

    public void OnDestroy()
    {
        InputHandler.UnregisterInputListener(this);
    }

    private void FadeInText(int i)
    {
        foreach (TextMeshProUGUI text in _texts)
        {
            text.color = UiAppearanceController.InvisibleColour;
        }

        Sequence sequence = DOTween.Sequence();
        if (_lastText != -1)
        {
            _texts[_lastText].color = Color.white;
            sequence.Append(_texts[_lastText].DOColor(UiAppearanceController.InvisibleColour, 1f));
        }

        if (i == _texts.Count)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
        
        sequence.Append(_texts[i].DOColor(Color.white, 1f));
        _lastText = i;
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld) return;
        FadeInText(_lastText + 1);
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}