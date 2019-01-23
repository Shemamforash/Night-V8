using DG.Tweening;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class CombatLogController : MonoBehaviour
{
    private static CanvasGroup _canvasGroup;
    private static EnhancedText _enhancedText;
    private static string[] _logs = {"", "", "", ""};
    private static Tweener _fadeTween;
    private static readonly string[] _alphaPrefixes = {"<alpha=#bb>", "<alpha=#88>", "<alpha=#55>", "<alpha=#22>"};

    public void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _enhancedText = GetComponent<EnhancedText>();
        _logs = new[] {"", "", "", ""};
        _enhancedText.SetText("");
        _canvasGroup.alpha = 0;
    }

    public static void PostLog(string log)
    {
        if (log == null) return;
        AppendLog(log);
        string logString = "";
        for (int i = _logs.Length - 1; i >= 0; --i)
        {
            logString += _alphaPrefixes[i] + _logs[i];
            if (i > 0) logString += "\n";
        }
        _enhancedText.SetText(logString);
        _canvasGroup.alpha = 1;
        _fadeTween?.Kill();
        _fadeTween = _canvasGroup.DOFade(0, 10f);
    }

    private static void AppendLog(string log)
    {
        for (int i = _logs.Length - 1; i > 0; --i) _logs[i] = _logs[i - 1];
        _logs[0] = log;
    }
}