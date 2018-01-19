using System.Collections.Generic;
using Game.Combat;
using Game.Combat.Enemies;
using SamsHelper;
using UnityEngine;

public class MeleeController : MonoBehaviour
{
	private static UIMeleeController _up, _left, _down, _right;
	private static List<UIMeleeController> _meleeControllers = new List<UIMeleeController>();
	private const float FalloffRatio = 0.9f;
	private const float MaxRingTime = 2f, MaxPressTime = 0.5f;
	private static float _initialRingTime, _initialPressTime;
	private static Enemy _targetEnemy;

	public void Awake()
	{
		_up = Helper.FindChildWithName<UIMeleeController>(gameObject, "Up");
		_left = Helper.FindChildWithName<UIMeleeController>(gameObject, "Left");
		_down = Helper.FindChildWithName<UIMeleeController>(gameObject, "Down");
		_right = Helper.FindChildWithName<UIMeleeController>(gameObject, "Right");
		_meleeControllers.Add(_up);
		_meleeControllers.Add(_left);
		_meleeControllers.Add(_down);
		_meleeControllers.Add(_right);
		Enable(null);
	}

//	public void Start()
//	{
//		StartMelee();
//	}
	
	private static void Enable(UIMeleeController controller)
	{
		_meleeControllers.ForEach(m => m.gameObject.SetActive(m == controller));
	}


	public static void StartMelee(Enemy enemy)
	{
		_targetEnemy = enemy;
		_initialRingTime = MaxRingTime;
		_initialPressTime = MaxPressTime;
		StartRandomController();
	}

	private static void StartRandomController()
	{
		UIMeleeController randomController = _meleeControllers[Random.Range(0, _meleeControllers.Count)];
		Enable(randomController);
		randomController.StartRunning();
	}

	public static void SucceedRound()
	{
		_initialPressTime *= FalloffRatio;
		_initialRingTime *= FalloffRatio;
		_targetEnemy.OnHit(10, false);
		if(_targetEnemy.IsDead) FailRound();
		else StartRandomController();
	}

	public static void FailRound()
	{
		Enable(null);
		CombatManager.LeaveMelee();
		_targetEnemy.Knockback(10);
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
