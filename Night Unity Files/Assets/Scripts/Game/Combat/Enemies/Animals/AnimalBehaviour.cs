using System.Collections;
using Game.Combat.Generation;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class AnimalBehaviour : UnarmedBehaviour
    {
        private bool _fleeing;

        public override void Initialise(Enemy e)
        {
            base.Initialise(e);
        }

        protected virtual void Flee()
        {
            Cell target = PathingGrid.GetCellOutOfRange();
            GetRouteToCell(target, () => StartCoroutine(FleeArea()));
            SetActionText("Fleeing");
            _fleeing = true;
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
            if (_fleeing) return;
            base.CheckForPlayer();
        }
    }
}