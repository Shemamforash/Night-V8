using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
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
            List<ITakeDamageInterface> chars = CombatManager.GetEnemiesInRange(PlayerCombat.Instance.transform.position, 10f);
            Vector2 playerDir = PlayerCombat.Instance.transform.up;
            float nearestAngle = 360;
            ITakeDamageInterface nearestCharacter = null;
            chars.ForEach(c =>
            {
                GameObject enemy = c.GetGameObject();
                if (!enemy.OnScreen()) return;
                Vector2 enemyDir = enemy.transform.position - PlayerCombat.Instance.transform.position;
                float enemyAngle = Vector2.Angle(playerDir, enemyDir);
                if (enemyAngle > 10) return;
                if (enemyAngle > nearestAngle) return;
                nearestAngle = enemyAngle;
                nearestCharacter = c;
            });

            PlayerCombat.Instance.SetTarget(nearestCharacter as EnemyBehaviour);
        }
    }
}