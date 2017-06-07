using UnityEngine.UI;
using UnityEngine;

public class CharacterSelect : BorderHighlight {
	private Selectable firstSelectInCharacter, thisSelectable;
	private bool selected;

	public void Start(){
		thisSelectable = GetComponent<Selectable>();
		firstSelectInCharacter = transform.Find("Name Container").GetComponent<Selectable>();
	}

	public void SelectCharacter(){
		firstSelectInCharacter.Select();
		selected = true;
	}

	public void Update(){
		if(selected && Input.GetAxis("Cancel") != 0){
			thisSelectable.Select();
			selected = false;
		}
	}
}
