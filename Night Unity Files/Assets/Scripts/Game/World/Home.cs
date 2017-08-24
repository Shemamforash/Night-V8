using System;
using System.Collections.Generic;
using Facilitating.Persistence;
using Persistence;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.World
{
    public class Home : MonoBehaviour
    {
        private PersistenceListener _persistenceListener;
        private static Inventory _homeInventory = new Inventory();

        public static Inventory Inventory()
        {
            return _homeInventory;
        }
        
        public void Awake()
        {
            GenerateResource("Water", "sips");
            GenerateResource("Food", "meals");
            GenerateResource("Fuel", "dregs");
            GenerateResource("Ammo", "rounds");
#if UNITY_EDITOR
            _homeInventory.IncrementResource("Ammo", 100);
#endif
            _persistenceListener = new PersistenceListener(Load, Save, "Home");
        }

        private void GenerateResource(string name, string convention)
        {
            Func<float, string> conversion = f => Mathf.Round(f).ToString() + " " + convention;
            Text resourceText = GameObject.Find(name).transform.Find("Text").GetComponent<Text>();
            ReactiveText<float> reactiveText = new ReactiveText<float>(resourceText, conversion);
            _homeInventory.AddResource(name, reactiveText);
        }
        
        public void Load()
        {
            _homeInventory.IncrementResource("Water", GameData.StoredWater);
            _homeInventory.IncrementResource("Food", GameData.StoredFood);
            _homeInventory.IncrementResource("Fuel", GameData.StoredFuel);
        }

        public void Save()
        {
            GameData.StoredWater = _homeInventory.GetResourceQuantity("Water");
            GameData.StoredFood = _homeInventory.GetResourceQuantity("Food");
            GameData.StoredFuel = _homeInventory.GetResourceQuantity("Fuel");
        }
    }
}