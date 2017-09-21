using Game.World.Time;
using SamsHelper.ReactiveUI.CustomTypes;
using SamsHelper.ReactiveUI.MenuSystem;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyFloat _strengthText;
        private static CombatScenario _scenario;
        
        protected void Awake()
        {
            CombatUi = new CombatUi(gameObject);
        }
        
        public static void EnterCombat(CombatScenario scenario)
        {
            WorldTime.Instance().Pause();
            MenuStateMachine.States.NavigateToState("Combat Menu");
            _scenario = scenario;
            CombatUi.CharacterName.text = _scenario.Character.Name;
            CombatUi.WeaponNameText.text = _scenario.Character.GetWeapon().Name;

            CombatUi.ResetMagazine(_scenario.Character.GetWeapon().Capacity);
            CombatUi.UpdateMagazine(_scenario.Character.GetWeapon().GetRemainingAmmo());
        }

        public static void ExitCombat()
        {
            WorldTime.Instance().UnPause();
            _scenario.Resolve();
            MenuStateMachine.States.NavigateToState("Game Menu");
        }

        public static void TakeDamage(float f)
        {
            _strengthText.Val -= f;
        }
    }
}