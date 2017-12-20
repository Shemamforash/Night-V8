using Game.Characters;
using Game.Combat.Enemies.EnemyTypes;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public partial class Enemy : Character
    {
        private const float ImmediateDistance = 1f, CloseDistance = 10f, MidDistance = 50f, FarDistance = 100f, MaxDistance = 150f;
        private MyValue _sightToCharacter;
        private MyValue _exposure;
        private bool _hasFled, _isDead;
        public readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        protected readonly ValueTextLink<string> ActionTextLink = new ValueTextLink<string>();

        private EnemyView _enemyView;
        private bool _alerted;
        public readonly MyValue Distance = new MyValue(0, 0, 150);

        private void MakeHealRequest()
        {
            foreach (Enemy enemy in CombatManager.GetEnemies())
            {
                Medic medic = enemy as Medic;
                medic?.RequestHeal(this);
            }
        }

        protected Enemy(string name, int enemyHp) : base(name)
        {
            BaseAttributes.Strength.Max = enemyHp;
            BaseAttributes.Strength.SetCurrentValue(enemyHp);
            if (!(this is Medic))
            {
                BaseAttributes.Strength.AddOnValueChange(a =>
                {
                    if (a.CurrentValue() <= a.Max / 2)
                    {
                        MakeHealRequest();
                    }
                });
            }
            BaseAttributes.Strength.OnMin(Kill);
            CharacterInventory.SetEnemyResources();
            
            _fireCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            _fireCooldown.SetStartAction(() => SetActionText("Firing"));
            _fireCooldown.SetDuringAction(f => TryFire());
            
            _aimCooldown = new Cooldown(CombatManager.CombatCooldowns, _aimTime);
            _aimCooldown.SetStartAction(() => SetActionText("Aiming"));
            _aimCooldown.SetDuringAction(f =>
            {
                float normalisedTime = Helper.Normalise(f, _aimTime);
                _enemyView.UiAimController.SetValue(1 - normalisedTime);
            });
            _aimCooldown.SetEndAction(() =>
            {
                _fireCooldown.Duration = Random.Range(1, 3);
                _fireCooldown.Start();
                _enemyView.UiAimController.SetValue(0);
            });

            ReloadingCooldown.SetStartAction(() => SetActionText("Reloading"));
            CoverCooldown.SetStartAction(() => SetActionText("Taking Cover"));
            SetWanderCooldown();
            CurrentAction = Wander;

            HealthController.AddOnTakeDamage(f =>
            {
                _enemyView.SpawnDamageText(f, false);
                TryAlert();
            });
//            Print();
        }

        public override Shot FireWeapon(Character target)
        {
            Shot s = base.FireWeapon(target);
            if (s != null) _enemyView.UiAimController.Fire();
            return s;
        }
        

        protected override float GetSpeedModifier()
        {
            return BaseAttributes.Endurance.CurrentValue() * Time.deltaTime;
        }

        private void Print()
        {
            Debug.Log(Name);
            Debug.Log("Strength " + Helper.Round(BaseAttributes.Strength.CurrentValue()));
            Debug.Log("Intelligence " + Helper.Round(BaseAttributes.Intelligence.CurrentValue()));
            Debug.Log("Stability " + Helper.Round(BaseAttributes.Stability.CurrentValue()));
            Debug.Log("Endurance " + Helper.Round(BaseAttributes.Endurance.CurrentValue()));
        }

        private void SetDistanceData()
        {
            Distance.SetCurrentValue(Random.Range(25, 50));
            Distance.AddThreshold(ImmediateDistance, "Immediate");
            Distance.AddThreshold(CloseDistance, "Close");
            Distance.AddThreshold(MidDistance, "Medium");
            Distance.AddThreshold(FarDistance, "Far");
            Distance.AddThreshold(MaxDistance, "Out of Range");
            Distance.AddOnValueChange(a =>
            {
                if (_hasFled || _isDead) return;
                float distance = Helper.Round(Distance.CurrentValue());
                string distanceText = distance.ToString() + "m";
                _enemyView.DistanceText.text = distanceText;
                float normalisedDistance = Helper.Normalise(distance, MaxDistance);
                float alpha = 1f - normalisedDistance;
                alpha *= alpha;
                alpha = Mathf.Clamp(alpha, 0.2f, 1f);
                _enemyView.SetColour(new Color(1, 1, 1, alpha));
                if (a.CurrentValue() <= MaxDistance) return;
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
//            VisionRange.AddModifier(amount);
//            DetectionRange.AddModifier(amount);
        }

        public void RemoveVisionModifier(float amount)
        {
//            VisionRange.RemoveModifier(amount);
//            DetectionRange.RemoveModifier(amount);
        }

        public void TryAlert()
        {
            if (_alerted) return;
            Alert();
        }

        public void SetActionText(string action)
        {
            ActionTextLink.Value(action);
        }

        private void UpdateDetection()
        {
            if (Distance < DetectionRange && !_alerted)
            {
                _enemyView.SetDetected();
                TryAlert();
                return;
            }
            if (Distance < VisionRange)
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

        public override void OnMiss()
        {
//            EnemyBehaviour.TakeFire();
            _enemyView.SpawnDamageText(0, false);
        }
        
        public override void Kill()
        {
            _enemyView.MarkDead();
            CombatManager.Flee(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            BaseAttributes.Strength.AddOnValueChange(f =>
            {
                float normalisedHealth = f.CurrentValue() / f.Max;
                _enemyView.SetHealth(normalisedHealth);
//                _enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString();
            });
            ActionTextLink.AddTextObject(_enemyView.ActionText);
            SetDistanceData();
            ArmourLevel.AddOnValueChange(a => _enemyView.SetArmour((int) ArmourLevel.CurrentValue()));
            return _enemyView;
        }

        public string EnemyType()
        {
            return "Default Enemy";
        }

        public bool IsDead() => _isDead;

        public bool HasFled() => _hasFled;
    }
}