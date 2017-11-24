using System.Collections.Generic;
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

    public void SetValue(int energy)
    {
        Assert.IsTrue(energy >= 0);
        _energyTicks.ForEach(Destroy);
        for (int i = 0; i < energy; ++i)
        {
            CreateEnergyTick();
        }
    }

    private void CreateEnergyTick()
    {
        GameObject energyTick = new GameObject();
        energyTick.AddComponent<RectTransform>();
        energyTick.transform.SetParent(_energyContainer);
        energyTick.transform.localScale = Vector3.one;
        energyTick.AddComponent<Image>();
        LayoutElement layout = energyTick.AddComponent<LayoutElement>();
        layout.preferredWidth = 2;
        layout.preferredHeight = 10;
        _energyTicks.Add(energyTick);
    }
}
