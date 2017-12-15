using System.Collections.Generic;
using UnityEngine;

namespace Game.World
{
    public class RegionTemplate
    {
        public string InternalName, DisplayName, Type, Encounters, Items;
        public int WaterAvailable, FoodAvailable, FuelAvailable, ScrapAvailable, AmmoAvailable;
        public List<string> Names;
        
        public string GenerateName()
        {
            int pos = Random.Range(0, Names.Count);
            string chosenName = Names[pos];
            Names.RemoveAt(pos);
            return chosenName;
        }
    }
}