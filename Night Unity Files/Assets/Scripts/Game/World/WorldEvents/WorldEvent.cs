namespace Game.World.WorldEvents
{
    public class WorldEvent
    {
        private readonly string _eventText;
        
        public WorldEvent(string eventText)
        {
            _eventText = eventText;
        }

        public virtual string EventText()
        {
            return _eventText;
        }
    }
}