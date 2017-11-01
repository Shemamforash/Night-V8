using Game.Characters;
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
    public class CombatStateMachine : StateMachine<CombatState>
    {
        private float _reloadStartTime, _cockStartTime;
        public readonly MyValue AimAmount = new MyValue(0, 0, 100);
        public readonly Character Character;
        
        public CombatStateMachine(Character character)
        {
            Character = character;
            AddState(new Approaching(this));
            AddState(new Waiting(this));
            AddState(new Cocking(this));
            AddState(new EnteringCover(this));
            AddState(new Firing(this));
            AddState(new Flanking(this));
            AddState(new Reloading(this));
            AddState(new Retreating(this));
            SetDefaultState(nameof(Waiting));
        }
    }
}