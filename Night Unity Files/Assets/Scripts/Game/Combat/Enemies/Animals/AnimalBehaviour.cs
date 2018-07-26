using System.Collections;
using Game.Combat.Generation;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class AnimalBehaviour : UnarmedBehaviour
    {
        protected bool Fleeing;

        protected override void OnAlert()
        {
        }

        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
        }

        protected void Flee(Cell target)
        {
            MoveBehaviour.GoToCell(target, () => StartCoroutine(FleeArea()));
            SetActionText("Fleeing");
            Fleeing = true;
        }

        private IEnumerator FleeArea()
        {
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

        protected override void CheckForPlayer()
        {
            if (Fleeing) return;
            base.CheckForPlayer();
        }
    }
}