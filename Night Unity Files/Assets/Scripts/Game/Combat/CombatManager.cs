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
        private static CombatUI _combatUi;
        private static MyFloat _strengthText;
        private static CombatState _currentCombatState = CombatState.Aiming;
        private static Character _character;
        private readonly InputListener _inputListener = new InputListener();
        private static float _reloadStartTime, _fireStartTime;

        private enum CombatState
        {
            Approaching,
            Retreating,
            EnteringCover,
            ExitingCover,
            Firing,
            Aiming,
            Cocking,
            Reloading,
        }

        public void Awake()
        {
            _combatUi = new CombatUI(GameObject.Find("Combat Menu"));
            _inputListener.OnAxisPress(InputAxis.Fire, Fire);
            _inputListener.OnAxisRelease(InputAxis.Fire, StopFiring);
            _inputListener.OnAxisPress(InputAxis.Reload, ReloadWeapon);
        }

        private void Fire()
        {
            if (_currentCombatState == CombatState.Aiming || _currentCombatState == CombatState.Firing)
            {
                if (_character.GetWeapon().GetRemainingAmmo() == 0)
                {
                    ReloadWeapon();
                }
                else
                {
                    _character.GetWeapon().Fire();
                    _combatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
                    if (_character.GetWeapon().Automatic)
                    {
                        _currentCombatState = CombatState.Firing;
                        _fireStartTime = Time.time;
                    }
                    else
                    {
                        _currentCombatState = CombatState.Cocking;
                    }
                }
            }
        }

        private void StopFiring()
        {
            if (_currentCombatState == CombatState.Firing)
            {
                _currentCombatState = CombatState.Aiming;
            }
            _fireStartTime = 0f;
        }

        private void ContinueFiring()
        {
            float timeSinceShot = Time.time - _fireStartTime;
            if (timeSinceShot >= 1f / _character.GetWeapon().FireRate)
            {
                Fire();
            }
        }

        public static void EnterCombat(Character c)
        {
            WorldTime.Pause();
            GameMenuNavigator.MenuNavigator.SwitchToMenu("Combat Menu", true);
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
            GameMenuNavigator.MenuNavigator.SwitchToMenu("Game Menu", true);
        }

        public void TakeDamage(float f)
        {
            _strengthText.Value -= f;
        }

        private void ReloadWeapon()
        {
            _currentCombatState = CombatState.Reloading;
            _combatUi.EmptyMagazine();
            _reloadStartTime = Time.time;
        }

        public void Update()
        {
            if (_currentCombatState == CombatState.Reloading)
            {
                float timeSinceReloadStarted = Time.time - _reloadStartTime;
                float reloadDuration = _character.GetWeapon().ReloadSpeed;
                _combatUi.UpdateReloadTime(reloadDuration - timeSinceReloadStarted);
                if (timeSinceReloadStarted >= reloadDuration)
                {
                    _currentCombatState = CombatState.Aiming;
                    _character.GetWeapon().Reload();
                    _combatUi.UpdateMagazine(_character.GetWeapon().GetRemainingAmmo());
                }
            }
            else if (_currentCombatState == CombatState.Firing)
            {
                ContinueFiring();
            } else if (_currentCombatState == CombatState.Cocking)
            {
                
            }
            
        }
    }
}