using Game.Characters;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;
using UnityEngine.Assertions;
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

        public void GenerateWeapon()
        {
            int difficulty = Mathf.FloorToInt(WorldState.Difficulty() / 10f);
            int difficultyMin = difficulty - 1;
            if (difficultyMin < 0) difficultyMin = 0;
            else if (difficultyMin > 4) difficultyMin = 4;
            int difficultyMax = difficulty + 1;
            if (difficultyMax > 4) difficultyMax = 4;
            
            ItemQuality targetQuality = (ItemQuality)Random.Range(difficultyMin, difficultyMax);
            Assert.IsTrue((int)targetQuality < 5);
            Weapon weapon = WeaponGenerator.GenerateWeapon(targetQuality);

            EquipWeapon(weapon);
            
            bool hasInscription = Random.Range(0, 4) == 0;
            if (hasInscription)
            {
                weapon.SetInscription(Inscription.Generate());
            }
        }

        public void GenerateArmour()
        {
            int difficulty = Mathf.FloorToInt(WorldState.Difficulty() / 5f);
            int armourMin = difficulty - 2;
            if (armourMin < 0) armourMin = 0;
            else if (armourMin > 10) armourMin = 10;
            int armourMax = difficulty + 2;
            if (armourMax < 0) armourMax = 0;
            else if (armourMax > 10) armourMax = 10;
            ArmourController.AutoFillSlots(Random.Range(armourMin, armourMax));
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
                    controller.AddToInventory(ResourceTemplate.AllResources.Find(r => r.Name == "Skin").Create());
                    break;
            }

            if (Weapon != null && Random.Range(0, 20) == 0) controller.AddToInventory(Weapon);

            return controller;
        }
    }
}