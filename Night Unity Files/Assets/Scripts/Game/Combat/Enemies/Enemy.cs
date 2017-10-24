using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        private MyValue _enemyHp;
        public readonly MyValue DistanceToCharacter = new MyValue(0, 0, MaxDistance);
        private MyValue _sightToCharacter;
        private MyValue _exposure;
        
        private const float _movementSpeed = 1;
        private const float ImmediateDistance = 1f, CloseDistance = 10f, MidDistance = 50f, FarDistance = 100f, MaxDistance = 150f;
        private CombatScenario _encounter;

        public Enemy(string name, int enemyHp, CombatScenario encounter) : base(name)
        {
            _enemyHp = new MyValue(enemyHp, 0, enemyHp);
            _enemyHp.OnMin(Kill);
            _encounter = encounter;
            DistanceToCharacter.SetCurrentValue(Random.Range(CloseDistance, FarDistance));
            DistanceToCharacter.AddThreshold(ImmediateDistance, "Immediate");
            DistanceToCharacter.AddThreshold(CloseDistance, "Close");
            DistanceToCharacter.AddThreshold(MidDistance, "Medium");
            DistanceToCharacter.AddThreshold(FarDistance, "Far");
            DistanceToCharacter.AddThreshold(MaxDistance, "Out of Range");
        }

        public override void IncreaseDistance(float speedModifier)
        {
            float distance = _movementSpeed * Time.deltaTime * speedModifier * 5f;
            DistanceToCharacter.SetCurrentValue(DistanceToCharacter.GetCurrentValue() + distance);
            if (distance > MaxDistance)
            {
                Flee();    
            }
        }

        public override void DecreaseDistance(float speedModifier)
        {
            float distance = _movementSpeed * Time.deltaTime * speedModifier * 5f;
            DistanceToCharacter.SetCurrentValue(DistanceToCharacter.GetCurrentValue() - distance);
        }

        private void Flee()
        {
            _encounter.Remove(this);
        }

        public override void TakeDamage(int amount)
        {
            _enemyHp.SetCurrentValue(_enemyHp.GetCurrentValue() - amount);
        }

        public override void Kill()
        {
            _encounter.Remove(this);
        }

        public override AttributeContainer GetAttributes()
        {
            throw new System.NotImplementedException();
        }

        public override ViewParent CreateUi(Transform parent)
        {
            EnemyView enemyView = new EnemyView(this, parent);
            _enemyHp.AddOnValueChange(f => enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString());
            DistanceToCharacter.AddOnValueChange(f => enemyView.DistanceText.text = Helper.Round(f.GetCurrentValue(), 0) + "m " + f.GetThresholdName());
            return enemyView;
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }
    }
}