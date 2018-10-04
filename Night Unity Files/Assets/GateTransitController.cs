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

    private void Awake()
    {
        _gateParticles = GetComponent<ParticleSystem>();
        _streakParticles = gameObject.FindChildWithName<ParticleSystem>("Streaks");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Light");
        _glow.color = UiAppearanceController.InvisibleColour;
        StartCoroutine(Transit());
    }

    private IEnumerator Transit()
    {
        _gateParticles.Play();
        _streakParticles.Play();
        float maxTime = _streakParticles.main.duration;
        float currentTime = 0f;
        while (currentTime < maxTime)
        {
            if (WorldState.Paused()) yield return null;
            currentTime += Time.deltaTime;
            float normalisedTime = currentTime / maxTime;
            float alpha = Mathf.PerlinNoise(currentTime, 0);
            alpha = (alpha - 0.5f) / 5f + normalisedTime;
            alpha = Mathf.Clamp(alpha, 0f, 1f);
            _glow.color = new Color(1f, 0.5f, 0.7f, alpha);
            yield return null;
        }

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