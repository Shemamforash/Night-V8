using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyValue _strengthText;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        private static readonly List<Enemy> Enemies = new List<Enemy>();
        private static Enemy _currentTarget;
        private static Player _player;

        public static List<Enemy> GetEnemies()
        {
            return Enemies;
        }

//        public static EnemyPlayerenemy Findenemy(Character character)
//        {
//            if(character is Enemy) return Enemies.FirstOrDefault(e => e == character);
//            return Enemies.FirstOrDefault(e => e == _currentTarget);
//        }

        protected void Awake()
        {
            CombatUi = new CombatUi(gameObject);
        }

        public void Update()
        {
            CombatCooldowns.UpdateCooldowns();
            CombatUi.Update();
            Enemies.ForEach(r =>
            {
                if (r.IsDead()) return;
                r.UpdateBehaviour();
            });
            Player().RageController.Decrease();
        }

        public static void ResetCombat()
        {
            Enemies.Clear();
            _currentTarget = null;
            CombatCooldowns.Clear();
        }

        public static void EnterCombat(CombatScenario scenario)
        {
            WorldState.Pause();
            _player = scenario.Player();
            ResetCombat();
            InputHandler.RegisterInputListener(_player);
            Enemies.AddRange(scenario.Enemies());
            MenuStateMachine.States.NavigateToState("Combat Menu");
            CombatUi.Start(scenario);
        }

        public static void ExitCombat()
        {
//            WorldState.UnPause();
//            MenuStateMachine.States.NavigateToState("Game Menu");
//            _scenario.Player().Rage.ClearOnValueChange();
            InputHandler.UnregisterInputListener(_player);
            CombatTester.RestartCombat();
        }

        public static Player Player()
        {
            return _player;
        }

        public static void SetPlayerHealthText(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.CurrentValue() - f);
        }

        public static void Flank(Character c)
        {
            //TODO Flank
//            Enemy enemy = c as Enemy;
//            if (enemy != null)
//            {
//                Findenemy(enemy).PlayerCover.Increment(-1f);
//            }
//            else
//            {
//                _currentTarget.EnemyCover.Increment(-1f);
//            }
        }

        public static void LeaveCover(Character character)
        {
            Enemy enemy = character as Enemy;
            if (enemy != null)
            {
                enemy.EnemyView().VisionText.text = "No Cover (Enemy)";
            }
            else
            {
                Enemies.ForEach(e => { e.EnemyView().CoverText.text = "No Cover (Player)"; });
            }
        }

        public static void TakeCover(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                enemy.EnemyView().VisionText.text = "Full Cover (Enemy)";
            }
            else
            {
                Enemies.ForEach(e => { e.EnemyView().CoverText.text = "Full Cover (Player)"; });
            }
        }

        public static float DistanceBetweenCharacter(Character origin, Character target)
        {
            if (origin is Player) return ((Enemy) target).Distance.CurrentValue();
            if (target is Player) return ((Enemy) origin).Distance.CurrentValue();
            return Mathf.Abs(((Enemy)target).Distance.CurrentValue() - ((Enemy)origin).Distance.CurrentValue());
        }

        public static void IncreaseDistance(Character c, float distance)
        {
            if(c is Player) Enemies.ForEach(e => e.Distance.Increment(distance));
            else ((Enemy)c).Distance.Increment(distance);
        }

        public static void ReduceDistance(Character c, float distance)
        {
            if (c is Player)
                Enemies.ForEach(e =>
                {
                    e.Distance.Increment(-distance);
                    //TODO melee
                    //if(e.Distance.ReachedMin()) EnterMelee(e);
                });
            else
            {
                ((Enemy)c).Distance.Increment(-distance);
//                if (((Enemy) c).Distance.ReachedMin()) EnterMelee(c);
            }
        }
        
        public static Character GetTarget(Character c)
        {
            if (c is Player)
            {
                return _currentTarget;
            }
            return _player;
        }

        public static void SetCurrentTarget(Enemy enemy)
        {
            _currentTarget?.EnemyView().MarkUnselected();
            if (enemy == null) return;
            _currentTarget = enemy;
            _currentTarget.EnemyView().MarkSelected();
        }

        public static void Flee(Enemy enemy)
        {
            enemy.MarkFled();
            CombatUi.Remove(enemy);
            if (Enemies.Any(e => e.InCombat()))
            {
                return;
            }
            ExitCombat();
        }

        public static Enemy GetCurrentTarget()
        {
            return _currentTarget;
        }

        public static List<Enemy> GetEnemiesBehindTarget(Character target)
        {
            List<Enemy> enemiesBehindTarget = new List<Enemy>();
            if (target is Player) return enemiesBehindTarget;
            foreach (Enemy enemy in Enemies)
            {
                if (enemy == target) continue;
                if (enemy.Distance > ((Enemy)target).Distance)
                {
                    enemiesBehindTarget.Add(enemy);
                }
            }
            return enemiesBehindTarget;
        }

        public static List<Character> GetCharactersInRange(Character target, int splinterRange)
        {
            List<Character> enemiesInRange = new List<Character>();
            Enemy enemy1 = target as Enemy;
            if (enemy1?.Distance.CurrentValue() <= splinterRange)
            {
                enemiesInRange.Add(_player);
            }
            foreach (Enemy enemy in Enemies)
            {
                if (enemy == target) continue;
                float distanceFromEnemy = DistanceBetweenCharacter(target, enemy);
                if (distanceFromEnemy <= splinterRange)
                {
                    enemiesInRange.Add(enemy);
                }
            }
            return enemiesInRange;
        }
    }
}