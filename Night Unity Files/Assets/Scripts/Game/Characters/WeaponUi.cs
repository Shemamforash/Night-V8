using Characters;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.ReactiveUI.CustomTypes;

namespace Game.Characters
{
    public class WeaponUi
    {
        private readonly MyFloat _weaponDamage = new MyFloat();
        private readonly MyFloat _weaponFireRate = new MyFloat();
        private readonly MyFloat _weaponReloadSpeed = new MyFloat();
        private readonly MyFloat _weaponCapacity = new MyFloat();
        private readonly MyFloat _weaponHandling = new MyFloat();
        private readonly MyFloat _weaponCriticalChance = new MyFloat();
        private readonly MyFloat _weaponAccuracy = new MyFloat();
        private readonly MyString _weaponName = new MyString();

        public WeaponUi(CharacterUI characterUi)
        {
            _weaponDamage.AddOnValueChange(f => characterUi.WeaponDamageText.text = Helper.Round(f, 2) + " dmg");
            _weaponAccuracy.AddOnValueChange(f => characterUi.WeaponAccuracyText.text = Helper.Round(f, 2) + "acc");
            _weaponFireRate.AddOnValueChange(f => characterUi.WeaponFireRateText.text = Helper.Round(f, 2) + "frt");
            _weaponReloadSpeed.AddOnValueChange(f => characterUi.WeaponReloadSpeedText.text = Helper.Round(f, 2) + "rld");
            _weaponCapacity.AddOnValueChange(f => characterUi.WeaponCapacityText.text = Helper.Round(f, 2) + "cap");
            _weaponCriticalChance.AddOnValueChange(f => characterUi.WeaponCriticalChanceText.text = Helper.Round(f, 2) + "crt");
            _weaponHandling.AddOnValueChange(f => characterUi.WeaponHandlingText.text = Helper.Round(f, 2) + "hnd");
            _weaponName.AddOnValueChange(t =>
            {
                characterUi.WeaponNameTextDetailed.text = t;
                characterUi.WeaponNameTextSimple.text = t;
            });
        }
        
        public void Update(Weapon weapon)
        {
            _weaponName.Text = weapon.Name();
            _weaponDamage.Val = weapon.Damage;
            _weaponAccuracy.Val = weapon.Accuracy;
            _weaponCapacity.Val = weapon.Capacity;
            _weaponCriticalChance.Val = weapon.CriticalChance;
            _weaponFireRate.Val = weapon.FireRate;
            _weaponHandling.Val = weapon.Handling;
            _weaponReloadSpeed.Val = weapon.ReloadSpeed;
        }
    }
}