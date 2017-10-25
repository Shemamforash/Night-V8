using Game.Combat.Enemies;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyValue _strengthText;
        private static CombatScenario _scenario;
        public static CooldownManager CombatCooldowns = new CooldownManager();

        protected void Awake()
        {
            CombatUi = new CombatUi(gameObject);
        }

        public void Update()
        {
            _scenario.Character().CombatStates.Update();
            _scenario.Enemies().ForEach(e => e.CombatStates.Update());
            CombatCooldowns.UpdateCooldowns();
        }

        public static CombatScenario Scenario()
        {
            return _scenario;
        }
        
        public static void EnterCombat(CombatScenario scenario)
        {
            _scenario = scenario;
            Weapon equippedWeapon = _scenario.Character().EquippedGear[GearSubtype.Weapon] as Weapon;
            WorldState.Pause();
            MenuStateMachine.States.NavigateToState("Combat Menu");
            CombatUi.CharacterName.text = _scenario.Character().Name;
            CombatUi.WeaponNameText.text = equippedWeapon.Name;

            CombatUi.ResetMagazine(equippedWeapon.Capacity);
            CombatUi.UpdateMagazine(equippedWeapon.GetRemainingAmmo());
            CombatUi.SetEncounter(scenario);
            scenario.Character().CombatStates.NavigateToState("Aiming");
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