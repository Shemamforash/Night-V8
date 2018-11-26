using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class AnimalBehaviour : UnarmedBehaviour
    {
        private bool Alerted;
        private bool Fleeing;
        private Cell _fleeTarget;
        private float DetectionRange = 6f;
        private const float LoseTargetRange = 10f;
        protected float WanderDistance = 3;
        private Cell _originCell;
        
        protected virtual void OnAlert()
        {
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
                AnimalBehaviour enemy = e as AnimalBehaviour;
                if (enemy == this || enemy == null) return;
                float distance = TargetTransform().Distance(enemy.transform);
                if (distance > LoseTargetRange) return;
                enemy.Alert(false);
            });
        }

        private void UpdateDistanceToTarget()
        {
            float distance = DistanceToTarget();
            if (Alerted && distance > LoseTargetRange) Wander(true);
            else if (distance < DetectionRange && !Alerted) Alert(true);
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

        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            DetectionRange = 0f;
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateDistanceToTarget();
            CheckToFade();
        }

        private void CheckToFade()
        {
            if (!Fleeing) return;
            if (!CurrentCell().IsEdgeCell) return;
            StartCoroutine(FleeArea());
        }

        protected void Flee(Cell target)
        {
            _fleeTarget = target;
            MoveBehaviour.GoToCell(_fleeTarget);
            CurrentAction = () => StartCoroutine(FleeArea());
        }

        protected override void UpdateTargetCell()
        {
        }

        private IEnumerator FleeArea()
        {
            Fleeing = true;
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            CombatManager.Remove(this);
            float currentTime = 2f;
            while (currentTime > 0f)
            {
                currentTime -= Time.deltaTime;
                sprite.color = Color.Lerp(Color.white, UiAppearanceController.InvisibleColour, 1 - currentTime / 2f);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}