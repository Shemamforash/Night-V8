using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Combat.Enemies.EnemyBehaviours;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        private MyValue _enemyHp;
        private MyValue _sightToCharacter;
        private MyValue _exposure;
        public readonly float VisionRange = 30f, DetectionRange = 20f;

        private const float _movementSpeed = 1;
        private CombatScenario _encounter;
        private EnemyView _enemyView;
        private List<EnemyBehaviour> _behaviours = new List<EnemyBehaviour>();
        private const int EnemyBehaviourTick = 4;
        private int _timeSinceLastBehaviourUpdate = 0;

        public Enemy(string name, int enemyHp, CombatScenario encounter) : base(name)
        {
            _enemyHp = new MyValue(enemyHp, 0, enemyHp);
            _enemyHp.OnMin(Kill);
            _encounter = encounter;
            Equip(WeaponGenerator.GenerateWeapon());
        }

        public EnemyBehaviour GetBehaviour(EnemyBehaviour behaviour)
        {
            return _behaviours.FirstOrDefault(b => b.GetType() == behaviour.GetType());
        }

        public void InitialiseBehaviour(EnemyPlayerRelation relation)
        {
//            _behaviours.Add(new Snipe(relation));
            _behaviours.Add(new Herd(relation));
        }

        public EnemyView EnemyView()
        {
            return _enemyView;
        }

        public override void TakeDamage(int amount)
        {
            _enemyHp.SetCurrentValue(_enemyHp.GetCurrentValue() - amount);
        }

        public override void Kill()
        {
            CombatManager.Flee(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            _enemyHp.AddOnValueChange(f =>
            {
                _enemyView.HealthSlider.value = _enemyHp.GetCurrentValue() / _enemyHp.Max;
                _enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString();
            });
            return _enemyView;
        }

        public void UpdateBehaviour()
        {
            if (_timeSinceLastBehaviourUpdate == 0)
            {
                foreach (EnemyBehaviour behaviour in _behaviours)
                {
                    behaviour.Execute();
                }
            }
            else
            {
                ++_timeSinceLastBehaviourUpdate;
                if (_timeSinceLastBehaviourUpdate == EnemyBehaviourTick)
                {
                    _timeSinceLastBehaviourUpdate = 0;
                }
            }
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }
    }
}