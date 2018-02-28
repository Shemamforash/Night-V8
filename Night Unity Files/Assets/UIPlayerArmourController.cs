using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.Gear.Armour;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerArmourController : MonoBehaviour
{
    private readonly List<Image> _plates = new List<Image>();
    private readonly List<Image> _joiners = new List<Image>();
    private EnhancedText _ratingText, _platesText;
    private GameObject _equippedObject, _notEquippedObject;

    public void Awake()
    {
        _equippedObject = Helper.FindChildWithName(gameObject, "Equipped");
        _notEquippedObject = Helper.FindChildWithName(gameObject, "Not Equipped");
        for (int i = 9; i >= 0; --i)
        {
            _plates.Add(Helper.FindChildWithName<Image>(gameObject, "Plate " + i));
            if (i < 9) _joiners.Add(Helper.FindChildWithName<Image>(gameObject, "Joiner " + i));
        }

        _ratingText = Helper.FindChildWithName<EnhancedText>(gameObject, "Rating");
        _platesText = Helper.FindChildWithName<EnhancedText>(gameObject, "Plates");
    }
    
    public void SetArmour(ArmourController armour)
    {
        if (armour.GetProtectionLevel() == 0)
        {
            _notEquippedObject.SetActive(true);
            _equippedObject.SetActive(false);
            return;
        }
        _notEquippedObject.SetActive(false);
        _equippedObject.SetActive(true);
        for (int i = 0; i < _plates.Count; ++i)
        {
            if (i >= armour.GetProtectionLevel())
            {
                _plates[i].color = new Color(1, 1, 1, 0f);
                if (i - 1 >= 0) _joiners[i - 1].color = new Color(1, 1, 1, 0f);
            }
            else
            {
                _plates[i].color = Color.white;
            }
        }

        _ratingText.Text("Max Armour: " + armour.GetMaxArmour());
        _platesText.Text(armour.GetProtectionLevel() + " Armour");


//        foreach (ArmourPlate armourPlate in armour.GetPlates())
//        {
//            int weight = (int) armourPlate.Weight;
//            for (int i = 0; i < weight; ++i)
//            {
//                _plates[i].color = Color.white;
//                if (i < weight - 1) _joiners[i].color = new Color(1, 1, 1, 0.4f);
//            }
//        }
    }
}