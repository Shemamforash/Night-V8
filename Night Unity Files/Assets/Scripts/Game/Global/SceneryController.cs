using Game.Exploration.Environment;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class SceneryController : MonoBehaviour
{
    private static Image _environment, _sunGlow, _gateShadow, _fireShadow;
    private static RectTransform _sun, _sky;
    private static Color _lightMinColour = new Color(0.3f, 0f, 0f, 1f);
    private const float SunYMax = 450;
    private const float SkyYMax = 800;

    private static Color _environmentDarkColor = new Color(0.05f, 0.05f, 0.05f);
    private static Color _glowMaxColor = new Color(1, 0, 0, 0.5f);

    public void Awake()
    {
        _sky = gameObject.FindChildWithName<RectTransform>("Sky");
        _sun = gameObject.FindChildWithName<RectTransform>("Sun");
        _environment = gameObject.FindChildWithName<Image>("Landscape");
        _gateShadow = gameObject.FindChildWithName<Image>("Gate Shadow");
        _fireShadow = gameObject.FindChildWithName<Image>("Fire Shadow");
        _sunGlow = gameObject.FindChildWithName<Image>("Sun Glow");
        UpdateEnvironmentBackground();
    }

    public static void SetTime(float normalisedTime) //6am = 0 6pm = 0.5
    {
        normalisedTime -= 0.25f;
        float sinTime = Mathf.Sin(normalisedTime / 0.5f * Mathf.PI);
        float sunPos = SunYMax * sinTime;
        _sun.anchoredPosition = new Vector2(0, sunPos);
        
        float skyPos = SkyYMax * sinTime;
        _sky.anchoredPosition = new Vector2(0, skyPos);

        if (sinTime < 0f)
        {
            _sunGlow.color = UiAppearanceController.InvisibleColour;
            _environment.color = _environmentDarkColor;
            _gateShadow.color = UiAppearanceController.InvisibleColour;
            _fireShadow.color = UiAppearanceController.InvisibleColour;
        }
        else
        {
            _sunGlow.color = Color.Lerp(UiAppearanceController.InvisibleColour, _glowMaxColor, sinTime);
            _environment.color = Color.Lerp(new Color(0.05f, 0.05f, 0.05f), Color.white, sinTime);
            _gateShadow.color = Color.Lerp(UiAppearanceController.InvisibleColour, Color.white, sinTime);
            _fireShadow.color = Color.Lerp(UiAppearanceController.InvisibleColour, Color.white, sinTime);
        }
    }

    public static void UpdateEnvironmentBackground()
    {
        if (_environment == null) return;
        Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
        if (currentEnvironment == null) return;
        _environment.sprite = Resources.Load<Sprite>("Images/Backgrounds/" + currentEnvironment.EnvironmentType + "/Environment");
        _environment.sprite = Resources.Load<Sprite>("Images/Backgrounds/" + EnvironmentManager.CurrentEnvironment.EnvironmentType + "/Environment");
    }
}