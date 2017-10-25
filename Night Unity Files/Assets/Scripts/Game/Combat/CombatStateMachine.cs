using Game.Combat.CombatStates;
using Game.Combat.Enemies;
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
            AddState(new Approaching(this));
            AddState(new Aiming(this));
            AddState(new Cocking(this));
            AddState(new EnteringCover(this));
            AddState(new Firing(this));
            AddState(new Flanking(this));
            AddState(new Reloading(this));
            AddState(new Retreating(this));
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