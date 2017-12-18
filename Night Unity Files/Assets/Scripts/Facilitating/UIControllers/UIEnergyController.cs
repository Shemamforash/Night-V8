using System.Collections.Generic;
using SamsHelper;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class UIEnergyController : MonoBehaviour
{
    private Transform _energyContainer;
    private readonly List<GameObject> _energyTicks = new List<GameObject>();

    public void Awake()
    {
        _energyContainer = transform.Find("Progress");
    }

    public void SetValue(int energy, int energyMax)
    {
        Assert.IsTrue(energy >= 0);
        _energyTicks.ForEach(Destroy);
        for (int i = 0; i < energyMax; ++i)
        {
            CreateEnergyTick(i < energy);
        }
    }

    private void CreateEnergyTick(bool energyAvailable)
    {
        GameObject energyTick = Helper.InstantiateUiObject("Prefabs/AttributeMarkerPrefab", _energyContainer);
//        energyTick.AddComponent<RectTransform>();
//        energyTick.transform.SetParent(_energyContainer);
//        energyTick.transform.localScale = Vector3.one;
//        energyTick.AddComponent<Image>();
//        LayoutElement layout = energyTick.AddComponent<LayoutElement>();
//        layout.preferredWidth = 2;
//        layout.preferredHeight = 10;
        if (!energyAvailable)
        {
            energyTick.GetComponent<Image>().color = new Color(1, 1, 1, 0.4f);
        }
        _energyTicks.Add(energyTick);
    }
}