using System.Collections;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GateTransitController : MonoBehaviour
{
    private ParticleSystem _gateParticles;
    private ParticleSystem _streakParticles;
    private SpriteRenderer _glow;
    private static GateTransitController _instance;

    private void Awake()
    {
        _instance = this;
        _gateParticles = GetComponent<ParticleSystem>();
        _streakParticles = gameObject.FindChildWithName<ParticleSystem>("Streaks");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Light");
        _glow.color = UiAppearanceController.InvisibleColour;
    }

    public static void StartTransit()
    {
        _instance.StartCoroutine(_instance.Transit());
    }

    private IEnumerator Transit()
    {
        WorldState.Pause();
        GameObject.Find("Game").SetActive(false);
        StartCoroutine(GoToNextArea());

        _gateParticles.Play();
        _streakParticles.Play();
        float maxTime = _streakParticles.main.duration;
        float currentTime = 0f;
        Color from = new Color(1f, 1f, 1f, 0f);
        Color to = Color.white;
        while (currentTime < maxTime)
        {
            if (WorldState.Paused()) yield return null;
            currentTime += Time.deltaTime;
            float normalisedTime = currentTime / maxTime;
            _glow.color = Color.Lerp(from, to, normalisedTime);
            _glow.transform.localScale = Vector2.Lerp(Vector2.one * 4, Vector2.one * 16, normalisedTime);
            yield return null;
        }

        _glow.color = UiAppearanceController.InvisibleColour;
    }

    private IEnumerator GoToNextArea()
    {
        Image screenFader = GameObject.Find("Screen Fader").GetComponent<Image>();
        screenFader.color = Color.white;
        screenFader.gameObject.GetComponent<CanvasGroup>().alpha = 1;

        Region r = new Region();
        r.SetRegionType(RegionType.Tomb);
        CombatManager.SetCurrentRegion(r);
        AsyncOperation sceneLoaded = SceneManager.LoadSceneAsync("Combat");
        sceneLoaded.allowSceneActivation = false;

        yield return screenFader.DOColor(Color.black, 3f).WaitForCompletion();

        while (sceneLoaded.progress != 0.9f) yield return null;
        sceneLoaded.allowSceneActivation = true;
        ButtonClickListener.SuppressClick();
    }
}