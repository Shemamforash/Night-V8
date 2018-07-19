using System;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

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

        public class AttributeNotRecognisedException : Exception
        {
            private readonly string _attribute;

            public AttributeNotRecognisedException(string attribute)
            {
                _attribute = attribute;
            }

            public override string Message => "Attribute not recognised: " + _attribute;
        }

        public class UnknownCharacterClassException : Exception
        {
            private readonly string _trait;

            public UnknownCharacterClassException(string trait)
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
            private readonly string _name;
            private readonly int _occurences;

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
            private readonly float _amount;
            private readonly string _direction;
            private readonly string _resourceName;

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
            private readonly float _amount;
            private readonly string _name;

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
            private readonly int _targetWeight;
            private readonly string _traitName, _name, _className;

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

        public class ChildNotFoundException : Exception
        {
            private readonly GameObject _parent;
            private readonly string _target;

            public ChildNotFoundException(GameObject parent, string target)
            {
                _parent = parent;
                _target = target;
            }

            public override string Message => "Could not find child '" + _target + "' under '" + Helper.PrintHierarchy(_parent) + "'.";
        }

        public class ComponentNotFoundException : Exception
        {
            private readonly string _parent, _componentType;

            public ComponentNotFoundException(string parent, Type componentType)
            {
                _parent = parent;
                _componentType = componentType.ToString();
            }

            public override string Message => "'" + _parent + "' does not have component '" + _componentType + "' but you are trying to access it.";
        }

        public class SkillSlotOutOfRangeException : Exception
        {
            private readonly int _slot, _noSlots;

            public SkillSlotOutOfRangeException(int slot, int noSlots)
            {
                _slot = slot;
                _noSlots = noSlots;
            }

            public override string Message => "Tried to assign skill to slot " + _slot + " in skill bar of size " + _noSlots + ".";
        }

        public class TryRemoveItemDoesNotExistException : Exception
        {
            private readonly string _itemName, _menuListName;

            public TryRemoveItemDoesNotExistException(MyGameObject item, GameObject menuList)
            {
                _itemName = item.Name;
                _menuListName = menuList.name;
            }

            public override string Message => "Tried to remove item '" + _itemName + "' from MenuList '" + _menuListName + "' but it does not exist.";
        }

        public class ItemAlreadyExistsInMenuListException : Exception
        {
            private readonly string _itemName, _menuListName;

            public ItemAlreadyExistsInMenuListException(MyGameObject item, GameObject menuList)
            {
                _itemName = item.Name;
                _menuListName = menuList.name;
            }

            public override string Message => "Tried to add item '" + _itemName + "' to MenuList '" + _menuListName + "' but it has already been added.";
        }

        public class MoreAmmoConsumedThanAvailableException : Exception
        {
            public override string Message => "Tried to consume more ammo than was available in the current magazine.";
        }

        public class UnknownRegionTypeException : Exception
        {
            private readonly string _type;

            public UnknownRegionTypeException(string type)
            {
                _type = type;
            }

            public override string Message => "Unknown region type '" + _type + "'";
        }

        public class EnemyTypeDoesNotExistException : Exception
        {
            private readonly string _type;

            public EnemyTypeDoesNotExistException(string type)
            {
                _type = type;
            }

            public override string Message => "Enemy type does not exist '" + _type + "'";
        }

        public class SkillDoesNotExistException : Exception
        {
            private readonly string _type;

            public SkillDoesNotExistException(string type)
            {
                _type = type;
            }

            public override string Message => "Skill does not exist '" + _type + "'";
        }

        public class InscriptionModificationException : Exception
        {
            private readonly string _name;

            public InscriptionModificationException(string name)
            {
                _name = name;
            }

            public override string Message => "Inscription had too many modification values, should have at most two '" + _name + "'";
        }

        public class StateDoesNotExistException : Exception
        {
            private readonly string _stateName;

            public StateDoesNotExistException(string stateName)
            {
                _stateName = stateName;
            }

            public override string Message => "State '" + _stateName + "' does not exist.";
        }

        public class XmlNodeDoesNotExistException : Exception
        {
            private readonly string _nodeName;

            public XmlNodeDoesNotExistException(string nodeName)
            {
                _nodeName = nodeName;
            }

            public override string Message => "Node " + _nodeName + " does not exist";
        }

        public class NodeHasNoAttributesException : Exception
        {
            private readonly string _nodeName;

            public NodeHasNoAttributesException(string nodeName)
            {
                _nodeName = nodeName;
            }

            public override string Message => "Node " + _nodeName + " has no attributes";
        }

        public class NoNodesWithNameException : Exception
        {
            private readonly string _nodeName;

            public NoNodesWithNameException(string nodeName)
            {
                _nodeName = nodeName;
            }

            public override string Message => "No nodes with name " + _nodeName + " found";
        }
    }
}