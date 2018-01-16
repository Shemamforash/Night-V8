using Game.Characters.Player;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;

namespace Game.Combat.Enemies.EnemyTypes.Misc
{
    public class BasicEnemyView : ViewParent
    {
        public TextMeshProUGUI DistanceText, _nameText;
        
        public BasicEnemyView(MyGameObject linkedObject, Transform parent, string prefabLocation = "Prefabs/Inventory/OtherCombatItem") : base(linkedObject, parent, prefabLocation)
        {
            GameObject.SetActive(true);
            SetAlpha(0f);
            SetNavigatable(false);
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
        }
    }
}