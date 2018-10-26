using System.Collections.Generic;
using Game.Characters;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterBrandUIController : MonoBehaviour
{
    private BrandUI _brand1, _brand2, _brand3;

    public void Awake()
    {
        _brand1 = new BrandUI(gameObject.FindChildWithName("Brand 1"));
        _brand2 = new BrandUI(gameObject.FindChildWithName("Brand 2"));
        _brand3 = new BrandUI(gameObject.FindChildWithName("Brand 3"));
    }

    public void UpdateBrands(BrandManager brandManager)
    {
        List<Brand> activeBrands = brandManager.GetActiveBrands();
        _brand1.SetBrand(activeBrands[0]);
        _brand2.SetBrand(activeBrands[1]);
        _brand3.SetBrand(activeBrands[2]);
    }

    private class BrandUI
    {
        private readonly TextMeshProUGUI _text;
        private readonly Image _left, _right;

        public BrandUI(GameObject parent)
        {
            _text = parent.FindChildWithName<TextMeshProUGUI>("Text");
            _left = parent.FindChildWithName<Image>("Left");
            _right = parent.FindChildWithName<Image>("Right");
        }

        public void SetBrand(Brand brand)
        {
            if (brand == null)
            {
                _left.color = UiAppearanceController.InvisibleColour;
                _right.color = UiAppearanceController.InvisibleColour;
                _text.color = UiAppearanceController.FadedColour;
                _text.text = "No Brand";
                return;
            }

            _left.color = Color.white;
            _right.color = Color.white;
            _text.color = Color.white;
            _text.text = brand.GetProgressString();
        }
    }
}