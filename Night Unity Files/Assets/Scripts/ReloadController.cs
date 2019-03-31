using DG.Tweening;
using SamsHelper.Input;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadController : MonoBehaviour
{
    private Image _glow, _progress;
    private CanvasGroup _canvasGroup;
    private static ReloadController _instance;
    private ControlTypeChangeListener _controlTypeChangeListener;
    private TextMeshProUGUI _reloadText;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
        _glow = gameObject.FindChildWithName<Image>("Glow");
        _progress = gameObject.FindChildWithName<Image>("Progress");
        _reloadText = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
        _canvasGroup.alpha = 0f;
        _glow.SetAlpha(0f);
        _instance = this;
    }

    private void Start()
    {
        _controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
    }

    private void UpdateText()
    {
        _reloadText.text = InputHandler.GetBindingForKey(InputAxis.Reload);
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    public static ReloadController Instance() => _instance;

    public void Show()
    {
        _canvasGroup.alpha = 1f;
        _glow.SetAlpha(0.5f);
        _glow.DOFade(0f, 1f);
        _progress.fillAmount = 0f;
    }

    public void SetProgress(float normalisedAmount)
    {
        _progress.fillAmount = normalisedAmount;
    }

    public void Complete()
    {
        _progress.fillAmount = 1f;
        _glow.SetAlpha(1f);
        _canvasGroup.DOFade(0f, 1f);
    }
}