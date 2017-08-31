using System;
using System.Collections.Generic;

namespace Game.World.Time
{
    public class TimeListener
    {
        private List<Action> hourEvents = new List<Action>();
        private List<Action> dayEvents = new List<Action>();
        private List<Action> travelEvents = new List<Action>();
        private List<Action> minuteEvents = new List<Action>();
        private List<Action<bool>> pauseEvents = new List<Action<bool>>();

        public TimeListener()
        {
            WorldTime.SubscribeTimeListener(this);
        }

        public void ReceiveHourEvent()
        {
            hourEvents.ForEach(a => a());
        }

        public void ReceiveDayEvent()
        {
            dayEvents.ForEach(a => a());
        }

        public void ReceivePauseEvent(bool paused)
        {
            pauseEvents.ForEach(a => a(paused));
        }

        public void ReceiveMinuteEvent()
        {
            minuteEvents.ForEach(a => a());
        }

        public void ReceiveTravelEvent()
        {
            travelEvents.ForEach(a => a());
        }

        public void OnHour(Action hourEvent)
        {
            hourEvents.Add(hourEvent);
        }

        public void OnDay(Action dayEvent)
        {
            dayEvents.Add(dayEvent);
        }

        public void OnPause(Action<bool> pauseEvent)
        {
            pauseEvents.Add(pauseEvent);
        }

        public void OnTravel(Action travelEvent)
        {
            travelEvents.Add(travelEvent);
        }

        public void OnMinute(Action minuteEvent)
        {
            minuteEvents.Add(minuteEvent);
        }
    }
}