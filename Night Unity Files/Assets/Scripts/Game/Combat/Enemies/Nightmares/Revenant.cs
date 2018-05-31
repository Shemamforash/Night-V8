using System.Collections;
using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Revenant : EnemyBehaviour
    {
        private Vector2 _lastPosition = Vector2.negativeInfinity;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _lastPosition = transform.position;
        }

        public override void ChooseNextAction()
        {
            Cell c = PathingGrid.GetCellOrbitingTarget(CurrentCell(), GetTarget().CurrentCell(), GetComponent<Rigidbody2D>().velocity, 4f, 0.5f);
            if (c != null) Debug.DrawLine(CurrentCell().Position, c.Position);
            Reposition(c);
        }

        public override void Update()
        {
            base.Update();
            if (_spawnProtectionTime > 0) _spawnProtectionTime -= Time.deltaTime;
            Vector2 currentPosition = transform.position;
            if (_lastPosition == currentPosition) return;
            float distance = Vector2.Distance(_lastPosition, currentPosition);
            Vector2 tempPos = _lastPosition;

            float interval = 0.25f;
            for (float i = interval; i < distance; i += interval)
            {
                float lerpVal = i / distance;
                tempPos = Vector2.Lerp(_lastPosition, currentPosition, lerpVal);
                FireBehaviour.Create(tempPos, 0.2f, false);
                    
            }

            _lastPosition = tempPos;
        }

        private float _spawnProtectionTime = 1f;

        public override void TakeDamage(Shot shot)
        {
            if (_spawnProtectionTime > 0) return;
            base.TakeDamage(shot);
        }

        private void Split()
        {
            int newHealth = (int) (HealthController.GetMaxHealth() / 3f);
            if (newHealth < 3) return;
            for (int i = 0; i < 3; ++i)
            {
                Revenant revenant = (Revenant) CombatManager.QueueEnemyToAdd(EnemyType.Revenant);
                Vector2 randomDir = AdvancedMaths.RandomVectorWithinRange(Vector3.zero, 1).normalized;
                revenant.gameObject.transform.position = transform.position;
                revenant._lastPosition = transform.position;
                revenant.GetComponent<Rigidbody2D>().AddForce(randomDir * 200f);
                revenant.SetHealth(newHealth);
            }
        }

        public override void Kill()
        {
            Split();
            base.Kill();
        }

        private void SetHealth(int newHealth)
        {
            HealthController.SetInitialHealth(newHealth, this);
        }
    }
}