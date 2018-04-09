namespace Game.Exploration.WorldEvents
{
    public class WorldEvent
    {
        protected readonly string EventText;

        public WorldEvent(string eventText)
        {
            EventText = eventText;
        }

        public virtual string Text()
        {
            return EventText;
        }
    }
}