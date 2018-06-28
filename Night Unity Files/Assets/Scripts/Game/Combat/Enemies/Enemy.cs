using System;
using Game.Characters;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        public readonly EnemyTemplate Template;
        private bool IsDead;

        public Enemy(EnemyType type) : base(type.ToString())
        {
            Template = EnemyTemplate.GetEnemyTemplate(type);
        }

        public void GenerateWeapon(float difficulty)
        {
            ItemQuality targetQuality = (ItemQuality) Enum.Parse(typeof(ItemQuality), Mathf.FloorToInt(difficulty * 5).ToString());
            Weapon weapon = WeaponGenerator.GenerateWeapon(targetQuality);
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

        public EnemyBehaviour GetEnemyBehaviour()
        {
            GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/" + Template.EnemyType);
            GameObject enemyObject = GameObject.Instantiate(enemyPrefab);
            enemyObject.name = Template.EnemyType.ToString() + Id;
            EnemyBehaviour enemyBehaviour = enemyObject.GetComponent<EnemyBehaviour>();
            enemyBehaviour.Initialise(this);
            return enemyBehaviour;
        }

        public void Kill()
        {
            IsDead = true;
            //todo register kill
        }

        public Loot DropLoot(Vector2 position)
        {
            Loot controller = new Loot(position, Name);
            switch (Template.DropResource)
            {
                case "Salt":
                    SaltBehaviour.Create(position, Template.DropCount);
                    break;
                case "Essence":
                    EssenceCloudBehaviour.Create(position, Template.DropCount);
                    break;
                case "Meat":
                    controller.AddToInventory(ResourceTemplate.GetMeat().Create());
                    break;
            }

            if (Weapon != null && Random.Range(0, 10) == 0) controller.AddToInventory(Weapon);

            return controller;
        }
    }
}