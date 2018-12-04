using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Global;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class ResourcesUiController : MonoBehaviour
{
    private static CanvasGroup _canvas;
    private static bool _hidden;
    private static GameObject _resourcePrefab;
    private ResourceTab _food, _water;
    private readonly Dictionary<string, ResourceTab> _resourceTabs = new Dictionary<string, ResourceTab>();

    public void Awake()
    {
        _canvas = GetComponent<CanvasGroup>();
        _resourcePrefab = Resources.Load<GameObject>("Prefabs/Resource");
        _food = gameObject.FindChildWithName<ResourceTab>("Food");
        _water = gameObject.FindChildWithName<ResourceTab>("Water");
        Populate();
    }

    public static void Show()
    {
        _hidden = false;
        _canvas.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
    }

    public static void Hide()
    {
        _hidden = true;
        _canvas.DOFade(0f, 1f).SetUpdate(UpdateType.Normal, true);
    }

    public static bool Hidden()
    {
        return _hidden;
    }

    public void UpdateResource(string resourceName, int quantity)
    {
        _resourceTabs[resourceName].UpdateTab(resourceName, quantity);
    }

    private void UpdateFood()
    {
        int quantity = (int) Inventory.Consumables().Sum(c =>
        {
            ResourceTemplate template = c.Template;
            float hungerOffset = template.ResourceType == ResourceType.Meat ? template.EffectBonus : 0;
            return hungerOffset * c.Quantity();
        });
        _food.UpdateTab("Food", quantity);
    }

    private void UpdateWater()
    {
        int quantity = (int) Inventory.Consumables().Sum(c =>
        {
            ResourceTemplate template = c.Template;
            float thirstOffset = template.ResourceType == ResourceType.Water ? template.EffectBonus : 0; 
            return thirstOffset * c.Quantity();
        });
        _water.UpdateTab("Water", quantity);
    }

    private void Populate()
    {
        ResourceTemplate.AllResources.ForEach(r =>
        {
            if (r.ResourceType != ResourceType.Resource) return;
            GameObject resourceTab = Instantiate(_resourcePrefab);
            resourceTab.transform.SetParent(transform);
            resourceTab.transform.SetAsLastSibling();
            resourceTab.transform.localScale = Vector2.one;
            Vector3 position = resourceTab.transform.position;
            position.z = 0;
            resourceTab.transform.position = position;
            _resourceTabs.Add(r.Name, resourceTab.GetComponent<ResourceTab>());
        });
    }

    public void Update()
    {
        UpdateFood();
        UpdateWater();
        foreach (ResourceTemplate resourceTemplate in ResourceTemplate.AllResources)
        {
            if (resourceTemplate.ResourceType != ResourceType.Resource) continue;
            int quantity = Mathf.FloorToInt(Inventory.GetResourceQuantity(resourceTemplate.Name));
            UpdateResource(resourceTemplate.Name, quantity);
        }
    }
}