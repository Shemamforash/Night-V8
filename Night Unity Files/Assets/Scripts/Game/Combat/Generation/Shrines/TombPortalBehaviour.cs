﻿using System.Collections;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using SamsHelper.Libraries;
using UnityEngine;

public class TombPortalBehaviour : MonoBehaviour
{
    private SpriteRenderer _shadow1, _shadow2, _shadow3, _shadow4;
    private ParticleSystem _ring, _dust, _trails, _twinkle;
    private bool _triggered;
    private const float FadeOutTime = 5f;

    public void Awake()
    {
        _shadow1 = gameObject.FindChildWithName<SpriteRenderer>("Shadow 1");
        _shadow2 = gameObject.FindChildWithName<SpriteRenderer>("Shadow 2");
        _shadow3 = gameObject.FindChildWithName<SpriteRenderer>("Shadow 3");
        _shadow4 = gameObject.FindChildWithName<SpriteRenderer>("Shadow 4");
        _ring = gameObject.FindChildWithName<ParticleSystem>("Ring");
        _dust = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _trails = gameObject.FindChildWithName<ParticleSystem>("Trails");
        _twinkle = gameObject.FindChildWithName<ParticleSystem>("Twinkle");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        ScreenFaderController.FlashWhite(2.5f, new Color(1, 1, 1, 0f));
        switch (EnvironmentManager.CurrentEnvironmentType())
        {
            case EnvironmentType.Desert:
                SerpentBehaviour.Create();
                break;
            case EnvironmentType.Mountains:
                StarfishBehaviour.Create();
                break;
            case EnvironmentType.Sea:
                SwarmBehaviour.Create();
                break;
            case EnvironmentType.Ruins:
                OvaBehaviour.Create();
                break;
        }

        StartCoroutine(FadeAndDie());
        _triggered = true;
    }

    private void SetEmission(ParticleSystem ps, float maxRate, float normalisedVal)
    {
        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = maxRate * normalisedVal;
        float scale = normalisedVal / 2f + 0.5f;
        ps.transform.localScale = Vector3.one * scale;
    }

    private void SetAlpha(SpriteRenderer spriteRenderer, float alpha)
    {
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    private IEnumerator FadeAndDie()
    {
        float ringStartEmission = _ring.emission.rateOverTime.constant;
        float dustStartEmission = _dust.emission.rateOverTime.constant;
        float trailsStartEmission = _trails.emission.rateOverTime.constant;
        float twinkleStartEmission = _twinkle.emission.rateOverTime.constant;

        float currentTimer = FadeOutTime;
        while (currentTimer > 0f)
        {
            if (!CombatManager.IsCombatActive()) yield return null;
            currentTimer -= Time.deltaTime;
            float normalisedVal = currentTimer / FadeOutTime;
            SetEmission(_ring, ringStartEmission, normalisedVal);
            SetEmission(_dust, dustStartEmission, normalisedVal);
            SetEmission(_trails, trailsStartEmission, normalisedVal);
            SetEmission(_twinkle, twinkleStartEmission, normalisedVal);
            SetAlpha(_shadow1, normalisedVal);
            SetAlpha(_shadow2, normalisedVal);
            SetAlpha(_shadow3, normalisedVal);
            SetAlpha(_shadow4, normalisedVal);
            yield return null;
        }

        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}