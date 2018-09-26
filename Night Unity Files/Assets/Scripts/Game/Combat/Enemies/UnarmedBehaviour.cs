using System;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class UnarmedBehaviour : EnemyBehaviour
    {
        protected bool Alerted;
        private const float DetectionRange = 5f;
        private const float LoseTargetRange = 10f;
        private Cell _originCell;
        protected float WanderDistance = 3;
        protected Cell TargetCell;
        private Cell _lastTargetCell;
        protected float DistanceFromTargetCell, MinDistanceToMove;
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
            TargetCell = PathingGrid.GetCellNearMe(_originCell, WanderDistance);
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
                float distance = GetTarget().transform.Distance(enemy.transform);
                if (distance > LoseTargetRange) return;
                enemy.Alert(false);
            });
        }

        protected bool MoveToCover(Action reachCoverAction)
        {
            bool moving = MoveBehaviour.MoveToCover();
            if (!moving) return false;
            CurrentAction = reachCoverAction;
            return true;
        }

        public override void TakeShotDamage(Shot shot)
        {
            base.TakeShotDamage(shot);
            Alert(true);
        }

        protected virtual void OnAlert()
        {
            TargetCell = GetTarget().CurrentCell();
        }

        private void GoToTargetCell()
        {
            if (TargetCell == _lastTargetCell) return;
            MoveBehaviour.GoToCell(TargetCell, DistanceFromTargetCell);
            _lastTargetCell = TargetCell;
        }


        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateDistanceToTarget();
            UpdateTargetCell();
            GoToTargetCell();
        }

        private void UpdateTargetCell()
        {
            if (!Alerted) return;
            Cell newTargetCell = GetTarget().CurrentCell();
            if (!newTargetCell.Reachable)
            {
                List<Cell> cellsNearMe = PathingGrid.GetCellsNearMe(newTargetCell.Position, 1, 1);
                if (cellsNearMe.Count == 0) return;
                TargetCell = cellsNearMe[0];
                return;
            }

            TargetCell = newTargetCell;
        }

        private void UpdateDistanceToTarget()
        {
            float distance = DistanceToTarget();
            if (Alerted && distance > LoseTargetRange) Wander(true);
            else if (distance < DetectionRange && !Alerted) Alert(true);
        }

        public override void Kill()
        {
            base.Kill();
            CombatManager.IncreaseHumansKilled();
        }
    }
}