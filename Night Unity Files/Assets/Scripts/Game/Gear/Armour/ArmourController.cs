using System;
using System.Xml;
using Facilitating.UIControllers;
using Game.Characters;
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

        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            return doc;
        }

        private event Action _onArmourChange;

        public void TakeDamage(float amount)
        {
            DivideDamageOrHeal(amount);
            GetUiArmourController()?.TakeDamage(this);
        }

        private UIArmourController GetUiArmourController()
        {
            return _character is Player ? PlayerUi.Instance().GetArmourController(_character) : EnemyUi.Instance().GetArmourController(_character);
        }

        private bool TakePlateDamage(ArmourPlate plate, float damage)
        {
            if (plate == null) return false;
            float armour = plate.GetCurrentProtection();
            float totalHealth = GetCurrentArmour();
            float proportion = armour / totalHealth;
            plate.TakeDamage(proportion * damage);
            return plate.GetCurrentProtection() == 0;
        }
        
        private void DivideDamageOrHeal(float amount)
        {
            if (TakePlateDamage(_plateOne, amount))
                _plateOne = null;

            if (TakePlateDamage(_plateTwo, amount))
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

        public void AutoFillSlots()
        {
            SetPlateOne(ArmourPlate.Create("Living Metal Plate"));
//            SetPlateTwo(ArmourPlate.Create("Living Metal Plate"));
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