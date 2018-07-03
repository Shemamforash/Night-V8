using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

public class SceneryController : MonoBehaviour
{
    private static Image _sky, _sun, _light, _gate, _shadow;
    private static Color _lightMinColour = new Color(0.3f, 0f, 0f, 1f);
    private const float SunYMax = -265, SunYMin = -1500;
    private const float SkyYMax = -1080, SkyYMin = 1080;

    public void Awake()
    {
        _sky = Helper.FindChildWithName<Image>(gameObject, "Sky");
        _sun = Helper.FindChildWithName<Image>(gameObject, "Sun");
        _light = Helper.FindChildWithName<Image>(gameObject, "Light");
        _gate = Helper.FindChildWithName<Image>(gameObject, "Gate");
        _shadow = Helper.FindChildWithName<Image>(gameObject, "Shadow");
    }

    public static void SetTime(float normalisedTime) //6am = 0 6pm = 0.5
    {
        normalisedTime -= 0.25f;
        if (normalisedTime < 0) normalisedTime += 1;
        float sinTime = Mathf.Sin(normalisedTime * 2 * Mathf.PI);
        bool isNegative = sinTime < 0;
        sinTime *= sinTime;
        if (isNegative) sinTime = -sinTime;
        float sunHeight = Mathf.Lerp(SunYMin, SunYMax, sinTime);
//        _sun.rectTransform.offsetMin = new Vector2(0, sunHeight);
//        _sun.rectTransform.offsetMax = new Vector2(0, sunHeight);
        _sun.rectTransform.anchoredPosition = new Vector2(0, sunHeight);
        float skyPos = Mathf.Lerp(SkyYMax, SkyYMin, sinTime);
        _sky.rectTransform.offsetMin = new Vector2(0, skyPos);
        _sky.rectTransform.offsetMax = new Vector2(0, skyPos);

        sinTime /= 2f;
        sinTime += 0.5f;
        _light.color = Color.Lerp(_lightMinColour, Color.red, sinTime);
        _shadow.color = Color.Lerp(new Color(100, 0, 100), Color.red, sinTime);
        _gate.color = Color.Lerp(Color.red / 2f, Color.red, sinTime);
    }
}