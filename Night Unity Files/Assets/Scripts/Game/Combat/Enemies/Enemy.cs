using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Enemies.Animals;
using Game.Combat.Enemies.Humans;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Gear;
using Game.Gear.Armour;
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
        private static GameObject _enemyPrefab;
        private static GameObject _footStepPrefab, _trailPrefab, _hoofprintPrefab;
        private static readonly Dictionary<EnemyType, Sprite> _enemySprites = new Dictionary<EnemyType, Sprite>();

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

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            doc.CreateChild("EnemyType", Template.EnemyType);
            return doc;
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

        private void AssignSprite(GameObject enemyObject)
        {
            if (!_enemySprites.ContainsKey(Template.EnemyType))
            {
                Sprite sprite = Resources.Load<Sprite>("Images/Enemy Symbols/" + Template.EnemyType);
                if (sprite == null) sprite = Resources.Load<Sprite>("Images/Enemy Symbols/Sentinel");
                Assert.IsNotNull(sprite);
                _enemySprites.Add(Template.EnemyType, sprite);
            }

            enemyObject.GetComponent<SpriteRenderer>().sprite = _enemySprites[Template.EnemyType];
        }

        private void AssignTrail(GameObject enemyObject)
        {
            GameObject trailPrefab = null;
            switch (Template.Species)
            {
                case "Human":
                    if (_footStepPrefab == null) _footStepPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Footstep Trail");
                    trailPrefab = _footStepPrefab;
                    break;
                case "Animal":
                    if (_hoofprintPrefab == null) _hoofprintPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Hoofprint Trail");
                    trailPrefab = _hoofprintPrefab;
                    break;
                case "Nightmare":
                    if (_trailPrefab == null) _trailPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Nightmare Trail");
                    trailPrefab = _trailPrefab;
                    break;
            }

            GameObject trailObject = GameObject.Instantiate(trailPrefab);
            trailObject.transform.SetParent(enemyObject.transform);
            trailObject.transform.localPosition = Vector2.zero;
        }

        public EnemyBehaviour GetEnemyBehaviour()
        {
            if (_enemyPrefab == null) _enemyPrefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Default Enemy");
            GameObject enemyObject = GameObject.Instantiate(_enemyPrefab);
            enemyObject.name = Template.EnemyType.ToString() + ID();
            AssignSprite(enemyObject);
            AssignTrail(enemyObject);
            EnemyBehaviour enemyBehaviour;
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
                case EnemyType.Ghoul:
                    enemyBehaviour = enemyObject.AddComponent<Ghoul>();
                    break;
                case EnemyType.GhoulMother:
                    enemyBehaviour = enemyObject.AddComponent<GhoulMother>();
                    break;
                case EnemyType.Maelstrom:
                    enemyBehaviour = enemyObject.AddComponent<Maelstrom>();
                    break;
                case EnemyType.Decoy:
                    enemyBehaviour = enemyObject.AddComponent<Decoy>();
                    break;
                case EnemyType.Ghast:
                    enemyBehaviour = enemyObject.AddComponent<Ghast>();
                    break;
                case EnemyType.Nightmare:
                    enemyBehaviour = enemyObject.AddComponent<Nightmares.Nightmare>();
                    break;
                case EnemyType.Revenant:
                    enemyBehaviour = enemyObject.AddComponent<Revenant>();
                    break;
                case EnemyType.Shadow:
                    enemyBehaviour = enemyObject.AddComponent<Shadow>();
                    break;
                case EnemyType.Drone:
                    enemyBehaviour = enemyObject.AddComponent<Drone>();
                    break;
                case EnemyType.Grazer:
                    enemyBehaviour = enemyObject.AddComponent<Grazer>();
                    break;
                case EnemyType.Watcher:
                    enemyBehaviour = enemyObject.AddComponent<Watcher>();
                    break;
                case EnemyType.Curio:
                    enemyBehaviour = enemyObject.AddComponent<Curio>();
                    break;
                case EnemyType.Flit:
                    enemyBehaviour = enemyObject.AddComponent<Flit>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            enemyBehaviour.Initialise(this);
            return enemyBehaviour;
        }

        public void Kill()
        {
            IsDead = true;
        }

        public Loot DropLoot(Vector2 position)
        {
            Loot controller = new Loot(position, Name);
            Debug.Log(Template.DropResource);
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
                        controller.AddToInventory(ResourceTemplate.Create("Skin"));
                        break;
                    }

                    controller.AddToInventory(ResourceTemplate.GetMeat());
                    break;
            }

            return controller;
        }

        public int GetHealth()
        {
            return Template.Health + Template.Health / WorldState.MaxDifficulty * WorldState.Difficulty();
        }
    }
}