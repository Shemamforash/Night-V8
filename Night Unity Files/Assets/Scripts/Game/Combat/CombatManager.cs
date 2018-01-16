using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Characters.Player;
using Game.Combat.Enemies;
using Game.World;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat
{
    public partial class CombatManager : Menu
    {
        private static Number _strengthText;
        public static readonly CooldownManager CombatCooldowns = new CooldownManager();
        private static readonly List<Enemy> Enemies = new List<Enemy>();
        private static readonly List<Grenade> Grenades = new List<Grenade>();
        private static Enemy _currentTarget;
        private static Player _player;
        private static CombatScenario _currentScenario;
        public static List<Enemy> _enemiesToAdd = new List<Enemy>();

        public static List<Enemy> GetEnemies()
        {
            return Enemies;
        }

//        public static EnemyPlayerenemy Findenemy(Character character)
//        {
//            if(character is Enemy) return Enemies.FirstOrDefault(e => e == character);
//            return Enemies.FirstOrDefault(e => e == _currentTarget);
//        }

        public void Update()
        {
            CombatCooldowns.UpdateCooldowns();
            Enemies.ForEach(r =>
            {
                if (r.IsDead) return;
                r.Update();
            });
            Grenades.ForEach(g =>
            {
                g.Update();
            });
            _player.Update();
            _enemiesToAdd.ForEach(AddEnemy);
            _enemiesToAdd.Clear();
            _player.RageController.Decrease();
        }

        public static void RemoveGrenade(Grenade g)
        {
            Grenades.Remove(g);
            _enemyList.Remove(g.EnemyUi);
        }
        
        public static void QueueEnemyToAdd(Enemy e)
        {
            _enemiesToAdd.Add(e);
            e.TryAlert();
            _currentScenario.AddEnemy(e);
        }

        public static void AddGrenade(Grenade g)
        {
            Grenades.Add(g);
            _enemyList.AddItem(g);
        }
        
        public static void ResetCombat()
        {
            Enemies.Clear();
            _currentTarget = null;
            CombatCooldowns.Clear();
        }

        public static void EnterCombat(Player player, CombatScenario scenario)
        {
            WorldState.Pause();
            _currentScenario = scenario;
            _player = player;
            _player.HealthController.EnterCombat();
            ResetCombat();
            ResetMagazine((int) _player.Weapon().WeaponAttributes.Capacity.CurrentValue());
            UpdateMagazine(_player.Weapon().GetRemainingAmmo());
            _playerName.text = _player.Name;
            _enemyList.Clear();
            _weaponNameText.text = _player.Weapon().Name + " (" + _player.Weapon().GetSummary() + ")";
            UpdatePlayerHealth();
            InputHandler.RegisterInputListener(_player);
            scenario.Enemies().ForEach(AddEnemy);
            MenuStateMachine.ShowMenu("Combat Menu");
        }

        public static Player Player()
        {
            return _player;
        }

        private static void AddEnemy(Enemy e)
        {
            Enemies.Add(e);
            e.HealthController.EnterCombat();
            _enemyList.AddItem(e);
            if (_enemyList.Items.Count == 1) SetTarget((Enemy) _enemyList.Items[0].GetLinkedObject());
        }

        private static void ExitCombat()
        {
            WorldState.UnPause();
            MenuStateMachine.ShowMenu("Game Menu");
            InputHandler.UnregisterInputListener(_player);
            if (Enemies.All(e => e.IsDead))
            {
                _currentScenario.FinishCombat();
            }

            _player.HealthController.ExitCombat();
        }

        public static void SetPlayerHealthText(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.CurrentValue() - f);
        }

        public static float DistanceBetweenCharacter(Character origin, Character target)
        {
            if (origin is Player) return ((Enemy) target).Distance.CurrentValue();
            if (target is Player) return ((Enemy) origin).Distance.CurrentValue();
            return Mathf.Abs(((Enemy) target).Distance.CurrentValue() - ((Enemy) origin).Distance.CurrentValue());
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
        }

        public static void Flee(Enemy enemy)
        {
            enemy.HasFled = true;
            CheckCombatEnd();
        }

        public static void CheckCombatEnd()
        {
            if (Enemies.All(e => !e.InCombat()))
            {
                ExitCombat();
            }
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
                if (enemy.Distance > ((Enemy) target).Distance)
                {
                    enemiesBehindTarget.Add(enemy);
                }
            }

            return enemiesBehindTarget;
        }

        public static List<Character> GetCharactersInRange(Character target, int range)
        {
            List<Character> charactersInRange = new List<Character>();
            if ((target as Enemy)?.Distance.CurrentValue() <= range)
            {
                charactersInRange.Add(_player);
            }

            foreach (Enemy enemy in Enemies)
            {
                if (enemy == target) continue;
                float distanceFromEnemy = DistanceBetweenCharacter(target, enemy);
                if (distanceFromEnemy <= range)
                {
                    charactersInRange.Add(enemy);
                }
            }

            return charactersInRange;
        }
    }
}