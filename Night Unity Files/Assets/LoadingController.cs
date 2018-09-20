using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class LoadingController : MonoBehaviour
{
    private ParticleSystem _dots, _trail;
    private CanvasGroup _canvasGroup;
    private float _lastAlpha = -1;
    private static GameObject _loadingScreen;

    public void Awake()
    {
        _dots = gameObject.FindChildWithName<ParticleSystem>("Dots");
        _trail = gameObject.FindChildWithName<ParticleSystem>("Trail");
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        _loadingScreen = gameObject.FindChildWithName("Loading Screen");
        _loadingScreen.SetActive(false);
    }

    public void Update()
    {
        if (!_loadingScreen.activeInHierarchy) return;
        float alpha = _canvasGroup.alpha;
        if (_canvasGroup.alpha == 0 && _lastAlpha == 0) return;
        _lastAlpha = alpha;
        Color color = new Color(1, 1, 1, alpha);
        ParticleSystem.MainModule main = _dots.main;
        main.startColor = color;
        ParticleSystem.TrailModule trails = _trail.trails;
        trails.colorOverLifetime = color;
    }

    public static void SetLoadingScreenActive()
    {
        _loadingScreen.SetActive(true);
    }
}