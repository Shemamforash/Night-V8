﻿using System;
using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Player;
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
        public bool Alerted;
        private bool Fleeing;
        protected const float DetectionRange = 6f;
        private const float WanderDistance = 3;
        private Cell _originCell;
        private Rigidbody2D _rigidBody2d;

        private void Wander(bool resetOrigin)
        {
            Alerted = false;
            if (resetOrigin) _originCell = WorldGrid.WorldToCellPosition(transform.position);
            TargetCell = WorldGrid.GetCellNearMe(_originCell, WanderDistance);
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
        }

        public virtual void Alert()
        {
            Move();
        }

        protected void Move()
        {
            Vector2 dir = (transform.position - PlayerCombat.Position()).normalized;
            float distance = Random.Range(0.5f, 3f);
            List<Cell> possibleCells = WorldGrid.GetCellsInFrontOfMe(CurrentCell(), dir, distance);
            Cell target = possibleCells.Count == 0 ? WorldGrid.GetCellNearMe(CurrentCell(), distance * 1.5f, distance) : possibleCells.RandomElement();
            MoveBehaviour.GoToCell(target);
        }

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            base.TakeDamage(damage, direction);
            Alert();
        }

        protected override void UpdateTargetCell()
        {
        }
    }
}