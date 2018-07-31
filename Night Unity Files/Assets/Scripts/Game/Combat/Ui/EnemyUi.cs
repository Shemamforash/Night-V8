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
            if (_instance == null) _instance = FindObjectOfType<EnemyUi>();
            return _instance;
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            NameText = gameObject.FindChildWithName<TextMeshProUGUI>("Name");
            UiHitController = gameObject.FindChildWithName<UIHitController>("Cover");
            ActionText = gameObject.FindChildWithName<TextMeshProUGUI>("Action");
            SetSelectedEnemy(null);
        }

        public void SetSelectedEnemy(EnemyBehaviour enemy)
        {
            _selectedEnemy = enemy;
            if (enemy == null)
            {
                SetAlpha(0);
                return;
            }

            SetAlpha(1);
            NameText.text = enemy.GetEnemyName();
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
            return enemy != _selectedEnemy ? null : base.GetHealthController(enemy);
        }

        public override UIArmourController GetArmourController(Character character)
        {
            if (_selectedEnemy == null) return null;
            return character != _selectedEnemy.Enemy ? null : base.GetArmourController(character);
        }
    }
}