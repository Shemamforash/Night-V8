using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class UnarmedBehaviour : EnemyBehaviour
    {
        protected bool Alerted;
        protected float DetectionRange = 3f;
        private const float LoseTargetRange = 6f;
        private Cell _originCell;
        protected float WanderDistance = 3;
        private Cell _targetCell;
        private Cell _lastTargetCell;
        protected float MaxDistance, MinDistance;
        private bool _wandering;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            Wander(true);
        }

        private void Wander(bool resetOrigin)
        {
            Alerted = false;
            if (resetOrigin) _originCell = PathingGrid.WorldToCellPosition(transform.position);
            _targetCell = PathingGrid.GetCellNearMe(_originCell, WanderDistance);
            float waitDuration = Random.Range(1f, 3f);
            CurrentAction = () =>
            {
                if (MoveBehaviour.Moving()) return;
                waitDuration -= Time.deltaTime;
                if (waitDuration > 0) return;
                Wander(false);
            };
        }

        public void Alert(bool alertOthers)
        {
            if (Alerted) return;
            Alerted = true;
            OnAlert();
            if (!alertOthers) return;
            CombatManager.Enemies().ForEach(e =>
            {
                UnarmedBehaviour enemy = e as UnarmedBehaviour;
                if (enemy == this || enemy == null) return;
                float distance = TargetTransform().Distance(enemy.transform);
                if (distance > LoseTargetRange) return;
                enemy.Alert(false);
            });
        }

        public override void TakeShotDamage(Shot shot)
        {
            base.TakeShotDamage(shot);
            Alert(true);
        }

        protected virtual void OnAlert()
        {
            _targetCell = ((CharacterCombat) GetTarget()).CurrentCell();
        }

        private void GoToTargetCell()
        {
            if (_targetCell == _lastTargetCell) return;
            MoveBehaviour.GoToCell(_targetCell, MaxDistance, MinDistance);
            _lastTargetCell = _targetCell;
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
//            DrawLine();
            UpdateDistanceToTarget();
            UpdateTargetCell();
            GoToTargetCell();
        }

        private void DrawLine()
        {
            Vector2 to = TargetPosition();
            Vector2 from = transform.position;
            Vector2 dir = (to - from).normalized;
            to = from + dir * MaxDistance;
            from = from + dir * MinDistance;
            Debug.DrawLine(to, from, Color.red, 0.02f);
        }

        protected virtual void UpdateTargetCell()
        {
            if (!Alerted) return;
            if (GetTarget() == null) SetTarget(PlayerCombat.Instance);
            Cell newTargetCell = ((CharacterCombat) GetTarget()).CurrentCell();
            if (!newTargetCell.Reachable)
            {
                List<Cell> cellsNearMe = PathingGrid.GetCellsNearMe(newTargetCell.Position, 1, 1);
                if (cellsNearMe.Count == 0) return;
                _targetCell = cellsNearMe[0];
                return;
            }

            _targetCell = newTargetCell;
        }

        private void UpdateDistanceToTarget()
        {
            float distance = DistanceToTarget();
            if (Alerted && distance > LoseTargetRange) Wander(true);
            else if (distance < DetectionRange && !Alerted) Alert(true);
        }
    }
}