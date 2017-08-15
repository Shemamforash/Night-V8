using Characters;
using Facilitating.MenuNavigation;
using Game.Misc;
using Menus;
using UnityEngine;
using World;

namespace Game.Combat
{
    public class CombatManager : MonoBehaviour
    {
        private CombatUI _combatUi;
        private MyFloat _strengthText;
        private CombatState _currentCombatState = CombatState.Aiming;
        private Character _character;
        private readonly InputListener _inputListener = new InputListener();
        private float _reloadStartTime;

        private enum CombatState
        {
            Approaching,
            Retreating,
            EnteringCover,
            ExitingCover,
            Firing,
            Aiming,
            Reloading,
        }

        public void Awake()
        {
            _combatUi = new CombatUI(GameObject.Find("Combat Menu"));
            _inputListener.OnAxis(InputAxis.Fire, FireWeapon);
            _inputListener.OnAxis(InputAxis.Reload, ReloadWeapon);
        }

        private void FireWeapon()
        {
            if (_currentCombatState == CombatState.Aiming)
            {
                _character.GetWeapon().Fire();
                _combatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
            }
        }

        public void EnterCombat(Character c)
        {
            WorldTime.Pause();
            _character = c;
            _combatUi.CharacterName.text = c.Name;
            _combatUi.WeaponNameText.text = c.GetWeapon().GetName();

            TextAssociation strengthAssociation =
                new TextAssociation(_combatUi.CharacterHealthText, f => f + "/" + c.Strength.Max() + " str", true);
            strengthAssociation.SetPrecision(0);
            _strengthText = new MyFloat(c.Strength.Value, strengthAssociation, 0f, c.Strength.Max());

            _combatUi.ResetMagazine(_character.GetWeapon().Capacity);
            _combatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
        }

        public void ExitCombat()
        {
            WorldTime.UnPause();
            _currentCombatState = CombatState.Aiming;
        }

        public void TakeDamage(float f)
        {
            _strengthText.Value -= f;
        }

        private void ReloadWeapon()
        {
            _reloadStartTime = Time.time;
            _currentCombatState = CombatState.Reloading;
        }

        public void Update()
        {
            if (_currentCombatState == CombatState.Reloading)
            {
                float timeSinceReloadStarted = Time.time - _reloadStartTime;
                if (timeSinceReloadStarted >= _character.GetWeapon().ReloadSpeed)
                {
                    _currentCombatState = CombatState.Aiming;
                    _combatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
                }
            }
        }
    }
}