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
            Action<float> takeDamage = null;
            if (_plateOne != null) takeDamage = _plateOne.TakeDamage;
            DivideDamageOrHeal(amount, takeDamage);
            GetUiArmourController()?.TakeDamage(this);
        }

        private UIArmourController GetUiArmourController()
        {
            return _character is Player ? PlayerUi.Instance().GetArmourController(_character) : EnemyUi.Instance().GetArmourController(_character);
        }

        private void DivideDamageOrHeal(float amount, Action<float> armourAction)
        {
            float plateOneHealth = _plateOne?.GetRemainingHealth() ?? 0;
            float plateTwoHealth = _plateTwo?.GetRemainingHealth() ?? 0;
            float totalHealth = GetCurrentArmour();
            float plateOneProportion = plateOneHealth / totalHealth;
            float plateTwoPropotion = plateTwoHealth / totalHealth;
            armourAction?.Invoke(amount * plateOneProportion);
            armourAction?.Invoke(amount * plateTwoPropotion);
        }

        public void Repair(float amount)
        {
            Action<float> repair = null;
            if (_plateOne != null) repair = _plateOne.Repair;
            DivideDamageOrHeal(amount, repair);
            GetUiArmourController()?.RepairArmour(this);
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
            SetPlateTwo(ArmourPlate.Create("Living Metal Plate"));
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