using System.Collections.Generic;
using Game.Characters;
using Extensions;
using TMPro;
using UnityEngine;

public class CharacterBrandUIController : MonoBehaviour
{
	private TextMeshProUGUI _brand1, _brand2, _brand3;

	public void Awake()
	{
		_brand1 = gameObject.FindChildWithName("Brand 1").FindChildWithName<TextMeshProUGUI>("Text");
		_brand2 = gameObject.FindChildWithName("Brand 2").FindChildWithName<TextMeshProUGUI>("Text");
		_brand3 = gameObject.FindChildWithName("Brand 3").FindChildWithName<TextMeshProUGUI>("Text");
	}

	public void UpdateBrands(BrandManager brandManager)
	{
		List<Brand> activeBrands = brandManager.GetActiveBrands();
		UpdateBrand(activeBrands[0], _brand1);
		UpdateBrand(activeBrands[1], _brand2);
		UpdateBrand(activeBrands[2], _brand3);
	}

	private void UpdateBrand(Brand brand, TextMeshProUGUI text)
	{
		if (brand == null)
		{
			text.text = "";
			return;
		}

		float progress = brand.NormalisedProgress();
		text.color = Color.Lerp(new Color(1, 1, 1, 0.4f), Color.white, progress);
		text.text  = brand.GetProgressString();
	}
}