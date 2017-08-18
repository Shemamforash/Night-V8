using System;
using Persistence;

namespace SamsHelper.Persistence
{
    public class PersistenceListener
    {
        private Action LoadEvent, SaveEvent;
        public string parent;

        public PersistenceListener(Action LoadEvent, Action SaveEvent, string parent)
        {
            this.parent = parent;
            this.LoadEvent = LoadEvent;
            this.SaveEvent = SaveEvent;
            SaveController.Register(this);
        }

        public void Load()
        {
            if (LoadEvent != null)
            {
                LoadEvent();
            }
            else
            {
                throw new Exceptions.LoadOrSaveNotSetException();
            }
        }

        public void Save()
        {
            if (LoadEvent != null)
            {
                SaveEvent();
            }
            else
            {
                throw new Exceptions.LoadOrSaveNotSetException();
            }
        }

    }
}