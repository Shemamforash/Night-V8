using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBrandDisplayController : MonoBehaviour
{
    private TextMeshProUGUI _brandText;
    private Image _divider;

    private void Awake()
    {
        _brandText = GetComponent<TextMeshProUGUI>();
        _divider = transform.parent.FindChildWithName<Image>("Divider");
        _divider.SetAlpha(0f);
    }

    public void Update()
    {
        if (PlayerCombat.Instance == null) return;
        Player player = PlayerCombat.Instance.Player;
        List<Brand> brands = player.BrandManager.GetActiveBrands().FindAll(b => b != null);
        if (brands.Count == 0)
        {
            _brandText.text = "";
            _divider.SetAlpha(0f);
            return;
        }

        string[] progressString = new string[brands.Count];
        for (int i = 0; i < brands.Count; i++)
        {
            Brand brand = brands[i];
            progressString[i] = brand.GetProgressString();
        }

        string brandString = String.Join("\n", progressString);
        _brandText.text = brandString;
        _divider.SetAlpha(0.4f);
    }
}