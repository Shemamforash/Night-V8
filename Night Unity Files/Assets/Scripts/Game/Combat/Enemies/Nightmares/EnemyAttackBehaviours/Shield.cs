using System.Collections;
using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Shield : MonoBehaviour
    {
        private SpriteRenderer _sprite;

        public void Awake()
        {
            _sprite = gameObject.FindChildWithName<SpriteRenderer>("Hit");
            _sprite.color = UiAppearanceController.InvisibleColour;
        }

        public void Activate(float duration)
        {
            StartCoroutine(ActivateShield(duration));
        }

        private IEnumerator ActivateShield(float f)
        {
            _sprite.color = Color.white;
            while (f > 0f)
            {
                f -= Time.deltaTime;
                yield return null;
            }

            _sprite.DOColor(UiAppearanceController.InvisibleColour, 0.5f);
        }
    }
}