using UnityEngine;
using UnityEngine.UI;
using System;

public class Resource {

	public enum ResourceType { Unknown, Nothing, Water, Food, Fuel, Ammo, Camp };
	private float _quantity = 0f;
	private readonly Text resourceText;
	private readonly Func<float, string> _unitConversion;

	public Resource(string name, Func<float, string> unitConversion){
		resourceText = GameObject.Find(name).transform.Find("Text").GetComponent<Text>();
		_unitConversion = unitConversion;
		Consume(0);
	}

	public float Consume(float amount){
		float difference = _quantity - amount;
		float actualConsumption = amount;
		if(difference < 0){
			_quantity = 0;
			actualConsumption = amount + difference;
		} else {
			_quantity -= amount;
		}
		resourceText.text = _unitConversion(_quantity);
		return actualConsumption;
	}

	public void Increment(float amount){
		_quantity += amount;
		resourceText.text = _unitConversion(_quantity);
	}

	public float Quantity() {
		return _quantity;
	}

	public GameObject GetObject()
	{
		return resourceText.transform.parent.gameObject;
	}
}
