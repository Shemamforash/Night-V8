using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Player;
using Game.Global;
using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Gear.Armour
{
    public class ArmourController
    {
        private int _currentLevel;
        private const int MaxLevel = 10;
        private const int ProtectionPerLevel = 25;
        private const float BaseRechargeTime = 5f;
        private float _currentRechargeTime;
        private readonly Number _currentHealth = new Number();
        private bool _justTookDamage;
        private ItemQuality _targetQuality;
        private bool _recharging;

        public ArmourController()
        {
            _currentLevel = 0;
            CalculateMaxHealth();
        }

        public void Load(XmlNode doc)
        {
            _currentLevel = doc.IntFromNode("CurrentLevel");
            CalculateMaxHealth();
        }

        public void Save(XmlNode doc)
        {
            doc.CreateChild("CurrentLevel", _currentLevel);
        }

        private void CalculateMaxHealth()
        {
            float maxHealth = _currentLevel * ProtectionPerLevel;
            _currentHealth.Max = maxHealth;
            _currentHealth.SetCurrentValue(maxHealth);
            _targetQuality = (ItemQuality) (_currentLevel / 2);
        }

        public void TakeDamage(int damage)
        {
            Assert.IsFalse(_currentLevel == 0);
            _currentHealth.Decrement(damage);
            if (!_currentHealth.ReachedMin()) return;
            _recharging = true;
        }

        public void Repair(int amount)
        {
            _currentHealth.Increment(amount);
        }

        private float GetRechargeTime()
        {
            return BaseRechargeTime;
        }

        public bool CanAbsorbDamage()
        {
            return !_recharging && _currentLevel > 0;
        }

        public bool Recharging()
        {
            return _recharging;
        }

        public bool DidJustTakeDamage()
        {
            bool didTakeDamage = _justTookDamage;
            _justTookDamage = false;
            return didTakeDamage;
        }

        private int GetUpgradeQuantity()
        {
            return (_currentLevel + 1) % 2 == 0 ? 2 : 1;
        }


        public bool CanUpgrade()
        {
            if (_currentLevel == 10) return false;
            return Inventory.GetResourceQuantity(Armour.QualityToName(_targetQuality)) >= GetUpgradeQuantity();
        }

        public void Upgrade()
        {
            if (!CanUpgrade()) return;
            Inventory.DecrementResource(Armour.QualityToName(_targetQuality), 1);
            ++_currentLevel;
            CalculateMaxHealth();
        }

        public void AutoGenerateArmour()
        {
            int difficulty = Mathf.FloorToInt(WorldState.Difficulty() / 5f);
            int armourMin = difficulty - 2;
            if (armourMin < 0) armourMin = 0;
            else if (armourMin > 10) armourMin = 10;
            int armourMax = difficulty + 2;
            if (armourMax < 0) armourMax = 0;
            else if (armourMax > 10) armourMax = 10;
            AutoFillSlots(Random.Range(armourMin, armourMax));
        }

        public void AutoFillSlots(int level)
        {
            _currentLevel = level;
            CalculateMaxHealth();
        }

        public float GetTotalProtection()
        {
            return (float) _currentLevel / MaxLevel;
        }

        public void Update()
        {
            if (!_recharging) return;
            _currentRechargeTime += Time.deltaTime;
            float normalisedTime = _currentRechargeTime / GetRechargeTime();
            if (normalisedTime > 1)
            {
                normalisedTime = 1;
                _recharging = false;
                _currentRechargeTime = 0f;
            }

            float newHealth = _currentLevel * ProtectionPerLevel * normalisedTime;
            _currentHealth.SetCurrentValue(newHealth);
        }

        public float GetCurrentFill()
        {
            return _currentHealth.Normalised();
        }

        private readonly string[] _names =
        {
            "No Armour", "Leather Armour", "Leather Armour+", "Makeshift Armour", "Makeshift Armour+", "Metal Armour", "Metal Armour+", "Iridescent Armour",
            "Iridescent Armour+", "Celestial Armour", "Celestial Armour+"
        };

        public string GetName()
        {
            return _names[_currentLevel];
        }

        public string GetBonus()
        {
            if (_currentLevel == 0) return "-";
            return "Absorbs " + _currentHealth.Max + " damage";
        }

        public string GetNextLevelBonus()
        {
            if (_currentLevel == 10) return "Fully Upgraded";
            int nextLevelHealth = (_currentLevel + 1) * ProtectionPerLevel;
            return "Next Level:\nAbsorbs " + nextLevelHealth + " Damage";
        }

        public string GetUpgradeRequirements()
        {
            if (_currentLevel == 10) return "Fully Upgraded";
            int quantity = GetUpgradeQuantity();
            if (!CanUpgrade()) return "Need x" + quantity + " " + Armour.QualityToName(_targetQuality) + " to Upgrade";
            return "Upgrade - x" + quantity + " " + Armour.QualityToName(_targetQuality);
        }

        public void Reset()
        {
            CalculateMaxHealth();
            _recharging = false;
        }

        public int GetCurrentLevel()
        {
            return _currentLevel;
        }
    }
}