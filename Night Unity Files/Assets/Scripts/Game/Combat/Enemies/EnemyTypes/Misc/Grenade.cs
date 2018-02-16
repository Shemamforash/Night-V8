using System;
using System.Collections;
using Facilitating.UIControllers;
using SamsHelper;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class Grenade : MonoBehaviour
    {
        private int _damage = 20;
        private Action _moveAction;
        private float _speed = 25;
        private TextMeshProUGUI _distanceText;
        public float CurrentPosition;
        private float _targetPosition;

        public virtual void Awake()
        {
            _distanceText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Distance");
            SetName("Grenade");
            StartCoroutine(MoveToPosition());
        }

        protected void SetName(string grenadeName)
        {
            Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name").text = grenadeName;
        }

        private IEnumerator MoveToPosition()
        {
            bool reachedTarget = false;
            while (!reachedTarget)
            {
                if (_targetPosition < CurrentPosition) _speed = -_speed;
                float lastPosition = CurrentPosition;
                CurrentPosition += _speed * Time.deltaTime;
                if (lastPosition > _targetPosition && CurrentPosition <= _targetPosition)
                {
                    CurrentPosition = _targetPosition;
                    reachedTarget = true;
                }
                else if (lastPosition < _targetPosition && CurrentPosition >= _targetPosition)
                {
                    CurrentPosition = _targetPosition;
                    reachedTarget = true;
                }

                _distanceText.text = Helper.Round(CombatManager.DistanceBetween(CurrentPosition, CombatManager.Player)) + "m";
                yield return null;
            }

            StartCoroutine(Detonate());
        }

        public void SetTargetPosition(float currentPosition, float targetPosition)
        {
            CurrentPosition = currentPosition;
            SetTargetPosition(targetPosition);
        }

        public void SetTargetPosition(float targetPosition)
        {
            _targetPosition = targetPosition;
        }

        private IEnumerator Detonate()
        {
            _distanceText.text = "!!!";
            _distanceText.color = Color.red;
            yield return new WaitForSeconds(1);
            DestroyGrenade();
        }

        private void DestroyGrenade()
        {
            CreateExplosion();
            UIGrenadeController.RemoveGrenade(this);
            Destroy(gameObject);
        }

        protected virtual void CreateExplosion()
        {
            Explosion explosion = new Explosion(CurrentPosition, 5, 20);
            explosion.SetKnockbackDistance(5);
            explosion.Fire();
        }
    }
}