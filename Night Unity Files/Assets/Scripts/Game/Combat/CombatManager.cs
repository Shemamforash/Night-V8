using System.Security.Cryptography.X509Certificates;
using Characters;
using Facilitating.MenuNavigation;
using Game.Combat.CombatStates;
using Game.World;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat
{
    public class CombatManager : StateMachine
    {
        public static CombatUI CombatUi;
        private static MyFloat _strengthText;
        private static Character _character;
        private float _reloadStartTime, _cockStartTime;
        private readonly MyFloat _aimAmount = new MyFloat(0, 0, 100);

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
        
        public void Awake()
        {
            base.Awake();
            CombatUi = new CombatUI(GameObject.Find("Combat Menu"));
            AddState(new Approaching(this, true));
            AddState(new Aiming(this, true));
            AddState(new Cocking(this, true));
            AddState(new EnteringCover(this, true));
            AddState(new ExitingCover(this, true));
            AddState(new Firing(this, true));
            AddState(new Flanking(this, true));
            AddState(new Reloading(this, true));
            AddState(new Retreating(this, true));
            SetDefaultState("Aiming");
        }
        
        public static void EnterCombat(Character c)
        {
            WorldTime.Pause();
            GameMenuNavigator.MenuNavigator.SwitchToMenu("Combat Menu", true);
            _character = c;
            CombatUi.CharacterName.text = c.Name;
            CombatUi.WeaponNameText.text = c.GetWeapon().GetName();

            CombatUi.ResetMagazine(_character.GetWeapon().Capacity);
            CombatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
        }

        public void ExitCombat()
        {
            WorldTime.UnPause();
            ReturnToDefault();
            GameMenuNavigator.MenuNavigator.SwitchToMenu("Game Menu", true);
        }

        public void TakeDamage(float f)
        {
            _strengthText.Val -= f;
        }
    }
}