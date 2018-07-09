using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class UnarmedBehaviour : EnemyBehaviour
    {
        protected bool Alerted;
        private const float DetectionRange = 4f;
        private Vector2 _originPosition;
        protected float WanderDistance = 3;
        private Cell _targetLastCell;
        private CharacterCombat _target;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _originPosition = transform.position;
            if (Random.Range(0, 3) == 1) SetActionText("Resting");
            else CurrentAction = Wander;
        }

        public void Alert(bool alertOthers)
        {
            if (Alerted) return;
            Alerted = true;
            OnAlert();
            if (!alertOthers) return;
            CombatManager.Enemies().ForEach(e =>
            {
                if (e == this) return;
                UnarmedBehaviour enemy = e as UnarmedBehaviour;
                if (enemy == null) return;
                if (Vector2.Distance(e.CurrentCell().Position, CurrentCell().Position) > 10) return;
                enemy.Alert(false);
            });
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            Alert(true);
        }

        private void WaitThenWander()
        {
            float waitDuration = Random.Range(1f, 3f);
            CurrentAction = () =>
            {
                waitDuration -= Time.deltaTime;
                if (waitDuration > 0) return;
                Wander();
            };
        }

        protected virtual void OnAlert()
        {
            FollowTarget(PlayerCombat.Instance);
        }

        protected void FollowTarget(CharacterCombat target)
        {
            _target = target;
            if (_target.IsDead) 
            {
                _targetLastCell = null;
                return;
            }

            if (_target.CurrentCell() == _targetLastCell)
            {
                ReachTarget();
                return;
            }

            GoToCell(_target.CurrentCell(), ReachTarget);
            _targetLastCell = _target.CurrentCell();
        }
        
        public override void Update()
        {
            base.Update();
            CheckForPlayer();
        }

        private void Wander()
        {
            Cell targetCell = PathingGrid.WorldToCellPosition(_originPosition);
            targetCell = PathingGrid.GetCellNearMe(targetCell, WanderDistance);
            GoToCell(targetCell, WaitThenWander);
            SetActionText("Wandering");
        }

        protected virtual void CheckForPlayer()
        {
            if (Alerted) return;
            if (DistanceToTarget() > DetectionRange) return;
            Alert(true);
        }

        public override void Kill()
        {
            base.Kill();
            CombatManager.IncreaseHumansKilled();
        }
    }
}