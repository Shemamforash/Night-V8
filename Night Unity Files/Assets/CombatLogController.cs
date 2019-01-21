using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class CombatLogController : MonoBehaviour
{
    private static CanvasGroup _canvasGroup;
    private static EnhancedText _enhancedText;
    private static Queue<string> _logs = new Queue<string>();
    private static Tweener _fadeTween;

    public void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _enhancedText = GetComponent<EnhancedText>();
        _logs.Clear();
        _enhancedText.SetText("");
        _canvasGroup.alpha = 0;
    }

    public static void PostLog(string log)
    {
        if (log == null) return;
        if (_logs.Count == 4) _logs.Dequeue();
        _logs.Enqueue(log);
        string logString = "";
        if (_logs.Count >= 1) logString += "<alpha=#22>" + _logs.ElementAt(0);
        if (_logs.Count >= 2) logString += "\n<alpha=#55>" + _logs.ElementAt(1);
        if (_logs.Count >= 3) logString += "\n<alpha=#88>" + _logs.ElementAt(2);
        if (_logs.Count >= 4) logString += "\n<alpha=#bb>" + _logs.ElementAt(3);
        _enhancedText.SetText(logString);
        _canvasGroup.alpha = 1;
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(0, 10f);
    }
}