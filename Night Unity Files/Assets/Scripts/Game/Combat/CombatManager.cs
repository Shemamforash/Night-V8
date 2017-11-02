using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.CombatStates;
using Game.Combat.Enemies;
using Game.Gear.Weapons;
using Game.World;
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
        private static readonly MyValue _rage = new MyValue(0, 0, 1);
        private static bool _rageActivated;

        private static void IncreaseRage()
        {
            if (!_rageActivated)
            {
                _rage.Increment(0.05f);
            }
        }

        public static bool RageActivated => _rageActivated;

        public static bool DecreaseRage()
        {
            float decreaseAmount = 0f;
            if (_rage.GetCurrentValue() < 1 && !_rageActivated)
            {
                decreaseAmount = -0.04f;
            }
            else if (_rageActivated)
            {
                decreaseAmount = -0.1f;
            }
            _rage.Increment(decreaseAmount * Time.deltaTime);
            return !_rage.ReachedMin();
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
            _rage.AddOnValueChange(a => RageBarController.SetRageBarFill(a.GetCurrentValue()));
            _rage.OnMin(EndRageMode);
        }

        public void Update()
        {
            _scenario.Player().CombatStates.Update();
            _scenario.Enemies().ForEach(e => e.UpdateBehaviour());
            CombatCooldowns.UpdateCooldowns();
            CombatUi.Update();
            EnemyPlayerRelations.ForEach(r => r.UpdateRelation());
            DecreaseRage();
        }

        public static CombatScenario Scenario()
        {
            return _scenario;
        }

        private static void CreateRelation(Enemy e)
        {
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
            scenario.Player().CombatStates.NavigateToState(nameof(Waiting));
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
                FindRelation(enemy).PlayerCover.Increment(-1f);
            }
            else
            {
                _currentTarget.EnemyCover.Increment(-1f);
            }
        }

        public static void LeaveCover(Character character)
        {
            Enemy enemy = character as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).EnemyCover.Increment(-100f);
            }
            else
            {
                EnemyPlayerRelations.ForEach(relation => relation.PlayerCover.Increment(-100f));
            }
        }


        public static void TakeCover(Character c)
        {
            Enemy enemy = c as Enemy;
            if (enemy != null)
            {
                FindRelation(enemy).EnemyCover.Increment(100f);
            }
            else
            {
                EnemyPlayerRelations.ForEach(relation => relation.PlayerCover.Increment(100f));
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
                int damage = c.Weapon().Fire(_currentTarget.Distance.GetCurrentValue(), _rageActivated);
                if (damage == 0) return;
                IncreaseRage();
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

        private static void EndRageMode()
        {
            if (!_rageActivated) return;
            _rageActivated = false;
            Weapon weapon = _scenario.Player().Weapon();
            weapon.WeaponAttributes.ReloadSpeed.RemoveModifier(-0.5f);
            weapon.WeaponAttributes.FireRate.RemoveModifier(0.5f);
        }

        public static void TryStartRageMode()
        {
            if (_rage.GetCurrentValue() != 1) return;
            _rageActivated = true;
            Weapon weapon = _scenario.Player().Weapon();
            weapon.WeaponAttributes.ReloadSpeed.AddModifier(-0.5f);
            weapon.WeaponAttributes.FireRate.AddModifier(0.5f);
        }
    }
}