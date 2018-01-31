using Game.Characters;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class BasicEnemyView : ViewParent
    {
        private const float AlphaCutoff = 0.2f;
        public const float FadeVisibilityDistance = 5f;
        public TextMeshProUGUI DistanceText, _nameText;
        protected float CurrentAlpha;
        private CombatItem _combatItem;

        public BasicEnemyView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/OtherCombatItem") : base(linkedObject, parent, prefabLocation)
        {
            GameObject.SetActive(true);
            SetAlpha(0f);
            _combatItem = (CombatItem) linkedObject;
        }

        protected override void CacheUiElements()
        {
            base.CacheUiElements();
            DistanceText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Distance");
            _nameText = Helper.FindChildWithName<TextMeshProUGUI>(GameObject, "Name");
        }

        public override void Update()
        {
            base.Update();
            _nameText.text = LinkedObject.Name;
        }

        public void SetAlpha(float alpha)
        {
            GetGameObject().GetComponent<CanvasGroup>().alpha = alpha;
            CurrentAlpha = alpha;
        }

        public void UpdateDistance()
        {
            UpdateDistanceText();
            UpdateDistanceAlpha();
        }

        protected virtual void UpdateDistanceText()
        {
            float distance = Helper.Round(_combatItem.DistanceToPlayer);
            string distanceText = distance + "m";
            DistanceText.text = distanceText;
        }

        private void UpdateDistanceAlpha()
        {
            float distanceToMaxVisibility = CombatManager.VisibilityRange + FadeVisibilityDistance - _combatItem.DistanceToPlayer;
            float alpha = 0;
            if (_combatItem.DistanceToPlayer < CombatManager.VisibilityRange)
            {
                float normalisedDistance = Helper.Normalise(_combatItem.DistanceToPlayer, CombatManager.VisibilityRange);
                alpha = 1f - normalisedDistance;
                alpha = Mathf.Clamp(alpha, AlphaCutoff, 1f);
            }
            else if (distanceToMaxVisibility >= 0)
            {
                alpha = Helper.Normalise(distanceToMaxVisibility, FadeVisibilityDistance);
                alpha = Mathf.Clamp(alpha, 0, AlphaCutoff);
            }

            SetNavigatable(alpha > 0);
            SetAlpha(alpha);
        }
    }
}