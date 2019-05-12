using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Flicker : MonoBehaviour
{
	private Image _image;
	public  float FlickerRate = 1;
	public  Color From, To;

	public void Awake()
	{
		_image = GetComponent<Image>();
	}

	public void Update()
	{
		float val = Mathf.PerlinNoise(Time.timeSinceLevelLoad * FlickerRate, 0);
		_image.color = Color.Lerp(From, To, val);
	}
}