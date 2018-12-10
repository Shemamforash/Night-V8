using System.Collections;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GateTransitController : Menu
{
    private ParticleSystem _streakParticles;
    private SpriteRenderer _glow;
    private static GateTransitController _instance;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        _streakParticles = gameObject.FindChildWithName<ParticleSystem>("Streaks");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Light");
        _glow.color = UiAppearanceController.InvisibleColour;
    }

    public override void Enter()
    {
        base.Enter();
        _instance.StartCoroutine(_instance.Transit());
    }

    public static void StartTransit()
    {
        MenuStateMachine.ShowMenu("Gate Mask");
    }

    private IEnumerator Transit()
    {
        WorldState.Pause();
        _streakParticles.Stop();
        _streakParticles.Play();

        float maxTime = _streakParticles.main.duration;
        float currentTime = 0f;
        Color from = new Color(1f, 1f, 1f, 0f);
        Color to = Color.white;
        while (currentTime < maxTime)
        {
            if (PauseMenuController.IsOpen()) yield return null;
            currentTime += Time.deltaTime;
            float normalisedTime = currentTime / maxTime;
            _glow.color = Color.Lerp(from, to, normalisedTime);
            _glow.transform.localScale = Vector2.Lerp(Vector2.one * 4, Vector2.one * 16, normalisedTime);
            yield return null;
        }

        StartCoroutine(GoToNextArea());
        _glow.color = UiAppearanceController.InvisibleColour;
    }

    private IEnumerator GoToNextArea()
    {
        Region r = new Region();
        r.SetRegionType(RegionType.Tomb);
        CharacterManager.SelectedCharacter = CharacterManager.Wanderer;
        ScreenFaderController.FlashWhite(3f);
        yield return new WaitForSeconds(3f);
        CombatManager.SetCurrentRegion(r);
        SceneChanger.GoToCombatScene();
    }
}