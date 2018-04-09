using System.Collections.Generic;
using UnityEngine;

namespace Game.Exploration.Region
{
    public class RegionTemplate
    {
        public string Encounters, Items;
        public string InternalName, DisplayName;
        public List<string> Names;
        public int WaterAvailable, FoodAvailable, FuelAvailable, ScrapAvailable, AmmoAvailable;

        public string GenerateName()
        {
            int pos = Random.Range(0, Names.Count);
            string chosenName = Names[pos];
            Names.RemoveAt(pos);
            return chosenName;
        }
    }
}