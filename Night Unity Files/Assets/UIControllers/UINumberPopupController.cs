using System.Collections;
using Facilitating.UI.Elements;
using UnityEngine;

public class UINumberPopupController : MonoBehaviour
{
	private EnhancedText _enhancedText;
	private Color _criticalColor = Color.red;
	private float _alpha = 1f;
	private float _duration = 1f;
	private float _speed = 0.2f;
	
	public void Awake ()
	{
		_enhancedText = GetComponent<EnhancedText>();
		_duration = Random.RandomRange(0.25f, 0.35f);
		_speed = Random.Range(3f, 5f);
	}

	public void ShowValue(int value, bool critical)
	{
		Vector3 position = transform.position;
		position.x += Random.Range(-0.3f, 0.3f);
		transform.position = position;
		string valueString = value.ToString();
		if (value == 0)
		{
			valueString = "miss";
		}
		_enhancedText.Text(valueString);
		_enhancedText.SetColor(critical ? _criticalColor : Color.white);
//		StartCoroutine(MoveText());
	}

	private IEnumerator MoveText()
	{
		while (_alpha > 0)
		{
			_enhancedText.SetAlpha(_alpha);
			_alpha -= Time.deltaTime / _duration;
			Vector3 position = transform.position;
			position.y += Time.deltaTime * _speed;
			transform.position = position;
			yield return null;
		}
		Destroy(gameObject);
	}
}
