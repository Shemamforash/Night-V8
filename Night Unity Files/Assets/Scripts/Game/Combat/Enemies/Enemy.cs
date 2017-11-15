using Game.Characters;
using Game.Combat.Enemies.EnemyBehaviours;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        private const float ImmediateDistance = 1f, CloseDistance = 10f, MidDistance = 50f, FarDistance = 100f, MaxDistance = 150f;
        public MyValue _enemyHp;
        private MyValue _sightToCharacter;
        private MyValue _exposure;
        private bool _hasFled, _isDead;
        public readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        protected readonly ValueTextLink<string> ActionTextLink = new ValueTextLink<string>();

        private const float _movementSpeed = 1;
        private EnemyView _enemyView;
        public GenericBehaviour EnemyBehaviour;
        private bool _alerted;
        private EnemyBehaviour _currentBehaviour;

        protected Enemy(string name, int enemyHp) : base(name)
        {
            _enemyHp = new MyValue(enemyHp, 0, enemyHp);
            _enemyHp.OnMin(Kill);
            Equip(WeaponGenerator.GenerateWeapon());
            EnemyBehaviour = new GenericBehaviour(this);
            Print();
        }

        private void Print()
        {
            Debug.Log(Name);
            Debug.Log("Strength " + Helper.Round(BaseAttributes.Strength.GetCalculatedValue()));
            Debug.Log("Intelligence " + Helper.Round(BaseAttributes.Intelligence.GetCalculatedValue()));
            Debug.Log("Stability " + Helper.Round(BaseAttributes.Stability.GetCalculatedValue()));
            Debug.Log("Endurance " + Helper.Round(BaseAttributes.Endurance.GetCalculatedValue()));
        }
        
        private void SetDistanceData()
        {
            Position.SetCurrentValue(Random.Range(25, 50));
            Position.AddThreshold(ImmediateDistance, "Immediate");
            Position.AddThreshold(CloseDistance, "Close");
            Position.AddThreshold(MidDistance, "Medium");
            Position.AddThreshold(FarDistance, "Far");
            Position.AddThreshold(MaxDistance, "Out of Range");
            Position.AddOnValueChange(a =>
            {
                if (_hasFled || _isDead) return;
                float distance = -Helper.Round(DistanceToCharacter(CombatManager.Player()));
                string distanceText = distance.ToString() + "m";
                if (distance < 0) distanceText += " (Behind)";
                _enemyView.DistanceText.text = distanceText;
                float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - normalisedDistance;
                alpha *= alpha;
                alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                _enemyView.SetColour(new Color(1, 1, 1, alpha));
                if (a.GetCurrentValue() <= MaxDistance) return;
                _hasFled = true;
            });
        }

        public bool InCombat()
        {
            return !_hasFled && !_isDead;
        }

        public void MarkFled()
        {
            _hasFled = true;
        }

        public bool IsAlerted()
        {
            return _alerted;
        }

        public void AddVisionModifier(float amount)
        {
            VisionRange.AddModifier(amount);
            DetectionRange.AddModifier(amount);
        }

        public void RemoveVisionModifier(float amount)
        {
            VisionRange.RemoveModifier(amount);
            DetectionRange.RemoveModifier(amount);
        }

        public void Alert()
        {
            if (_alerted) return;
            _alerted = true;
//            EnemyBehaviour.StatesAsList().ForEach(b => b.OnDetect());
        }

        public void SetActionText(string action)
        {
            ActionTextLink.Value(action);
        }

        private void UpdateDetection()
        {
            if (Position < DetectionRange || _alerted)
            {
                _enemyView.SetDetected();
                Alert();
                return;
            }
            if (Position < VisionRange)
            {
                _enemyView.SetAlert();
                return;
            }
            _enemyView.SetUnaware();
        }

//        public EnemyBehaviour GetBehaviour(EnemyBehaviour behaviour)
//        {
//            return EnemyBehaviour.StatesAsList().FirstOrDefault(b => b.GetType() == behaviour.GetType());
//        }

        public EnemyView EnemyView()
        {
            return _enemyView;
        }

        private int GetCoverProtection(int damage)
        {
            bool protectedByCover = Random.Range(0, 2) == 0;
            if (protectedByCover)
            {
                if (InCover()) return (int) (damage * 0.5f);
                if (InPartialCover()) return (int) (damage * 0.75);
            }
            return damage;
        }

        public override void TakeDamage(Shot shot, int damage)
        {
            EnemyBehaviour.TakeFire();
            if (damage == 0) return;
            damage = GetCoverProtection(damage);
            Alert();
            _enemyHp.Increment(-damage);
            EnemyBehaviour.TakeDamage(damage);
        }

        public override void Kill()
        {
            _enemyView.MarkDead();
            CombatManager.Flee(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            _enemyHp.AddOnValueChange(f =>
            {
                float normalisedHealth = _enemyHp.GetCurrentValue() / _enemyHp.Max;
                _enemyView.SetHealth(normalisedHealth);
//                _enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString();
            });
            ActionTextLink.AddTextObject(_enemyView.ActionText);
            SetDistanceData();
            ArmourLevel.AddOnValueChange(a => _enemyView.SetArmour((int)ArmourLevel.GetCurrentValue()));
            ArmourLevel.SetCurrentValue(Random.Range(2, 10));
            return _enemyView;
        }

        public void UpdateBehaviour()
        {
            if (!InCombat()) return;
            DecreaseRage();
            UpdateDetection();
            EnemyBehaviour.Update();
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }
    }
}