using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        public EnhancedText NameText;
        public UIHitController UiHitController;

        public override void Awake()
        {
            base.Awake();
            NameText = gameObject.FindChildWithName<EnhancedText>("Name");
            UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
        }

        protected override void LateUpdate()
        {
            Character = PlayerCombat.Instance.GetTarget();
            base.LateUpdate();
            if (Character == null) return;
            NameText.SetText(Character.GetDisplayName());
        }

        public void RegisterHit(CanTakeDamage enemy)
        {
            if (enemy != Character) return;
            UiHitController.RegisterShot();
        }
    }
}