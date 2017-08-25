using System;

namespace SamsHelper
{
    public class Exceptions
    {
        public class LoadOrSaveNotSetException : Exception
        {
            public override string Message
            {
                get { return "A save or load action was not set in a PersistenceListener"; }
            }
        }

        public class UnrecognisedWeightCategoryException : Exception
        {
            public override string Message
            {
                get { return "Unrecognised Weight Category Assigned: "; }
            }
        }

        public class TraitAttributeNotRecognisedException : Exception
        {
            private readonly string _attribute;

            public TraitAttributeNotRecognisedException(string attribute)
            {
                _attribute = attribute;
            }

            public override string Message
            {
                get { return "Trait attribute not recognised in traits.txt file: " + _attribute; }
            }
        }

        public class UnknownTraitException : Exception
        {
            private readonly string _trait;

            public UnknownTraitException(string trait)
            {
                _trait = trait;
            }

            public override String Message
            {
                get { return "Trait not recognised: " + _trait; }
            }
        }

        public class CappedValueExceededBoundsException : Exception
        {
            public override string Message
            {
                get { return "Capped value exceeded bounds"; }
            }
        }

        public class UnspecificGameObjectNameException : Exception
        {
            private readonly int _occurences;
            private readonly string _name;
            
            public UnspecificGameObjectNameException(int occurences, string name)
            {
                _occurences = occurences;
                _name = name;
            }
            
            public override string Message
            {
                get { return "GameObject name too general, found " + _occurences + " occurences of " + _name; }
            }
        }

        public class CannotGetGameObjectComponent : Exception
        {
            public override string Message
            {
                get { return "GameObject references cannot be gathered from getcomponent() calls."; }
            }
        }

        public class ResourceValueChangeInvalid : Exception
        {
            private readonly string _direction;
            private readonly string _resourceName;
            private readonly float _amount;
            public ResourceValueChangeInvalid(string resourceName, string decrement, float amount)
            {
                _resourceName = resourceName;
                _direction = decrement;
                _amount = amount;
            }

            public override string Message
            {
                get
                {
                    return "Attempted to " + _direction + " resource " + _resourceName + " by " + _amount;
                }
            }
        }
    }
}