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
            bool isCharacter = !(Character is Enemy);
            AddState(new Approaching(this, isCharacter));
            AddState(new Aiming(this, isCharacter));
            AddState(new Cocking(this, isCharacter));
            AddState(new EnteringCover(this, isCharacter));
            AddState(new ExitingCover(this, isCharacter));
            AddState(new Firing(this, isCharacter));
            AddState(new Flanking(this, isCharacter));
            AddState(new Reloading(this, isCharacter));
            AddState(new Retreating(this, isCharacter));
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