using Game.Exploration.Environment;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class SceneryController : MonoBehaviour
{
    private static Image _environment, _sunGlow, _sunGlowNarrow, _gateShadow;
    private static RectTransform _sun, _sky;
    private const float SunYMax = 450;
    private const float SkyYMax = 800;

    private static readonly Color _environmentDarkColor = new Color(0.05f, 0.05f, 0.05f);
    private static readonly Color _glowMaxColor = new Color(1, 0, 0, 0.5f);

    public void Awake()
    {
        _sky = gameObject.FindChildWithName<RectTransform>("Sky");
        _sun = gameObject.FindChildWithName<RectTransform>("Sun");
        _environment = gameObject.FindChildWithName<Image>("Landscape");
        _gateShadow = gameObject.FindChildWithName<Image>("Gate Shadow");
        _sunGlow = gameObject.FindChildWithName<Image>("Sun Glow Wide");
        _sunGlowNarrow = gameObject.FindChildWithName<Image>("Sun Glow Small");
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
            _sunGlowNarrow.color = UiAppearanceController.InvisibleColour;
            _environment.color = _environmentDarkColor;
            _gateShadow.color = UiAppearanceController.InvisibleColour;
        }
        else
        {
            _sunGlow.color = Color.Lerp(UiAppearanceController.InvisibleColour, _glowMaxColor, sinTime);
            _sunGlowNarrow.color = _sunGlow.color;
            _environment.color = Color.Lerp(new Color(0.05f, 0.05f, 0.05f), Color.white, sinTime);
            _gateShadow.color = Color.Lerp(UiAppearanceController.InvisibleColour, Color.white, sinTime);
        }
    }

    public static void UpdateEnvironmentBackground()
    {
        if (_environment == null) return;
        Environment currentEnvironment = EnvironmentManager.CurrentEnvironment;
        if (currentEnvironment == null) return;
        _environment.sprite = Resources.Load<Sprite>("Images/Backgrounds/" + currentEnvironment.EnvironmentType + "/Environment");
        _gateShadow.sprite = Resources.Load<Sprite>("Images/Backgrounds/" + EnvironmentManager.CurrentEnvironmentType() + "/Shadow");
    }
}