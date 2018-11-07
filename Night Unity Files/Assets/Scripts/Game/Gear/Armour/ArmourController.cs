using System;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Ui;

namespace Game.Gear.Armour
{
    public class ArmourController
    {
        private readonly Character _character;
        private Armour _chest, _head;
        private Action _onArmourChange;

        public ArmourController(Character character)
        {
            _character = character;
        }

        public void Load(XmlNode doc)
        {
        }

        public XmlNode Save(XmlNode doc)
        {
            _chest?.Save(doc.CreateChild("Chest"));
            _head?.Save(doc.CreateChild("Head"));
            return doc;
        }

        public void TakeDamage(float amount)
        {
            DivideDamage(amount);
        }

        public void Repair(float amount)
        {
            float remaining = amount;
            if(_chest != null) remaining = _chest.Repair(amount);
            if (_head != null) _head.Repair(remaining);
        }

        private void TakePlateDamage(ref Armour plate, float damage)
        {
            if (plate == null) return;
            float plateProtection = plate.GetCurrentProtection();
            float totalProtection = GetCurrentProtection();
            float proportion = plateProtection / totalProtection;
            if (!plate.TakeDamage(proportion * damage)) return;
            plate = null;
        }

        private void DivideDamage(float amount)
        {
            TakePlateDamage(ref _chest, amount);
            TakePlateDamage(ref _head, amount);
        }

        public void SetArmour(Armour armour)
        {
            if (armour.GetArmourType() == Armour.ArmourType.Chest)
                SetChestArmour(armour);
            else
                SetHeadArmour(armour);
        }

        public float CalculateDamageModifier()
        {
            float currentProtection = GetCurrentProtection();
            currentProtection /= 20f;
            return 1 - currentProtection;
        }
        
        private void SetChestArmour(Armour chest)
        {
            _chest?.UnEquip();
            if (_character != null) _chest?.Equip(_character);
            _chest = chest;
            _onArmourChange?.Invoke();
        }

        private void SetHeadArmour(Armour head)
        {
            _head?.UnEquip();
            if (_character != null) head?.Equip(_character);
            _head = head;
            _onArmourChange?.Invoke();
        }

        public int GetCurrentProtection()
        {
            int plateOneCurrent = _head?.GetCurrentProtection() ?? 0;
            int plateTwoCurrent = _chest?.GetCurrentProtection() ?? 0;
            return plateOneCurrent + plateTwoCurrent;
        }

        public void AutoFillSlots(int range)
        {
            int plateOne = range > 5 ? 5 : range;
            int plateTwo = range > 5 ? range - 5 : 0;
            if (plateOne != 0) SetChestArmour(Armour.Create((ItemQuality) plateOne - 1));
            if (plateTwo != 0) SetHeadArmour(Armour.Create((ItemQuality) plateTwo - 1));
        }

        public int GetTotalProtection()
        {
            int chestProtection = _chest?.GetMaxProtection() ?? 0;
            int headProtection = _head?.GetMaxProtection() ?? 0;
            return chestProtection + headProtection;
        }

        public void SetOnArmourChange(Action a)
        {
            _onArmourChange = a;
            _onArmourChange.Invoke();
        }

        public Armour GetChestArmour() => _chest;
        public Armour GetHeadArmour() => _head;
    }
}