using Game.Characters;

namespace Game.Exploration.WorldEvents
{
    public class CharacterMessage : WorldEvent
    {
        public CharacterMessage(string eventText, Player player) : base("<i>\"" + eventText + "\"- " + player.Name + "</i>")
        {
        }
    }
}