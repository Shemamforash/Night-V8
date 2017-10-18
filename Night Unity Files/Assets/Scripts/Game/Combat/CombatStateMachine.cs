using Game.Combat.CombatStates;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat
{
    public class CombatStateMachine : StateMachine
    {
        private float _reloadStartTime, _cockStartTime;
        private readonly MyValue _aimAmount = new MyValue(0, 0, 100);
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
            float amount = 5f + ((Weapon)Character.EquippedGear[GearSubtype.Weapon]).GetAttributeValue(AttributeType.Handling) / 10f;
            amount *= Time.deltaTime;
            _aimAmount.SetCurrentValue(_aimAmount.GetCurrentValue() + amount);
//            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }

        public void DecreaseAim()
        {
            float amount = 100f / ((Weapon)Character.EquippedGear[GearSubtype.Weapon]).Capacity;
            _aimAmount.SetCurrentValue(_aimAmount.GetCurrentValue() - amount);
//            CombatUi.UpdateAimSlider(_aimAmount.Val);
        }
    }
}