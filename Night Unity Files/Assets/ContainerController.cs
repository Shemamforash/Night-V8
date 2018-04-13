using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

public class ContainerController : DesolationInventory 
{
    private const int MinDistanceToFlash = 4;
    private static GameObject _prefab;
    public static List<ContainerBehaviour> Containers = new List<ContainerBehaviour>();
    private Vector2 _position;

    public ContainerController(Vector2 position, DesolationInventory inventory = null) : base("Cache")
    {
        _position = position;
        inventory?.Contents().ForEach(i => Move(i, i.Quantity()));
    }
    
    public void CreateObject()
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Container");
        GameObject container = GameObject.Instantiate(_prefab);
        container.transform.position = _position;
        container.transform.localScale = Vector3.one * 0.03f;
        container.AddComponent<ContainerBehaviour>().SetContainerController(this);
    }

    public class ContainerBehaviour : MonoBehaviour
    {
        private float _currentFlashIntensity;
        private SpriteRenderer _renderer;
        public ContainerController ContainerController;

        public void Awake()
        {
            Containers.Add(this);
            _renderer = GetComponent<SpriteRenderer>();
        }

        public void SetContainerController(ContainerController containerController)
        {
            ContainerController = containerController;
        }
        
        private void OnDestroy()
        {
            Containers.Remove(this);
        }

        public void Update()
        {
            float distanceToPlayer = Vector2.Distance(transform.position, CombatManager.Player().transform.position);
            if (distanceToPlayer > MinDistanceToFlash)
            {
                _currentFlashIntensity = 0;
                return;
            }
            float normalisedDistance = 1 - distanceToPlayer / MinDistanceToFlash;
            Color c = _renderer.color;
            float intensityModifier = _currentFlashIntensity <= 1 ? _currentFlashIntensity : 1 - (_currentFlashIntensity - 1);
            c.a = intensityModifier * normalisedDistance;
            _renderer.color = c;
            _currentFlashIntensity += Time.deltaTime;
            if (_currentFlashIntensity > 2) _currentFlashIntensity = 0;
        }
    }
}
