using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	private Text buttonText;

	void Start () {
		buttonText = transform.FindChild("Text").GetComponent<Text>();
		Button btn = GetComponent<Button>();
	}
	
	public void OnPointerEnter(PointerEventData eventData){
		buttonText.color = Color.black;
	}

	public void OnPointerExit(PointerEventData eventData){
		buttonText.color = Color.white;
	}
}
