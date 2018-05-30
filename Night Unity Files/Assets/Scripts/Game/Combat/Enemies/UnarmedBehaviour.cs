using System.Threading;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies
{
    public class UnarmedBehaviour : EnemyBehaviour
    {
        protected bool Alerted;
        private readonly CharacterAttribute _detectionRange = new CharacterAttribute(AttributeType.Detection, 2f);
        private readonly CharacterAttribute _visionRange = new CharacterAttribute(AttributeType.Vision, 5f);
        private Vector2 _originPosition;


        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _originPosition = transform.position;
            if (Random.Range(0, 3) == 1) SetActionText("Resting");
            else CurrentAction = Wander;
        }

        public void Alert(bool alertAll = true)
        {
            if (Alerted) return;
            Alerted = true;
            OnAlert();
            if (!alertAll) return;
            CombatManager.Enemies().ForEach(e =>
            {
                if (e == this) return;
                UnarmedBehaviour enemy = e as UnarmedBehaviour;
                if (enemy == null) return;
                enemy.Alert(false);
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

        private void Wander()
        {
            float randomDistance = Random.Range(0.5f, 1.5f);
            float currentAngle = AdvancedMaths.AngleFromUp(_originPosition, transform.position);
            float randomAngle = currentAngle + Random.Range(20f, 60f);
            randomAngle *= Mathf.Deg2Rad;
            Vector2 randomPoint = new Vector2();
            randomPoint.x = randomDistance * Mathf.Cos(randomAngle) + _originPosition.x;
            randomPoint.y = randomDistance * Mathf.Sin(randomAngle) + _originPosition.y;

            Cell targetCell = _grid.PositionToCell(_originPosition);
            targetCell = PathingGrid.Instance().GetCellNearMe(targetCell, 3);
            Thread routingThread = _grid.RouteToCell(CurrentCell(), targetCell, route);
            WaitForRoute(routingThread, WaitThenWander);
            SetActionText("Wandering");
        }

        private void Flee()
        {
            SetActionText("Fleeing");
            CurrentAction = () =>
            {
                if (DistanceToTarget() > CombatManager.VisibilityRange()) Kill();
            };
        }

        private void CheckForPlayer()
        {
            if (DistanceToTarget() > _visionRange) return;
            CurrentAction = Suspicious;
        }

        private void Suspicious()
        {
            SetActionText("Suspicious");
            if (DistanceToTarget() >= _detectionRange.CurrentValue()) return;
            if (DistanceToTarget() >= _visionRange.CurrentValue()) CurrentAction = Wander;
            Alert();
        }
    }
}