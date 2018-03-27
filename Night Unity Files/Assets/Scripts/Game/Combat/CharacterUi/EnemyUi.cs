using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

namespace Game.Combat.CharacterUi
{
    public class EnemyUi : CharacterUi
    {
        public TextMeshProUGUI NameText;
        public UIHitController UiHitController;
        public TextMeshProUGUI ActionText;
        public EnhancedButton PrimaryButton;

        public override void Awake()
        {
            base.Awake();
            gameObject.GetComponent<CanvasGroup>().alpha = UiAppearanceController.FadedColour.a;
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            UiHitController = Helper.FindChildWithName<UIHitController>(gameObject, "Cover");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Action");
            PrimaryButton = gameObject.GetComponent<EnhancedButton>();
            CanvasGroup.alpha = 0f;
        }
    }
}