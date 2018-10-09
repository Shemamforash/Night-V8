using System.Collections;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses.Starfish
{
    public class StarfishSpreadFire : MonoBehaviour
    {
        private bool _tier1Active;
        private bool _tier2Active;

        private float _spinAttackCooldown, _burstAttackCooldown, _radialAttackTimer, _radialAttackAngle;

        public void UpdateSpreadFire()
        {
            UpdateSpinAttack();
            UpdateBurstAttack();
            UpdateRadialAttack();
        }

        private void UpdateSpinAttack()
        {
            if (_spinAttackCooldown < 0f) return;
            _spinAttackCooldown -= Time.deltaTime;
            if (_spinAttackCooldown > 0f) return;
            StartCoroutine(DoSpinAttack());
        }

        private void UpdateRadialAttack()
        {
            if (!_tier2Active) return;
            _radialAttackTimer -= Time.deltaTime;
            if (_radialAttackTimer > 0f) return;
            _radialAttackTimer = 0.5f;
            int count = 20;
            float angleInterval = 360f / count;
            for (int j = 0; j < count; ++j)
            {
                float angle = j * angleInterval + _radialAttackAngle;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                Vector3 direction = new Vector2(x, y);
                MaelstromShotBehaviour.Create(direction, transform.position + direction, 1.5f, false);
            }

            _radialAttackAngle += 5f;
        }

        private void UpdateBurstAttack()
        {
            if (!_tier1Active) return;
            if (_burstAttackCooldown < 0f) return;
            _burstAttackCooldown -= Time.deltaTime;
            if (_burstAttackCooldown > 0f) return;
            StartCoroutine(DoBurstAttack());
        }

        private IEnumerator DoBurstAttack()
        {
            int count = 50;
            float angleInterval = 360f / count;
            float startAngle = 0f;

            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < count; ++j)
                {
                    float angle = j * angleInterval + startAngle;
                    float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                    float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                    Vector3 direction = new Vector2(x, y);
                    MaelstromShotBehaviour.Create(direction, transform.position + direction, 1.5f, false);
                }

                startAngle = startAngle == 0 ? angleInterval / 2f : 0;
                yield return new WaitForSeconds(0.25f);
            }

            _burstAttackCooldown = Random.Range(5f, 10f);
        }

        private IEnumerator DoSpinAttack()
        {
            int count = 50;
            float angleInterval = 180f / count;
            while (count > 0f)
            {
                float angle = angleInterval * count;
                float angleB = angle + 180;
                --count;
                float x = Mathf.Cos(angle * Mathf.Deg2Rad);
                float y = Mathf.Sin(angle * Mathf.Deg2Rad);
                Vector3 dirA = new Vector2(x, y);
                x = Mathf.Cos(angleB * Mathf.Deg2Rad);
                y = Mathf.Sin(angleB * Mathf.Deg2Rad);
                Vector3 dirB = new Vector2(x, y);
                MaelstromShotBehaviour.Create(dirA, transform.position + dirA, 1.5f, false);
                MaelstromShotBehaviour.Create(dirB, transform.position + dirB, 1.5f, false);
                yield return new WaitForSeconds(0.15f);
            }

            _spinAttackCooldown = Random.Range(5f, 7.5f);
        }

        public void StartTier1()
        {
            _tier1Active = true;
        }

        public void StartTier2()
        {
            _tier2Active = true;
        }
    }
}