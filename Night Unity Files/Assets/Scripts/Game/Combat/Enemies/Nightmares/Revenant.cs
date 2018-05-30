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
        private static float _beamTimer;
        private static readonly List<Revenant> _revenants = new List<Revenant>();
        private static readonly List<Revenant> _firedRevenants = new List<Revenant>();
        private static Revenant _nextChainStarter;
        private Vector2 _lastPosition = Vector2.negativeInfinity;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _revenants.Add(this);
            if (_nextChainStarter == null) _nextChainStarter = this;
            _lastPosition = transform.position;
        }

        public override void ChooseNextAction()
        {
            Cell c = PathingGrid.Instance().GetCellOrbitingTarget(CurrentCell(), GetTarget().CurrentCell(), GetComponent<Rigidbody2D>().velocity, 4f, 0.5f);
            if (c != null) Debug.DrawLine(CurrentCell().Position, c.Position);
            Reposition(c);
        }

        public override void Update()
        {
            base.Update();
            if (_spawnProtectionTime > 0) _spawnProtectionTime -= Time.deltaTime;
            UpdateBeamTimer();
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

        private void UpdateBeamTimer()
        {
            if (_firedRevenants.Count > 0) return;
            if (this != _nextChainStarter) return;
            _beamTimer += Time.deltaTime;
            if (_beamTimer < 3) return;
            _beamTimer = 0f;
            StartCoroutine(ChainBeamAttack(ChainPauseDuration));
        }

        private const float ChainPauseDuration = 1f;

        private IEnumerator ChainBeamAttack(float pauseDuration)
        {
            _firedRevenants.Add(this);
            yield return new WaitForSeconds(pauseDuration);
//            _revenants.Sort((a, b) => Vector2.Distance(a.transform.position, transform.position).CompareTo(Vector2.Distance(b.transform.position, transform.position)));
//            _revenants.ForEach(r =>
//            {
//                if (_firedRevenants.Contains(r)) return;
//                if (Vector2.Distance(r.transform.position, transform.position) > 2f) return;
//                r.StartCoroutine(r.ChainBeamAttack(ChainPauseDuration / 8f));
//                BeamController beam = BeamController.Create(false, pauseDuration, 1f);
//                beam.SetFollowTransforms(transform, r.transform);
//                beam.SetBeamWidth(0.25f);
//            });
//            yield return new WaitForSeconds(3);
//            _firedRevenants.Clear();

            Revenant nearest = null;
            float nearestDistance = float.MaxValue;
            _revenants.ForEach(r =>
            {
                if (_firedRevenants.Contains(r)) return;
                float distance = Vector2.Distance(r.transform.position, transform.position);
                if (distance > 2f || distance > nearestDistance) return;
                nearest = r;
                nearestDistance = distance;
            });
            if (nearest == null)
            {
                _firedRevenants.Clear();
            }
            else
            {
                nearest.StartCoroutine(nearest.ChainBeamAttack(ChainPauseDuration / 8f));
                BeamController beam = BeamController.Create(false, pauseDuration, 1f);
                beam.SetFollowTransforms(transform, nearest.transform);
                beam.SetBeamWidth(0.25f);
            }
        }

        private void OnDestroy()
        {
            _revenants.Remove(this);
            _firedRevenants.Remove(this);
            if (this == _nextChainStarter && _revenants.Count > 0)
            {
                _nextChainStarter = Helper.RandomInList(_revenants);
            }
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