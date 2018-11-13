using System.Collections;
using System.Threading;
using DG.Tweening;
using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

public class SaveIconController : MonoBehaviour
{
    private Image _glow;
    private ParticleSystem _spinParticles;
    private static GameObject _spinnerPrefab;

    public void Awake()
    {
        _glow = gameObject.FindChildWithName<Image>("Sprite");
        _spinParticles = gameObject.FindChildWithName<ParticleSystem>("Particles");
        _glow.color = UiAppearanceController.InvisibleColour;
        GetComponent<Canvas>().worldCamera = Camera.main;
        StartCoroutine(Spin());
    }

    public static void Save()
    {
        if (_spinnerPrefab == null) _spinnerPrefab = Resources.Load<GameObject>("Prefabs/Save Spinner");
        Instantiate(_spinnerPrefab);
    }

    private IEnumerator Spin()
    {
        Thread thread = new Thread(SaveController.SaveGame);
        thread.Start();
        ParticleSystem.MainModule main = _spinParticles.main;
        main.startColor = Color.white;
        _spinParticles.Play();
        float minTime = 1f;
        while (minTime > 0f && thread.IsAlive)
        {
            minTime -= Time.deltaTime;
            yield return null;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(_glow.DOColor(UiAppearanceController.FadedColour, 0.4f).SetEase(Ease.InExpo));
        sequence.Append(_glow.DOColor(UiAppearanceController.InvisibleColour, 1f).SetEase(Ease.Linear));
        sequence.AppendCallback(() => Destroy(gameObject));

        minTime = 1f;
        while (minTime > 0f)
        {
            minTime -= Time.deltaTime;
            main.startColor = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, 1f - minTime);
            yield return null;
        }

        _spinParticles.Stop();
    }
}