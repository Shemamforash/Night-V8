using System;
using Game.Gear;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;

namespace SamsHelper
{
    public class Exceptions
    {
        public class LoadOrSaveNotSetException : Exception
        {
            public override string Message => "A save or load action was not set in a PersistenceListener";
        }

        public class UnrecognisedWeightCategoryException : Exception
        {
            public override string Message => "Unrecognised Weight Category Assigned: ";
        }

        public class TraitAttributeNotRecognisedException : Exception
        {
            private readonly string _attribute;

            public TraitAttributeNotRecognisedException(string attribute)
            {
                _attribute = attribute;
            }

            public override string Message => "Trait attribute not recognised in traits.txt file: " + _attribute;
        }

        public class UnknownTraitException : Exception
        {
            private readonly string _trait;

            public UnknownTraitException(string trait)
            {
                _trait = trait;
            }

            public override string Message => "Trait not recognised: " + _trait;
        }

        public class CappedValueExceededBoundsException : Exception
        {
            public override string Message => "Capped value exceeded bounds";
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

            public override string Message => "GameObject name too general, found " + _occurences + " occurences of " + _name;
        }

        public class CannotGetGameObjectComponent : Exception
        {
            public override string Message => "GameObject references cannot be gathered from getcomponent() calls.";
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

            public override string Message => "Attempted to " + _direction + " resource " + _resourceName + " by " + _amount;
        }

        public class DefaultSelectableNotProvidedForMenu : Exception
        {
            private readonly string _name;

            public DefaultSelectableNotProvidedForMenu(string name)
            {
                _name = name;
            }

            public override string Message => "Attempted to navigate to menu '" + _name + "' but no default selectable was found";
        }

        public class InventoryItemNotStackableException : Exception
        {
            private readonly string _name;
            private readonly float _amount;

            public InventoryItemNotStackableException(string name, float amount)
            {
                _name = name;
                _amount = amount;
            }

            public override string Message => "Tried to change item " + _name + " quantity by " + _amount + " but item is unique.";
        }

        public class MoveItemToSameInventoryException : Exception
        {
            public override string Message => "Tried to move item from inventory to same inventory.";
        }

        public class ItemNotInInventoryException : Exception
        {
            private readonly string _itemName;

            public ItemNotInInventoryException(string itemName)
            {
                _itemName = itemName;
            }

            public override string Message => "Tried to remove item " + _itemName + " from an inventory that did not contain it.";
        }

        public class ResourceDoesNotExistException : Exception
        {
            private readonly string _resourceName;

            public ResourceDoesNotExistException(string resourceName)
            {
                _resourceName = resourceName;
            }

            public override string Message => "Tried to get resource " + _resourceName + " but resource does not exist.";
        }

        public class ResourceAlreadyExistsException : Exception
        {
            private readonly string _name;

            public ResourceAlreadyExistsException(string name)
            {
                _name = name;
            }

            public override string Message => "Resource " + _name + " already exists in inventory.";
        }

        public class MaxOrMinWeightExceededException : Exception
        {
            private readonly string _traitName, _name, _className;
            private readonly int _targetWeight;

            public MaxOrMinWeightExceededException(string name, int targetWeight, string characterClassName, string characterTraitName)
            {
                _name = name;
                _targetWeight = targetWeight;
                _className = characterClassName;
                _traitName = characterTraitName;
            }

            public override string Message => "Tried to assign weight " + _targetWeight + " to " + _name + " with class " + _className + " and trait " + _traitName;
        }

        public class UnknownStateNameException : Exception
        {
            private readonly string _stateName;

            public UnknownStateNameException(string stateName)
            {
                _stateName = stateName;
            }

            public override string Message => "Tried to navigate to unknown state '" + _stateName + "'.";
        }

        public class MultipleInstancesOfSingletonException : Exception
        {
            private readonly Type _type;

            public MultipleInstancesOfSingletonException(Type type)
            {
                _type = type;
            }

            public override string Message => "Tried to create instance of " + _type + " but instance already exists.";
        }

        public class AttributeContainerAlreadyContainsAttributeException : Exception
        {
            private readonly AttributeType _type;

            public AttributeContainerAlreadyContainsAttributeException(AttributeType type)
            {
                _type = type;
            }

            public override string Message => "Tried to add existing attribute to container " + _type;
        }

        public class CannotAddItemTypeToInventoryException : Exception
        {
            private readonly GameObjectType _gameObjectType;

            public CannotAddItemTypeToInventoryException(GameObjectType gameObjectType)
            {
                _gameObjectType = gameObjectType;
            }

            public override string Message => "Tried to add " + _gameObjectType + " to inventory, but no UI has been created to hold it";
        }

        public class InvalidInventoryItemException : Exception
        {
            private readonly string _desiredType;
            private readonly MyGameObject _item;

            public InvalidInventoryItemException(MyGameObject item, string desiredType)
            {
                _item = item;
                _desiredType = desiredType;
            }

            public override string Message => "Tried to add " + _item.Name + " with type " + _item.GetType() + " to inventory but only " + _desiredType + " can be added to this inventory.";
        }

        public class ThresholdValueNotReachableException : Exception
        {
            private readonly string _name;
            private readonly float _thresholdValue, _min, _max;

            public ThresholdValueNotReachableException(string thresholdName, float thresholdValue, float min, float max)
            {
                _name = thresholdName;
                _thresholdValue = thresholdValue;
                _min = min;
                _max = max;
            }

            public override string Message => "Tried to add threshold '" + _name + "' at value '" + _thresholdValue + "' exceeds min-max range (" + _min + "-" + _max + ")";
        }

        public class FiredWithNoAmmoException : Exception
        {
            public override string Message => "Tried to fire but weapon had no ammo on firing.";
        }

        public class ProbabalisticMachineHasNoDefaultStateException : Exception
        {
            public override string Message => "Tried to use method \"ReturnToDefault\" when ProbabalisticStateMachine has no default state.";
        }

        public class DefaultStateNotSpecifiedException : Exception
        {
            public override string Message => "Default state not set.";
        }
    }
}