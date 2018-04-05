using System.Collections.Generic;
using Game.World.Region;
using UnityEngine;

namespace Game.World
{
    public class RegionTemplate
    {
        public string InternalName, DisplayName;
        public RegionType Type;
        public string Encounters, Items;
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