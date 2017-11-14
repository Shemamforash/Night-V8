using System.Collections.Generic;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.StateMachines;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyBehaviours
{
    public abstract class EnemyBehaviour : State
    {
        protected readonly Enemy Enemy;
        protected float Duration;
        private readonly List<EnemyBehaviour> _onExitTransitions = new List<EnemyBehaviour>();
        private EnemyBehaviour _onDetectBehaviour;
        public readonly bool IsPassive;
        protected readonly Weapon EnemyWeapon;

        protected EnemyBehaviour(string name, Enemy enemy, bool isPassive = false) : base(name, StateSubtype.EnemyBehaviour)
        {
            Enemy = enemy;
            EnemyWeapon = enemy.Weapon();
            IsPassive = isPassive;
//            relation.Enemy.EnemyBehaviour.AddState(this);
        }

        public void SetOnDetectBehaviour(EnemyBehaviour behaviour)
        {
            _onDetectBehaviour = behaviour;
        }

        protected override void ReturnToDefault()
        {
//            Relation.Enemy.CombatController.ReturnToDefault();
        }

        protected override void NavigateToState(string stateName)
        {
            Enemy.EnemyBehaviour.NavigateToState(stateName);
        }

        protected void SelectRandomTransition()
        {
            NavigateToState(_onExitTransitions[Random.Range(0, _onExitTransitions.Count)].Name);
        }

        public void AddExitTransition(EnemyBehaviour transition)
        {
            if (!_onExitTransitions.Contains(transition))
            {
                _onExitTransitions.Add(transition);
            }
        }

        protected void SetStatusText(string text)
        {
            Enemy.SetActionText(text);
        }

        public virtual void OnDetect()
        {
            if (_onDetectBehaviour == null) return;
            NavigateToState(_onDetectBehaviour.Name);
        }
    }
}