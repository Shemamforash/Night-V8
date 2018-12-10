using System;
using System.Collections;
using Game.Combat.Generation;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Nightmares
{
    public abstract class AnimalBehaviour : UnarmedBehaviour
    {
        protected bool Alerted;
        private bool Fleeing;
        protected const float DetectionRange = 6f;
        private const float WanderDistance = 3;
        private Cell _originCell;
        private Rigidbody2D _rigidBody2d;

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

        protected virtual void UpdateDistanceToTarget()
        {
            float distance = DistanceToTarget();
            if (Alerted && distance > DetectionRange) Wander(true);
        }

        protected override void UpdateRotation()
        {
            Vector2 velocity = _rigidBody2d.velocity;
            float targetRotation = AdvancedMaths.AngleFromUp(Vector2.zero, velocity);
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetRotation));
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
            _rigidBody2d = GetComponent<Rigidbody2D>();
            Wander(true);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            UpdateDistanceToTarget();
            CheckToFade();
        }

        private void CheckToFade()
        {
            bool fade = CurrentCell().OutOfRange;
            fade |= Fleeing && CurrentCell().IsEdgeCell;
            if (!fade) return;
            StartCoroutine(FadeOut());
        }

        public abstract void Alert(bool alertOthers);

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            base.TakeDamage(damage, direction);
            Alert(true);
        }

        protected override void UpdateTargetCell()
        {
        }

        private IEnumerator FadeOut()
        {
            Fleeing = true;
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            CombatManager.RemoveEnemy(this);
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