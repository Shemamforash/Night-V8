using Game.Characters;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies
{
    public class Enemy : Character
    {
        public readonly EnemyTemplate Template;
        private bool IsDead;

        public Enemy(EnemyTemplate template) : base(template.EnemyType.ToString())
        {
            Template = template;
            if (Template.HasGear)
            {
                GenerateArmour();
            }

            if (Template.HasWeapon)
            {
                GenerateWeapon();
            }
        }

        private void GenerateWeapon()
        {
            int difficulty = WorldState.GenerateGearLevel();
            ItemQuality targetQuality = (ItemQuality) difficulty;
            Assert.IsTrue((int) targetQuality < 5);
            Weapon weapon = WeaponGenerator.GenerateWeapon(targetQuality);
            EquipWeapon(weapon);
            bool hasInscription = Random.Range(0, 4) == 0;
            if (hasInscription)
            {
                weapon.SetInscription(Inscription.Generate());
            }
        }

        private void GenerateArmour()
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
                    if (EquippedWeapon != null && Random.Range(0, 20) == 0) controller.AddToInventory(EquippedWeapon);
                    else if (Random.Range(0, 20) == 0) controller.AddToInventory(Accessory.Generate());
                    break;
                case "Essence":
                    EssenceCloudBehaviour.Create(position, Template.DropCount);
                    if (Random.Range(0, 20) == 0) controller.AddToInventory(Inscription.Generate());
                    break;
                case "Meat":
                    if (Template.EnemyType == EnemyType.Grazer || Template.EnemyType == EnemyType.Watcher && Random.Range(0, 2) == 1)
                    {
                        controller.AddToInventory(ResourceTemplate.AllResources.Find(r => r.Name == "Skin").Create());
                        break;
                    }

                    controller.AddToInventory(ResourceTemplate.GetMeat().Create());
                    break;
            }

            return controller;
        }
    }
}