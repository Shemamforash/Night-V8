using Game.Characters;
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
        
        private const float _movementSpeed = 1;
        private CombatScenario _encounter;
        private EnemyView _enemyView;
        
        public Enemy(string name, int enemyHp, CombatScenario encounter) : base(name)
        {
            _enemyHp = new MyValue(enemyHp, 0, enemyHp);
            _enemyHp.OnMin(Kill);
            _encounter = encounter;
        
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
            _encounter.Remove(this);
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

        public string EnemyType()
        {
            return "Default Enemy";
        }
    }
}