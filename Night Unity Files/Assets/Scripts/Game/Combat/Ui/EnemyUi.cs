using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using TMPro;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        private static EnemyUi _instance;
        public TextMeshProUGUI NameText;
        public UIHitController UiHitController;

        public static EnemyUi Instance()
        {
            if (_instance == null) _instance = FindObjectOfType<EnemyUi>();
            return _instance;
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            NameText = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
            UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
        }

        protected override void LateUpdate()
        {
            Character = PlayerCombat.Instance.GetTarget();
            base.LateUpdate();
            if (Character == null) return;
            NameText.text = Character.GetDisplayName();
            GetHealthController().SetValue(Character.HealthController.GetHealth());
            GetArmourController().TakeDamage(Character.ArmourController);
        }

        public void RegisterHit(CanTakeDamage enemy)
        {
            if (enemy != Character) return;
            UiHitController.RegisterShot();
        }
    }
}