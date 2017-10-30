using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.CombatStates;
using Game.Combat.Enemies;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyValue _strengthText;
        private static CombatScenario _scenario;
        public static CooldownManager CombatCooldowns = new CooldownManager();
        private static List<EnemyPlayerRelation> _enemyPlayerRelations = new List<EnemyPlayerRelation>();
        private static EnemyPlayerRelation _currentTarget;
        private const float ImmediateDistance = 1f, CloseDistance = 10f, MidDistance = 50f, FarDistance = 100f, MaxDistance = 150f;
        
        private class EnemyPlayerRelation
        {
            public readonly Enemy Enemy;
            public readonly Player Player;
            public readonly MyValue Distance = new MyValue(0, 0, 150);
            public readonly MyValue EnemyCover = new MyValue(0, 0, 100);
            public readonly MyValue PlayerCover = new MyValue(0, 0, 100);
            private bool _hasFled, _isDead;

            public EnemyPlayerRelation(Enemy enemy, Player player)
            {
                Enemy = enemy;
                Distance.AddOnValueChange(a => enemy.EnemyView().DistanceText.text = Helper.Round(Distance.GetCurrentValue(), 1) + "m (" + a.GetThresholdName() + ")");
                Distance.AddOnValueChange(a =>
                {
                    if (a.GetCurrentValue() <= MaxDistance) return;
                    Scenario().Remove(enemy);
                    _hasFled = true;
                });
                Player = player;
                Distance.SetCurrentValue(Random.Range(CloseDistance, FarDistance));
                Distance.AddThreshold(ImmediateDistance, "Immediate");
                Distance.AddThreshold(CloseDistance, "Close");
                Distance.AddThreshold(MidDistance, "Medium");
                Distance.AddThreshold(FarDistance, "Far");
                Distance.AddThreshold(MaxDistance, "Out of Range");
                EnemyCover.AddOnValueChange(a => enemy.EnemyView().VisionText.text = "Sight: " + Helper.Round(100 - EnemyCover.GetCurrentValue(), 0) + "%");
                PlayerCover.AddOnValueChange(a => enemy.EnemyView().CoverText.text = "Cover: " + Helper.Round(PlayerCover.GetCurrentValue(), 0) + "%");
            }
        }

        private static EnemyPlayerRelation FindRelation(Enemy enemy)
        {
            return _enemyPlayerRelations.FirstOrDefault(e => e.Enemy == enemy);
        }

        public static void IncreaseDistance(Character c, float amount)
        {
            ChangeDistance(c, amount);
        }

        private static void ChangeDistance(Character c, float amount)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                EnemyPlayerRelation relation = FindRelation(enemy);
                if (amount < 0)
                {
                    relation.Distance.Decrement(-amount);
                }
                else
                {
                    relation.Distance.Increment(amount);
                }
            }
            else
            {
                _enemyPlayerRelations.ForEach(e =>
                {
                    if (amount < 0)
                    {
                        e.Distance.Decrement(-amount);
                    }
                    else
                    {
                        e.Distance.Increment(amount);
                    }
                });
            }
        }

        public static void DecreaseDistance(Character c, float amount)
        {
            ChangeDistance(c, -amount);
        }

        protected void Awake()
        {
            CombatUi = new CombatUi(gameObject);
        }

        public void Update()
        {
            _scenario.Player().CombatStates.Update();
            _scenario.Enemies().ForEach(e => e.CombatStates.Update());
            CombatCooldowns.UpdateCooldowns();
            CombatUi.Update();
        }

        public static CombatScenario Scenario()
        {
            return _scenario;
        }

        public static void EnterCombat(CombatScenario scenario)
        {
            _scenario = scenario;
            _enemyPlayerRelations.Clear();
            WorldState.Pause();
            MenuStateMachine.States.NavigateToState("Combat Menu");
            CombatUi.Start(scenario);
            foreach (Enemy e in _scenario.Enemies())
            {
                _enemyPlayerRelations.Add(new EnemyPlayerRelation(e, scenario.Player()));
            }
            _currentTarget = _enemyPlayerRelations[0];
            scenario.Player().CombatStates.NavigateToState("Aiming");
        }

        public static void ExitCombat()
        {
            WorldState.UnPause();
            _scenario.Resolve();
            MenuStateMachine.States.NavigateToState("Game Menu");
        }

        public static void TakeDamage(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.GetCurrentValue() - f);
        }

        public static void Flank(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).PlayerCover.Decrement(1f);
            }
            else
            {
                Debug.Log("here");
                _currentTarget.EnemyCover.Decrement(1f);
            }
        }

        public static void TakeCover(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).EnemyCover.Increment(1f);
            }
            else
            {
                _enemyPlayerRelations.ForEach(relation => relation.PlayerCover.Increment(1f));
            }
        }

        public static void FireWeapon(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                EnemyPlayerRelation relation = FindRelation(enemy);
                FindRelation(enemy).Player.TakeDamage(enemy.Weapon().Fire(relation.Distance.GetCurrentValue()));
            } else 
            {
                _currentTarget.Enemy.TakeDamage(c.Weapon().Fire(_currentTarget.Distance.GetCurrentValue()));
            }
        }

        public static void SetCurrentTarget(MyGameObject enemy)
        {
            _currentTarget = FindRelation((Enemy) enemy);
        }
    }
}