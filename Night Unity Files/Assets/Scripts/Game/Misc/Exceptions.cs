using System;

namespace Game.Misc
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
    }
}