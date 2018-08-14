using System;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Combat.Ui;
using SamsHelper.Persistence;

namespace Game.Gear.Armour
{
    public class ArmourController : IPersistenceTemplate
    {
        private readonly Character _character;
        private ArmourPlate _plateOne, _plateTwo;

        public ArmourController(Character character)
        {
            _character = character;
        }

        public void Load(XmlNode doc)
        {
        }

        public XmlNode Save(XmlNode doc)
        {
            _plateOne?.Save(doc.CreateChild("Plate 1"));
            _plateTwo?.Save(doc.CreateChild("Plate 2"));
            return doc;
        }

        private event Action _onArmourChange;

        public void TakeDamage(CharacterCombat character, float amount)
        {
            DivideDamageOrHeal(character, amount);
            GetUiArmourController()?.TakeDamage(this);
        }

        private UIArmourController GetUiArmourController()
        {
            return _character is Player ? PlayerUi.Instance().GetArmourController(_character) : EnemyUi.Instance().GetArmourController(_character);
        }

        private bool TakePlateDamage(CharacterCombat character, ArmourPlate plate, float damage)
        {
            if (plate == null) return false;
            float armour = plate.GetCurrentProtection();
            float totalHealth = GetCurrentArmour();
            float proportion = armour / totalHealth;
            plate.TakeDamage(proportion * damage);
            bool plateDestroyed = plate.GetCurrentProtection() == 0;
            if (plateDestroyed)
            {
                character.WeaponAudio.BreakArmour();
            }
            return plateDestroyed;
        }

        private void DivideDamageOrHeal(CharacterCombat character, float amount)
        {
            if (TakePlateDamage(character, _plateOne, amount))
                _plateOne = null;

            if (TakePlateDamage(character, _plateTwo, amount))
                _plateTwo = null;
        }

        public void SetPlateOne(ArmourPlate plate)
        {
            _plateOne?.Unequip();
            plate?.Equip(_character);
            _plateOne = plate;
            _onArmourChange?.Invoke();
        }

        public int GetCurrentArmour()
        {
            int plateOneCurrent = _plateOne?.GetCurrentProtection() ?? 0;
            int plateTwoCurrent = _plateTwo?.GetCurrentProtection() ?? 0;
            return plateOneCurrent + plateTwoCurrent;
        }

        public int GetMaxArmour()
        {
            int plateOneMax = _plateOne?.GetMaxProtection() ?? 0;
            int plateTwoMax = _plateTwo?.GetMaxProtection() ?? 0;
            return plateOneMax + plateTwoMax;
        }

        public void AddOnArmourChange(Action a)
        {
            _onArmourChange += a;
        }

        public void SetPlateTwo(ArmourPlate plate)
        {
            _plateTwo?.Unequip();
            plate?.Equip(_character);
            _plateTwo = plate;
            _onArmourChange?.Invoke();
        }

        public void AutoFillSlots(int range)
        {
            int plateOne, plateTwo;
            if (range > 5)
            {
                plateOne = 5;
                plateTwo = range - 5;
            }
            else
            {
                plateOne = range;
                plateTwo = 0;
            }

            if (plateOne != 0) SetPlateOne(ArmourPlate.Create((ItemQuality) plateOne - 1));
            if (plateTwo != 0) SetPlateTwo(ArmourPlate.Create((ItemQuality) plateTwo - 1));
        }

        public int GetProtectionLevel()
        {
            //todo plate damage
            int plateOneProtection = _plateOne?.Protection ?? 0;
            int plateTwoProtection = _plateTwo?.Protection ?? 0;
            return plateOneProtection + plateTwoProtection;
        }

        public ArmourPlate GetPlateOne()
        {
            return _plateOne;
        }

        public ArmourPlate GetPlateTwo()
        {
            return _plateTwo;
        }
    }
}