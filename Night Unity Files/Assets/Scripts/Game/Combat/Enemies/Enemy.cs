using System;
using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Enemies.EnemyTypes.Humans;
using Game.Combat.Enemies.EnemyTypes.Misc;
using Game.Combat.Skills;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
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
            if (Template.AllowedWeaponTypes.Count != 0)
            {
                Weapon weapon = WeaponGenerator.GenerateWeapon(Template.AllowedWeaponTypes, 1);
                EquipWeapon(weapon);
            }
            Reset();
        }

        private void Reset()
        {
            CharacterInventory.SetEnemyResources();
            Weapon?.Reload(Inventory());
        }

        public DetailedEnemyCombat CreateUi(Transform parent)
        {
            Reset();
            GameObject enemyUiPrefab = Helper.InstantiateUiObject("Prefabs/Inventory/EnemyItem", parent);
            DetailedEnemyCombat enemyCombat;
            switch (Template.EnemyType)
            {
                case EnemyType.Brawler:
                    enemyCombat = enemyUiPrefab.AddComponent<Brawler>();
                    break;
                case EnemyType.Martyr:
                    enemyCombat = enemyUiPrefab.AddComponent<Martyr>();
                    break;
                case EnemyType.Medic:
                    enemyCombat = enemyUiPrefab.AddComponent<Medic>();
                    break;
                case EnemyType.Mountain:
                    enemyCombat = enemyUiPrefab.AddComponent<Mountain>();
                    break;
                case EnemyType.Sentinel:
                    enemyCombat = enemyUiPrefab.AddComponent<Sentinel>();
                    break;
                case EnemyType.Sniper:
                    enemyCombat = enemyUiPrefab.AddComponent<Sniper>();
                    break;
                case EnemyType.Warlord:
                    enemyCombat = enemyUiPrefab.AddComponent<Warlord>();
                    break;
                case EnemyType.Witch:
                    enemyCombat = enemyUiPrefab.AddComponent<Witch>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            enemyCombat.SetPlayer(this);
            return enemyCombat;
        }

        public override void Kill()
        {
            IsDead = true;
            //todo register kill
        }
    }
}