using System.Collections.Generic;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Ui
{
    public class PlayerUi : CharacterUi
    {
        private static PlayerUi _instance;

        public override void Awake()
        {
            base.Awake();
            _instance = this;
        }

        public static PlayerUi Instance()
        {
            if (_instance != null) return _instance;
            _instance = FindObjectOfType<PlayerUi>();
            return _instance;
        }

        public void Update()
        {
            List<CanTakeDamage> chars = CombatManager.GetEnemiesInRange(PlayerCombat.Instance.transform.position, 5f);
            Vector2 playerDir = PlayerCombat.Instance.transform.up;
            float nearestAngle = 360;
            CanTakeDamage nearestCharacter = null;
            chars.ForEach(c =>
            {
                Vector2 enemyDir = c.transform.position - PlayerCombat.Instance.transform.position;
                float enemyAngle = Vector2.Angle(playerDir, enemyDir);
                if (enemyAngle > 10) return;
                if (enemyAngle > nearestAngle) return;
                nearestAngle = enemyAngle;
                nearestCharacter = c;
            });

            PlayerCombat.Instance.SetTarget(nearestCharacter);
        }

        protected override void LateUpdate()
        {
            Character = PlayerCombat.Instance;
            base.LateUpdate();
            if (Character == null) return;
            GetHealthController().SetValue(PlayerCombat.Instance.HealthController.GetHealth());
            GetArmourController().TakeDamage(PlayerCombat.Instance.ArmourController);
        }

    }
}