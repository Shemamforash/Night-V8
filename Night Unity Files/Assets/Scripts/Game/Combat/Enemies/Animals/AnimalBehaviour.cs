using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class AnimalBehaviour : UnarmedBehaviour
    {
        private bool Fleeing;
        private Cell _fleeTarget;

        protected override void OnAlert()
        {
        }

        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
            DetectionRange = 0f;
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            CheckToFade();
        }

        private void CheckToFade()
        {
            if (!Fleeing) return;
            if (_fleeTarget.Position.Distance(transform.position) > 1f) return;
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