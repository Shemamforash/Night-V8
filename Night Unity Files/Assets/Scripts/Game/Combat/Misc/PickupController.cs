using System;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Misc
{
    public class PickupController : MonoBehaviour
    {
        private static readonly ObjectPool<PickupController> _pickupPool = new ObjectPool<PickupController>("Prefabs/Combat/Pickup");
        private InventoryResourceType _pickupType;
        private Rigidbody2D _rigidBody;
        private TextMeshProUGUI _text;

        public static void Create(Vector3 origin, InventoryResourceType resourceType)
        {
            PickupController pickup = _pickupPool.Create();
            pickup.Initialise(origin, resourceType);
        }

        private void Initialise(Vector3 origin, InventoryResourceType resourceType)
        {
            transform.position = origin;
            if (_rigidBody == null) _rigidBody = GetComponent<Rigidbody2D>();
            if (_text == null) _text = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Text");
            Vector3 direction = Quaternion.Euler(0, 0, Random.Range(0, 360)).eulerAngles.normalized;
            _rigidBody.AddForce(direction * Random.Range(20, 50));
            _pickupType = resourceType;
            UpdateText();
        }

        private void UpdateText()
        {
            string textString;
            switch (_pickupType)
            {
                case InventoryResourceType.Food:
                    textString = "F";
                    break;
                case InventoryResourceType.Water:
                    textString = "W";
                    break;
                case InventoryResourceType.Fuel:
                    textString = "C";
                    break;
                case InventoryResourceType.Scrap:
                    textString = "S";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _text.text = textString;
        }

        private void OnDestroy()
        {
            _pickupPool.Dispose(this);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            other.gameObject.GetComponent<PlayerCombat>().Player.Inventory().IncrementResource(_pickupType, 1);
            _pickupPool.Return(this);
        }
    }
}