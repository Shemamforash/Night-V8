using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyValue _strengthText;
        private static CombatScenario _scenario;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        private static readonly List<EnemyPlayerRelation> EnemyPlayerRelations = new List<EnemyPlayerRelation>();
        private static EnemyPlayerRelation _currentTarget;
        
        public static List<EnemyPlayerRelation> GetEnemyPlayerRelations()
        {
            return EnemyPlayerRelations;
        }
        
        private static EnemyPlayerRelation FindRelation(Enemy enemy)
        {
            return EnemyPlayerRelations.FirstOrDefault(e => e.Enemy == enemy);
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
                FindRelation(enemy).Distance.Increment(amount);
            }
            else
            {
                EnemyPlayerRelations.ForEach(e => e.Distance.Increment(amount));
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
            CombatCooldowns.UpdateCooldowns();
            CombatUi.Update();
            EnemyPlayerRelations.ForEach(r =>
            {
                r.UpdateRelation();
                r.Enemy.DecreaseRage();
            });
            _scenario.Player().DecreaseRage();
        }

        public static CombatScenario Scenario()
        {
            return _scenario;
        }

        private static void CreateRelation(Enemy e)
        {
            Debug.Log(e.Name);
            EnemyPlayerRelation relation = new EnemyPlayerRelation(e, _scenario.Player());
            EnemyPlayerRelations.Add(relation);
            e.InitialiseBehaviour(relation);
        }

        public static void EnterCombat(CombatScenario scenario)
        {
            _scenario = scenario;
            EnemyPlayerRelations.Clear();
            WorldState.Pause();
            MenuStateMachine.States.NavigateToState("Combat Menu");
            CombatUi.Start(scenario);
            _scenario.Enemies().ForEach(CreateRelation);
            _currentTarget = EnemyPlayerRelations[0];
            _scenario.Player().Rage.AddOnValueChange(a => RageBarController.SetRageBarFill(a.GetCurrentValue(), _scenario.Player().RageActivated()));
            CombatUi.SetSkillCooldownNames();
        }

        public static void ExitCombat()
        {
            WorldState.UnPause();
            MenuStateMachine.States.NavigateToState("Game Menu");
            _scenario.Player().Rage.ClearOnValueChange();
        }

        public static void TakeDamage(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.GetCurrentValue() - f);
        }

        public static void Flank(Character c)
        {
            //TODO Flank
//            Enemy enemy = c as Enemy;
//            if (enemy != null)
//            {
//                FindRelation(enemy).PlayerCover.Increment(-1f);
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
                FindRelation(enemy).Enemy.EnemyView().VisionText.text = "No Cover (Enemy)";
            }
            else
            {
                EnemyPlayerRelations.ForEach(relation => { relation.Enemy.EnemyView().CoverText.text = "No Cover (Player)"; });
            }
        }

        public static void TakeCover(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).Enemy.EnemyView().VisionText.text = "Full Cover (Enemy)";
            }
            else
            {
                EnemyPlayerRelations.ForEach(relation => { relation.Enemy.EnemyView().CoverText.text = "Full Cover (Player)"; });
            }
        }

        public static void FireWeapon(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                EnemyPlayerRelation relation = FindRelation(enemy);
                FindRelation(enemy).Player.TakeDamage(enemy.Weapon().Fire(relation.Distance.GetCurrentValue(), false));
            }
            else
            {
                int damage = c.Weapon().Fire(_currentTarget.Distance.GetCurrentValue(), c.RageActivated());
                _currentTarget.Enemy.EnemyBehaviour.TakeFire();
                if (damage == 0) return;
                c.IncreaseRage();
                _currentTarget.Enemy.TakeDamage(damage);
            }
        }

        public static void SetCurrentTarget(MyGameObject enemy)
        {
            _currentTarget.Enemy.EnemyView().MarkUnselected();
            _currentTarget = FindRelation((Enemy) enemy);
        }

        public static void Flee(Enemy enemy)
        {
            FindRelation(enemy).MarkFled();
            CombatUi.Remove(enemy);
        }

        public static void DashForward(Character character)
        {
            Dash(character, Direction.Right);
        }

        private static void Dash(Character c, Direction direction)
        {
            float dashAmount = 5;
            if (direction == Direction.Right)
            {
                dashAmount = -dashAmount;
            }
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).Distance.Increment(dashAmount);
            }
            else
            {
                EnemyPlayerRelations.ForEach(relation => relation.Distance.Increment(dashAmount));
            }
        }

        public static void DashBackward(Character character)
        {
            Dash(character, Direction.Left);
        }

        public static EnemyPlayerRelation GetCurrentTarget()
        {
            return _currentTarget;
        }
    }
}