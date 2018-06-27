using System.Threading;
using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class UnarmedBehaviour : EnemyBehaviour
    {
        protected bool Alerted;
        private const float DetectionRange = 2f;
        private const float VisionRange = 5f;
        private Vector2 _originPosition;
        protected bool AlertAll;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _originPosition = transform.position;
            if (Random.Range(0, 3) == 1) SetActionText("Resting");
            else CurrentAction = Wander;
        }

        public void Alert()
        {
            if (Alerted) return;
            Alerted = true;
            OnAlert();
            if (!AlertAll) return;
            CombatManager.Enemies().ForEach(e =>
            {
                if (e == this) return;
                UnarmedBehaviour enemy = e as UnarmedBehaviour;
                if (enemy == null) return;
                enemy.AlertAll = false;
                enemy.Alert();
            });
        }

        public override void TakeDamage(Shot shot)
        {
            base.TakeDamage(shot);
            Alert();
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
            MoveToPlayer();
        }

        public override void Update()
        {
            base.Update();
            CheckForPlayer();
        }

        private void Wander()
        {
            Cell targetCell = PathingGrid.WorldToCellPosition(_originPosition);
            targetCell = PathingGrid.GetCellNearMe(targetCell, 3);
            Thread routingThread = PathingGrid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(routingThread, WaitThenWander);
            SetActionText("Wandering");
        }

        protected virtual void CheckForPlayer()
        {
            if (DistanceToTarget() > VisionRange) return;
            CurrentAction = Suspicious;
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToTarget() >= DetectionRange) return;
            if (DistanceToTarget() >= VisionRange) CurrentAction = Wander;
            Alert();
        }
    }
}