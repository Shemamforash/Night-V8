using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class EnemyUi : CharacterUi
    {
        private static EnemyUi _instance;
        private EnemyBehaviour _selectedEnemy;
        public TextMeshProUGUI ActionText;
        public TextMeshProUGUI NameText;
        public UIHitController UiHitController;
        private CanvasGroup _canvasGroup;

        public static EnemyUi Instance()
        {
            if (_instance == null) _instance = FindObjectOfType<EnemyUi>();
            return _instance;
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _canvasGroup = GetComponent<CanvasGroup>();
            NameText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Name");
            UiHitController = Helper.FindChildWithName<UIHitController>(gameObject, "Cover");
            ActionText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Action");
        }

        public void SetSelectedEnemy(EnemyBehaviour enemy)
        {
            _selectedEnemy = enemy;
            if (enemy == null)
            {
                _canvasGroup.alpha = 0;
                return;
            }

            _canvasGroup.alpha = 1;
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