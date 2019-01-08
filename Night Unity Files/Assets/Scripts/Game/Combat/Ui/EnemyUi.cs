using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        public EnhancedText NameText;
        public UIHitController UiHitController;
        public static EnemyUi Instance;

        public override void Awake()
        {
            base.Awake();
            NameText = gameObject.FindChildWithName<EnhancedText>("Name");
            UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
            Instance = this;
        }

        protected override void LateUpdate()
        {
            Character = PlayerCombat.Instance.GetTarget();
            base.LateUpdate();
            if (Character == null) return;
            NameText.SetText(Character.GetDisplayName());
        }
    }
}