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
        private bool _hasFled, _isDead;
        public readonly CharacterAttribute VisionRange = new CharacterAttribute(AttributeType.Vision, 30f);
        public readonly CharacterAttribute DetectionRange = new CharacterAttribute(AttributeType.Detection, 15f);
        protected readonly ValueTextLink<string> ActionTextLink = new ValueTextLink<string>();

        private EnemyView _enemyView;
        private bool _alerted, _waitingForHeal;
        public readonly Number Distance = new Number(0, 0, 150);
        public readonly int MaxHealth;
        public int Speed;
        
        private void MakeHealRequest()
        {
            foreach (Enemy enemy in CombatManager.GetEnemies())
            {
                Medic medic = enemy as Medic;
                medic?.RequestHeal(this);
                SetActionText("Waiting for Healing");
                TakeCover();
                _waitingForHeal = true;
            }
        }

        public void ReceiveHealing(int amount)
        {
            HealthController.Heal(amount);
            _waitingForHeal = false;
        }

        protected Enemy(string name, int enemyHp) : base(name)
        {
            MaxHealth = enemyHp;
            if (!(this is Medic))
            {
                HealthController.AddOnTakeDamage(a =>
                {
                    if (HealthController.GetNormalisedHealthValue() <= 0.5f)
                    {
                        MakeHealRequest();
                    }
                });
            }
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
//            CoverCooldown.SetStartAction(() => SetActionText("Taking Cover"));
            SetWanderCooldown();
            CurrentAction = Wander;
            HealthController.AddOnTakeDamage(f =>
            {
                TryAlert();
            });
//            Print();
        }

        protected override Shot FireWeapon(Character target)
        {
            Shot s = base.FireWeapon(target);
            if (s != null) _enemyView.UiAimController.Fire();
            return s;
        }
        

        protected override float GetSpeedModifier()
        {
            return Speed * Time.deltaTime;
        }

//        private void Print()
//        {
//            Debug.Log(Name);
//            Debug.Log("Strength " + Helper.Round(Attributes.Strength.CurrentValue()));
//            Debug.Log("Perception " + Helper.Round(Attributes.Perception.CurrentValue()));
//            Debug.Log("Willpower " + Helper.Round(Attributes.Willpower.CurrentValue()));
//            Debug.Log("Endurance " + Helper.Round(Attributes.Endurance.CurrentValue()));
//        }

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
                _enemyView.SetAlpha(alpha);
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
        }
        
        public override void Kill()
        {
            _enemyView.MarkDead();
            CombatManager.Flee(this);
        }

        public override ViewParent CreateUi(Transform parent)
        {
            _enemyView = new EnemyView(this, parent);
            HealthController.AddOnTakeDamage(f =>
            {
                _enemyView.SetHealth(HealthController.GetNormalisedHealthValue());
//                _enemyView.StrengthText.text = Helper.Round(f.GetCurrentValue(), 0).ToString();
            });
            HealthController.AddOnHeal(f => _enemyView.SetHealth(HealthController.GetNormalisedHealthValue()));
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