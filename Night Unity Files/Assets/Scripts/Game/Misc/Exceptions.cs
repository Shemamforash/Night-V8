using System;

public class Exceptions
{
    public class LoadOrSaveNotSetException : Exception
    {
        public override string Message
        {
            get
            {
                return "A save or load action was not set in a PersistenceListener";
            }
        }
    }
}