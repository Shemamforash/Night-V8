using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class ShelterCharacterBehaviour : CharacterCombat
    {
        private static GameObject _shelterCharacterPrefab;
        private TextMeshProUGUI _text;
        private bool _followPlayer;

        public override void Awake()
        {
            base.Awake();
            _text = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Text");
            HealthController.SetInitialHealth(1000, this);
            transform.SetParent(GameObject.Find("World").transform);
        }
        
        public override Weapon Weapon()
        {
            return null;
        }

        public override CharacterCombat GetTarget()
        {
            return null;
        }

        public static void Generate(Vector2 position)
        {
            if (_shelterCharacterPrefab == null) _shelterCharacterPrefab = Resources.Load<GameObject>("Prefabs/Combat/Shelter Character");
            GameObject characterObject = Instantiate(_shelterCharacterPrefab);
            characterObject.transform.position = position;
        }

        public void Enter()
        {
            Sequence sequence = DOTween.Sequence();
            _text.text = "Help me, please...";
            sequence.Append(_text.DOFade(1, 1));
            sequence.AppendInterval(3);
            _text.DOFade(0, 1);
            GenerateEncounter();
            _followPlayer = true;
        }

        public void Update()
        {
            if (_followPlayer)
            {
                
            }
        }

        private void GenerateEncounter()
        {
            int size = WorldState.Difficulty() + 10;

            List<EnemyTemplate> allowedTypes = WorldState.GetAllowedHumanEnemyTypes();
            while (size > 0)
            {
                EnemyTemplate template = Helper.RandomInList(allowedTypes);
                EnemyBehaviour enemyBehaviour = CombatManager.QueueEnemyToAdd(template);
                if(Random.Range(0, 3) == 0) enemyBehaviour.SetTarget(this);
                size -= template.Value;
            }
        }
    }
}