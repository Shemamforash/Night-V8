﻿using System;
using Game.Characters;
using Game.Combat.CharacterUi;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using Game.Gear.Weapons;
using SamsHelper;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        public readonly EnemyTemplate Template;
        public bool IsDead;
        
        public Enemy(EnemyType type) : base(type.ToString())
        {
            Template = EnemyTemplate.GetEnemyTemplate(type);
            Reset();
        }

        public void GenerateWeapon(float difficulty)
        {
            if (Template.AllowedWeaponTypes.Count == 0) return;
            ItemQuality targetQuality = (ItemQuality) Enum.Parse(typeof(ItemQuality), Mathf.FloorToInt(difficulty * 5).ToString());
            int durability = Random.Range(0, 10);
            Weapon weapon = WeaponGenerator.GenerateWeapon(targetQuality, Template.AllowedWeaponTypes, durability);
            EquipWeapon(weapon);
        }

        public void GenerateArmour(float difficulty)
        {
            int armourPivot = (int) (difficulty / 2);
            int minArmour = armourPivot - 1;
            if (minArmour < 0) minArmour = 0;
            int maxArmour = armourPivot + 1;
            if (maxArmour > 10) maxArmour = 10;
            ArmourController.AutoFillSlots();
        }

        private void Reset()
        {
            CharacterInventory.SetEnemyResources();
            Weapon?.Reload(Inventory());
        }

        public EnemyBehaviour CreateEnemyObject()
        {
            Reset();
            EnemyBehaviour enemyBehaviour;
            GameObject enemyObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Combat Character"));
            switch (Template.EnemyType)
            {
                case EnemyType.Brawler:
                    enemyBehaviour = enemyObject.AddComponent<Brawler>();
                    break;
                case EnemyType.Martyr:
                    enemyBehaviour = enemyObject.AddComponent<Martyr>();
                    break;
                case EnemyType.Medic:
                    enemyBehaviour = enemyObject.AddComponent<Medic>();
                    break;
                case EnemyType.Mountain:
                    enemyBehaviour = enemyObject.AddComponent<Mountain>();
                    break;
                case EnemyType.Sentinel:
                    enemyBehaviour = enemyObject.AddComponent<Sentinel>();
                    break;
                case EnemyType.Sniper:
                    enemyBehaviour = enemyObject.AddComponent<Sniper>();
                    break;
                case EnemyType.Warlord:
                    enemyBehaviour = enemyObject.AddComponent<Warlord>();
                    break;
                case EnemyType.Witch:
                    enemyBehaviour = enemyObject.AddComponent<Witch>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            enemyBehaviour.Initialise(this);
            return enemyBehaviour;
        }

        public override void Kill()
        {
            IsDead = true;
            //todo register kill
        }
    }
}