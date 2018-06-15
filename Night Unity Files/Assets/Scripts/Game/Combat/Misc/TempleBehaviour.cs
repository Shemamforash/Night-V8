﻿using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class TempleBehaviour : MonoBehaviour
{
    private bool _lit, _ring1Active, _ring2Active;
    private ColourPulse ringPulse1, ringPulse2;
    private bool _activated;
    private ParticleSystem _vortex, _explosion, _altar;
    private SpriteRenderer _glow;


    public void Awake()
    {
        Rotate ring1 = Helper.FindChildWithName<Rotate>(gameObject, "Ring 1");
        Rotate ring2 = Helper.FindChildWithName<Rotate>(gameObject, "Ring 2");
        ring1.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        ring2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        ringPulse1 = ring1.GetComponent<ColourPulse>();
        ringPulse1.SetAlphaMultiplier(0);
        ringPulse2 = ring2.GetComponent<ColourPulse>();
        ringPulse2.SetAlphaMultiplier(0);
        _vortex = Helper.FindChildWithName<ParticleSystem>(gameObject, "Vortex");
        _explosion = Helper.FindChildWithName<ParticleSystem>(gameObject, "Explosion");
        _altar = Helper.FindChildWithName<ParticleSystem>(gameObject, "Altar");
        _glow = Helper.FindChildWithName<SpriteRenderer>(gameObject, "Glow");
        _glow.color = UiAppearanceController.InvisibleColour;
    }

    public void Update()
    {
        float distance = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
        if (!_lit && distance < 8) StartLights();
        if (!_ring1Active && distance < 6)
        {
            StartCoroutine(FadeInRing(ringPulse1, 3f));
            _ring1Active = true;
        }

        if (!_ring2Active && distance < 5)
        {
            StartCoroutine(FadeInRing(ringPulse2, 3f));
            _ring2Active = true;
        }

        if (!_activated && distance < 3f)
        {
            StartCoroutine(Activate());
        }
    }

    private IEnumerator Activate()
    {
        _activated = true;
        _vortex.Play();
        float vortexTime = _vortex.main.duration + 0.5f;
        while (vortexTime > 0f)
        {
            vortexTime -= Time.deltaTime;
            yield return null;
        }

        _altar.Clear();
        _altar.Stop();
        _explosion.Emit(200);

        StartCoroutine(StartSpawningEnemies());

        float glowTimeMax = 1f;
        float currentTime = glowTimeMax;
        while (currentTime > 0f)
        {
            _glow.color = new Color(1, 1, 1, currentTime / glowTimeMax);
            currentTime -= Time.deltaTime;
            yield return null;
        }

        _glow.color = UiAppearanceController.InvisibleColour;
    }

    private IEnumerator StartSpawningEnemies()
    {
        int startEnemies = 10;
        List<Cell> cells = PathingGrid.GetCellsNearMe(transform.position, startEnemies, 5, 2);
        for (int i = 0; i < startEnemies; ++i)
        {
            CombatManager.SpawnEnemy(EnemyType.Ghoul, cells[i].Position);
        }

        float nextEnemy = 1f;
        float currentTime = nextEnemy;
        while (nextEnemy > 0.1f)
        {
            while (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                yield return null;
            }

            Cell cell = PathingGrid.GetCellNearMe(transform.position, 5, 2);
            CombatManager.SpawnEnemy(EnemyType.Ghoul, cell.Position);

            nextEnemy *= 0.95f;
            currentTime = nextEnemy;
            yield return null;
        }

        BossShrine.GenerateBoss(transform.position);
    }

    private void StartLights()
    {
        _lit = true;
        StartCoroutine(LightFires(0));
        StartCoroutine(LightFires(90));
        StartCoroutine(LightFires(180));
        StartCoroutine(LightFires(270));
    }

    private IEnumerator FadeInRing(ColourPulse ring, float duration)
    {
        float maxTime = duration;
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            ring.SetAlphaMultiplier(1f - duration / maxTime);
            yield return null;
        }

        ring.SetAlphaMultiplier(1);
    }

    private IEnumerator LightFires(int startAngle)
    {
        float timeToLight = 0.5f;
        for (int i = 0; i < 4; ++i)
        {
            float current = timeToLight;
            while (current > 0f)
            {
                current -= Time.deltaTime;
                yield return null;
            }

            float angleA = startAngle + 10 * (i + 1);
            float angleB = startAngle - 10 * (i + 1);
            Vector2 positionA = AdvancedMaths.CalculatePointOnCircle(angleA, 4f, transform.position);
            Vector2 positionB = AdvancedMaths.CalculatePointOnCircle(angleB, 4f, transform.position);
            FireGenerator.Create(positionA);
            FireGenerator.Create(positionB);
        }
    }
}