using UnityEngine;

namespace Game.World.WorldEvents
{
    public class WeaponFindEvent : WorldEvent
    {
        private string[] weaponFindStrings =
        {
            "Found a {0}, let the blood flow.",
            "{0}- a generous gift, if only to take life.",
            "{0}, a curse from the dead gods.",
            "Found a {0}, though it will not save me from the darkness."
        };

        public WeaponFindEvent(string eventText) : base(eventText)
        {
        }

        public override string Text()
        {
            string chosenEventText = weaponFindStrings[Random.Range(0, weaponFindStrings.Length)];
            return string.Format(chosenEventText, EventText);
        }
    }
}