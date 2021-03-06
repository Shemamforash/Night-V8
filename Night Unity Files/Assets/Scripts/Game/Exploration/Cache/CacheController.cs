﻿using System.Collections;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class CacheController : MonoBehaviour
{
    private readonly List<CacheButtonController> _orderedButtons = new List<CacheButtonController>();
    private readonly List<CacheButtonController> _scrambledButtons = new List<CacheButtonController>();
    private readonly List<CacheRingController> _cacheRings = new List<CacheRingController>();
    private CacheButtonController _nextButton;
    private static CacheController _instance;
    private static GameObject _prefab;
    private CacheButtonController _lastButton;
    private UiSkillUpgradeEffectController _successEffect;
    private bool _active = true;
    private bool _started;

    private void Awake()
    {
        _instance = this;
        _successEffect = gameObject.FindChildWithName<UiSkillUpgradeEffectController>("Upgrade");
        GameObject ringParent = gameObject.FindChildWithName("Cache Rings");
        for (int i = 1; i < 6; ++i)
        {
            AddButton("Anchor " + i);
            _cacheRings.Add(ringParent.FindChildWithName<CacheRingController>("Ring " + i));
        }

        Scramble();
    }

    private void Scramble()
    {
        _scrambledButtons.AddRange(_orderedButtons);
        _scrambledButtons.Shuffle();
        _nextButton = _scrambledButtons[0];
    }

    public void Start()
    {
        WorldGrid.FinaliseGrid();
        if (CharacterManager.CurrentRegion().IsWeaponHere) return;
        Deactivate();
        Destroy(this);
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    private void AddButton(string anchorName)
    {
        GameObject anchor = gameObject.FindChildWithName(anchorName);
        CacheButtonController button = anchor.FindChildWithName<CacheButtonController>("Button");
        _orderedButtons.Add(button);
    }

    public static void Generate()
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Buildings/Cache");
        Instantiate(_prefab, Vector3.zero, Quaternion.identity);
    }

    public static CacheController Instance()
    {
        return _instance;
    }

    public void TryActivateButton(CacheButtonController cacheButtonController)
    {
        if (cacheButtonController == _lastButton) return;
        if (cacheButtonController == _nextButton) ActivateButton();
        else ResetButtons();
    }

    private void Succeed()
    {
        _scrambledButtons.ForEach(b =>
        {
            b.DisableButton();
            b.SetGateActive(true);
        });
        _cacheRings.ForEach(r => r.Activate());
        StartCoroutine(SpawnEnemies());
        _started = true;
    }

    private void Deactivate()
    {
        _active = false;
        _cacheRings.ForEach(r => r.Deactivate());
        _orderedButtons.ForEach(b =>
        {
            b.DisableButton();
            b.SetGateActive(false);
        });
    }

    private IEnumerator SpawnEnemies()
    {
        int enemyCount = WorldState.ScaleValue(10);
        int currentButton = 0;
        List<EnemyType> allowedEnemies = WorldState.GetAllowedHumanEnemyTypes();
        int maxEnemies = WorldState.ScaleValue(3);
        while (enemyCount > 0)
        {
            if (CombatManager.Instance().Enemies().Count < maxEnemies)
            {
                EnemyType enemyType = allowedEnemies.RandomElement();
                _orderedButtons[currentButton].SpawnInEnemy(enemyType);
                --enemyCount;
                ++currentButton;
                if (currentButton == _orderedButtons.Count) currentButton = 0;
                yield return new WaitForSeconds(3f);
            }

            yield return null;
        }

        while (CombatManager.Instance().Enemies().Count > 0) yield return null;
        Deactivate();
        _successEffect.Activate();
        Loot loot = new Loot(Vector2.zero);
        loot.SetItem(WeaponGenerator.GenerateWeapon(true));
        loot.CreateObject(true);
        CombatLogController.PostLog("An artifact is revealed");
        CharacterManager.CurrentRegion().IsWeaponHere = false;
    }

    private void ResetButtons()
    {
        _lastButton = null;
        _nextButton = _scrambledButtons[0];
        _scrambledButtons.ForEach(b => b.DeactivateButton());
        _cacheRings.ForEach(r => r.SetActive(false));
    }

    private void ActivateButton()
    {
        _lastButton = _nextButton;
        _nextButton.ActivateButton();
        int buttonIndex = _scrambledButtons.IndexOf(_nextButton);
        _cacheRings[buttonIndex].SetActive(true);
        if (buttonIndex + 1 == _scrambledButtons.Count) Succeed();
        else _nextButton = _scrambledButtons[buttonIndex + 1];
    }

    public static bool Active()
    {
        return _instance != null && _instance._active;
    }

    public static bool Started()
    {
        return _instance != null && _instance._started;
    }
}