using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Player;
using Game.Exploration.Regions;
using Extensions;
using SamsHelper.Libraries;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBrandDisplayController : MonoBehaviour
{
	private TextMeshProUGUI _brandText;
	private Image           _divider;

	private void Awake()
	{
		_brandText = GetComponent<TextMeshProUGUI>();
		_divider   = transform.parent.FindChildWithName<Image>("Divider");
		_divider.SetAlpha(0f);
	}

	public void Update()
	{
		if (PlayerCombat.Instance == null) return;
		RegionType regionType = CharacterManager.CurrentRegion().GetRegionType();
		if (regionType == RegionType.Rite || regionType == RegionType.Tomb || regionType == RegionType.Gate)
		{
			gameObject.SetActive(false);
			Destroy(this);
			return;
		}

		Player      player = PlayerCombat.Instance.Player;
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

		string brandString = string.Join("\n", progressString);
		_brandText.text = brandString;
		_divider.SetAlpha(0.4f);
	}
}