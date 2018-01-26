using System.Collections.Generic;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
    private static UIMeleeController _up, _left, _down, _right;
    private static readonly List<UIMeleeController> MeleeControllers = new List<UIMeleeController>();
    private const float FalloffRatio = 0.95f;
    private const float MaxRingTime = 0.6f, MaxPressTime = 0.3f;
    private static float _initialRingTime, _initialPressTime;
    private static Enemy _targetEnemy;
    public static bool InMelee;
    private static int _remainingHits, _hitsWon, _hitsLost;

    public void Awake()
    {
        _up = Helper.FindChildWithName<UIMeleeController>(gameObject, "Up");
        _left = Helper.FindChildWithName<UIMeleeController>(gameObject, "Left");
        _down = Helper.FindChildWithName<UIMeleeController>(gameObject, "Down");
        _right = Helper.FindChildWithName<UIMeleeController>(gameObject, "Right");
        MeleeControllers.Add(_up);
        MeleeControllers.Add(_left);
        MeleeControllers.Add(_down);
        MeleeControllers.Add(_right);
    }

    public static void StartMelee(Enemy enemy)
    {
        _remainingHits = 10;
        _hitsWon = 0;
        _hitsLost = 0;
        _targetEnemy = enemy;
        _initialRingTime = MaxRingTime;
        _initialPressTime = MaxPressTime;
        CombatManager.CombatCanvas.alpha = 0.4f;
        CombatManager.DisengagePlayerInput();
        InMelee = true;
        StartRandomController();
    }

    private static void StartRandomController()
    {
        UIMeleeController randomController = MeleeControllers[Random.Range(0, MeleeControllers.Count)];
        randomController.StartRunning();
    }

    public static void SucceedRound()
    {
        _targetEnemy.OnHit(10, false);
        ++_hitsWon;
        GoToNextRound();
    }

    private static void GoToNextRound()
    {
        _initialPressTime *= FalloffRatio;
        _initialRingTime *= FalloffRatio;
        --_remainingHits;
        if (_remainingHits == 0 || _targetEnemy.IsDead) ExitMelee();
        else StartRandomController();
    }

    public static void FailRound()
    {
        CombatManager.Player().OnHit(10, false);
        ++_hitsLost;
        GoToNextRound();
    }

    private static void ExitMelee()
    {
        CombatManager.CombatCanvas.alpha = 1f;
        CombatManager.EngagePlayerInput();
        InMelee = false;
        if (_targetEnemy.IsDead) return;
        if (_hitsWon >= _hitsLost)
        {
            _targetEnemy.Knockback(10);
        }
        else
        {
            CombatManager.Player().Knockback(10);
        }
    }

    public static float InitialRingTime()
    {
        return _initialRingTime;
    }

    public static float InitialPressTime()
    {
        return _initialPressTime;
    }
}