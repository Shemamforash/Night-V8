using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Generation.Shrines;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class TempleBehaviour : BasicShrineBehaviour
{
    private ColourPulse ringPulse1, ringPulse2;
    private ParticleSystem _vortex, _explosion, _altar, _flames, _dust;
    private GameObject _cleansedObject;
    private SpriteRenderer _glow;
    private AudioSource _audioSource, _cleansedAudio, _burstAudio;
    private SpriteRenderer _templeSprite;

    private int _bossCount;

    public void Awake()
    {
        Rotate ring1 = gameObject.FindChildWithName<Rotate>("Ring 1");
        Rotate ring2 = gameObject.FindChildWithName<Rotate>("Ring 2");
        ring1.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        ring2.transform.rotation = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        ringPulse1 = ring1.GetComponent<ColourPulse>();
        ringPulse1.SetAlphaMultiplier(0);
        ringPulse2 = ring2.GetComponent<ColourPulse>();
        ringPulse2.SetAlphaMultiplier(0);
        _vortex = gameObject.FindChildWithName<ParticleSystem>("Vortex");
        _explosion = gameObject.FindChildWithName<ParticleSystem>("Explosion");
        _burstAudio = _explosion.GetComponent<AudioSource>();
        _altar = gameObject.FindChildWithName<ParticleSystem>("Altar");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        _flames = gameObject.FindChildWithName<ParticleSystem>("Flames");
        _dust = gameObject.FindChildWithName<ParticleSystem>("Dust");
        _cleansedObject = gameObject.FindChildWithName("Temple Cleansed");
        _templeSprite = gameObject.FindChildWithName<SpriteRenderer>("Temple Image");
        _cleansedObject.SetActive(false);
        _glow.color = UiAppearanceController.InvisibleColour;
        _audioSource = GetComponent<AudioSource>();
        _cleansedAudio = _cleansedObject.GetComponent<AudioSource>();
    }

    protected override void StartShrine()
    {
        if (CharacterManager.CurrentRegion().IsTempleCleansed())
        {
            Succeed();
            return;
        }

        Triggered = true;
        _flames.Play();
        _dust.Play();
        StartCoroutine(FadeInRing(ringPulse1, 3f));
        StartCoroutine(FadeInRing(ringPulse2, 3f));
        StartCoroutine(Activate());
    }

    public void Update()
    {
        if (Triggered) return;
        float distanceToPlayer = PlayerCombat.Position().magnitude;
        float alpha = 0;
        if (distanceToPlayer < 4) alpha = 0.3f;
        if (distanceToPlayer < 8)
        {
            alpha = (distanceToPlayer - 4) / 4f;
            alpha = 1 - alpha;
            alpha *= 0.3f;
        }

        _templeSprite.SetAlpha(alpha);
    }

    private IEnumerator Activate()
    {
        _vortex.Play();
        float vortexTime = _vortex.main.duration + 0.5f;
        _audioSource.Play();
        while (vortexTime > 0f)
        {
            if (!CombatManager.Instance().IsCombatActive()) yield return null;
            vortexTime -= Time.deltaTime;
            yield return null;
        }

        ringPulse1.SetAlphaMultiplier(0);
        ringPulse2.SetAlphaMultiplier(0);
        _altar.Clear();
        _altar.Stop();
        _explosion.Emit(200);
        _burstAudio.Play();
        _templeSprite.SetAlpha(0.75f);
        _templeSprite.DOFade(0.4f, 2f);

        yield return StartCoroutine(StartSpawningEnemies());

        float glowTimeMax = 1f;
        float currentTime = glowTimeMax;
        while (currentTime > 0f)
        {
            if (!CombatManager.Instance().IsCombatActive()) yield return null;
            _glow.color = new Color(1, 1, 1, currentTime / glowTimeMax);
            currentTime -= Time.deltaTime;
            yield return null;
        }

        _glow.color = UiAppearanceController.InvisibleColour;
    }

    private void SpawnInitialEnemies()
    {
        int startEnemies = 10;
        List<Cell> cells = WorldGrid.GetCellsNearMe(transform.position, startEnemies, 5, 2);
        for (int i = 0; i < startEnemies; ++i)
            CombatManager.Instance().SpawnEnemy(EnemyType.Ghoul, cells[i].Position);
    }

    private Queue<EnemyType> GetEnemyTypesToSpawn()
    {
        List<EnemyType> allowedTypes = WorldState.GetAllowedNightmareEnemyTypes();
        int size = (Mathf.FloorToInt(WorldState.Difficulty() / 20f) + 1) * 15;
        List<EnemyType> enemyTypesToSpawn = EnemyTemplate.RandomiseEnemiesToSize(allowedTypes, size);
        Queue<EnemyType> typeQueue = new Queue<EnemyType>();
        enemyTypesToSpawn.ForEach(e => typeQueue.Enqueue(e));
        return typeQueue;
    }

    private IEnumerator StartSpawningEnemies()
    {
        SpawnInitialEnemies();
        yield return new WaitForSeconds(2f);
        Queue<EnemyType> enemyTypesToSpawn = GetEnemyTypesToSpawn();
        int maxEnemyCount = 5 + WorldState.Difficulty() / 5;
        while (enemyTypesToSpawn.NotEmpty())
        {
            EnemyType nextEnemy = enemyTypesToSpawn.Dequeue();
            float nextEnemyArrivalTime = EnemyTemplate.GetEnemyValue(nextEnemy);
            while (nextEnemyArrivalTime > 0f)
            {
                if (CombatManager.Instance().IsCombatActive()) nextEnemyArrivalTime -= Time.deltaTime;
                yield return null;
            }

            while (CombatManager.Instance().Enemies().Count > maxEnemyCount) yield return null;
            Cell cell = WorldGrid.GetCellNearMe(transform.position, 5, 2);
            CombatManager.Instance().SpawnEnemy(nextEnemy, cell.Position);
        }

        StartCoroutine(CheckAllEnemiesDead());
    }

    protected override void Succeed()
    {
        _templeSprite.DOFade(0f, 1f);
        _cleansedObject.SetActive(true);
        _cleansedAudio.Play();
        _explosion.Play();
        _flames.Stop();
        ScreenFaderController.ShowText("Temple Cleansed");
    }

    private IEnumerator CheckAllEnemiesDead()
    {
        while (!CombatManager.Instance().ClearOfEnemies()) yield return null;
        End();
        Succeed();
        WorldState.ActivateTemple();
        CharacterManager.CurrentRegion().SetTempleCleansed();
    }

    private IEnumerator FadeInRing(ColourPulse ring, float duration)
    {
        float maxTime = duration;
        while (duration > 0f)
        {
            if (!CombatManager.Instance().IsCombatActive()) yield return null;
            duration -= Time.deltaTime;
            ring.SetAlphaMultiplier(1f - duration / maxTime);
            yield return null;
        }

        ring.SetAlphaMultiplier(1);
    }
}