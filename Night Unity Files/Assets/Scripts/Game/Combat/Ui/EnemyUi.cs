using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using TMPro;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        public TextMeshProUGUI NameText;
        public UIHitController UiHitController;

        public override void Awake()
        {
            base.Awake();
            NameText = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
            UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
        }

        protected override void LateUpdate()
        {
            Character = PlayerCombat.Instance.GetTarget();
            base.LateUpdate();
            if (Character == null) return;
            NameText.text = Character.GetDisplayName();
        }

        public void RegisterHit(CanTakeDamage enemy)
        {
            if (enemy != Character) return;
            UiHitController.RegisterShot();
        }
    }
}