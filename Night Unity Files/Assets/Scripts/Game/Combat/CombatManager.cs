using Game.Combat.CombatStates;
using Game.World.Time;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.CustomTypes;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Game.Combat
{
    public class CombatManager : Menu
    {
        public static CombatUi CombatUi;
        private static MyFloat _strengthText;
        private static Character _character;
        private float _reloadStartTime, _cockStartTime;
        private readonly MyFloat _aimAmount = new MyFloat(0, 0, 100);
        private CombatStateMachine _machine;
        private static CombatManager _instance;
        
        public static CombatManager Instance()
        {
            return _instance ?? FindObjectOfType<CombatManager>();
        }
        
        public Character Character()
        {
            return _character;
        }

        public void IncreaseAim()
        {
            float amount = 5f + _character.GetWeapon().Handling / 10f;
            amount *= Time.deltaTime;
            _aimAmount.Val = _aimAmount.Val + amount;
            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }

        public void DecreaseAim()
        {
            float amount = 100f / _character.GetWeapon().Capacity;
            _aimAmount.Val = _aimAmount.Val - amount;
            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }

        protected void Awake()
        {
            _instance = this;
            _machine = gameObject.AddComponent<CombatStateMachine>();
            CombatUi = new CombatUi(gameObject);
        }
        
        public static void EnterCombat(Character c)
        {
            WorldTime.Instance().Pause();
            MenuStateMachine.Instance().NavigateToState("Combat Menu");
            _character = c;
            CombatUi.CharacterName.text = c.CharacterName;
            CombatUi.WeaponNameText.text = c.GetWeapon().Name();

            CombatUi.ResetMagazine(_character.GetWeapon().Capacity);
            CombatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
        }

        public void ExitCombat()
        {
            WorldTime.Instance().UnPause();
            _machine.ReturnToDefault();
            MenuStateMachine.Instance().NavigateToState("Game Menu");
        }

        public void TakeDamage(float f)
        {
            _strengthText.Val -= f;
        }
    }
}