using System.Collections.Generic;
using Game.Combat.Generation;
using Game.Global;
using UnityEngine;

public class ContainerController : MonoBehaviour
{
    public DesolationInventory Inventory;
    private SpriteRenderer _renderer;
    private const int MinDistanceToFlash = 4;
    private float _currentFlashIntensity;
    private static GameObject _prefab;
    public static List<ContainerController> Containers = new List<ContainerController>();

    public static void CreateContainer(Vector2 position, DesolationInventory inventory)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Container");
        GameObject container = Instantiate(_prefab);
        container.transform.position = position;
        container.transform.localScale = Vector3.one * 0.03f;
        container.GetComponent<ContainerController>().Inventory = inventory;
    }
    
    public void Awake()
    {
        Containers.Add(this);
        _renderer = GetComponent<SpriteRenderer>();
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
