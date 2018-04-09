using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using TMPro;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        private static EnemyUi _instance;
        private EnemyBehaviour _selectedEnemy;
        public TextMeshProUGUI ActionText;
        public TextMeshProUGUI NameText;
        public UIHitController UiHitController;

        public static EnemyUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<EnemyUi>();
            return _instance;
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            UiHitController = Helper.FindChildWithName<UIHitController>(gameObject, "Cover");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Action");
        }

        public void SetSelectedEnemy(EnemyBehaviour enemy)
        {
            _selectedEnemy = enemy;
            NameText.text = enemy.Enemy.Name;
            ActionText.text = enemy.ActionText;
            enemy.HealthController.UpdateHealth();
        }

        public void RegisterHit(EnemyBehaviour enemy)
        {
            if (enemy != _selectedEnemy) return;
            UiHitController.RegisterShot();
        }

        public void UpdateActionText(EnemyBehaviour enemy, string text)
        {
            if (enemy != _selectedEnemy) return;
            ActionText.text = text;
        }

        public override UIHealthBarController GetHealthController(CharacterCombat enemy)
        {
            return enemy != _selectedEnemy ? null : _healthBarController;
        }

        public override UIArmourController GetArmourController(Character character)
        {
            return character != _selectedEnemy.Enemy ? null : _armourController;
        }
    }
}