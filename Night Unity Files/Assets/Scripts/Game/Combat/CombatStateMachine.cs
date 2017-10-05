using Game.Combat.CombatStates;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI.CustomTypes;
using UnityEngine;

namespace Game.Combat
{
    public class CombatStateMachine : StateMachine
    {
        private float _reloadStartTime, _cockStartTime;
        private readonly MyFloat _aimAmount = new MyFloat(0, 0, 100);
        public readonly Character Character;
        
        public CombatStateMachine(Character character)
        {
            Character = character;
            AddState(new Approaching(this, true));
            AddState(new Aiming(this, true));
            AddState(new Cocking(this, true));
            AddState(new EnteringCover(this, true));
            AddState(new ExitingCover(this, true));
            AddState(new Firing(this, true));
            AddState(new Flanking(this, true));
            AddState(new Reloading(this, true));
            AddState(new Retreating(this, true));
        }
        
        public void IncreaseAim()
        {
            float amount = 5f + Character.GetWeapon().AttributeVal(AttributeType.Handling) / 10f;
            amount *= Time.deltaTime;
            _aimAmount.Val = _aimAmount.Val + amount;
//            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }

        public void DecreaseAim()
        {
            float amount = 100f / Character.GetWeapon().AttributeVal(AttributeType.Capacity);
            _aimAmount.Val = _aimAmount.Val - amount;
//            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }
    }
}