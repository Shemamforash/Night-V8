using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyValue _strengthText;
        private static CombatScenario _scenario;
        
        protected void Awake()
        {
            CombatUi = new CombatUi(gameObject);
        }
        
        public static void EnterCombat(CombatScenario scenario)
        {
            Weapon equippedWeapon = (Weapon) _scenario.Character.EquippedGear[GearSubtype.Weapon];
            WorldState.Pause();
            MenuStateMachine.States.NavigateToState("Combat Menu");
            _scenario = scenario;
            CombatUi.CharacterName.text = _scenario.Character.Name;
            CombatUi.WeaponNameText.text = equippedWeapon.Name;

            CombatUi.ResetMagazine(equippedWeapon.Capacity);
            CombatUi.UpdateMagazine(equippedWeapon.GetRemainingAmmo());
        }

        public static void ExitCombat()
        {
            WorldState.UnPause();
            _scenario.Resolve();
            MenuStateMachine.States.NavigateToState("Game Menu");
        }

        public static void TakeDamage(float f)
        {
            _strengthText.SetCurrentValue(_strengthText.GetCurrentValue() - f);
        }
    }
}