using System.Collections.Generic;
using Extensions;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Facilitating.UI
{
	public class Highlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
	{
		/*Base class for all interactive objects.
		If it's interactive, it must have it's colours inverted when selected, and must have a tooltip.
		*/

		protected List<TextMeshProUGUI> childTexts = new List<TextMeshProUGUI>();

		public virtual void OnDeselect(BaseEventData eventData)
		{
			Fade();
		}

		public void OnPointerEnter(PointerEventData p)
		{
			ChangeTextColour(Color.white);
		}

		public void OnPointerExit(PointerEventData p)
		{
			Fade();
		}

		public virtual void OnSelect(BaseEventData eventData)
		{
			ChangeTextColour(Color.white);
		}

		public virtual void Awake()
		{
			List<Transform> children = transform.FindAllChildren();
			foreach (Transform t in children)
			{
				TextMeshProUGUI text = t.GetComponent<TextMeshProUGUI>();
				if (text != null) childTexts.Add(text);
			}
		}

		private void Fade()
		{
			ChangeTextColour(UiAppearanceController.FadedColour);
		}

		private void ChangeTextColour(Color c)
		{
			foreach (TextMeshProUGUI buttonText in childTexts) buttonText.color = c;
		}

		public void OnDisable()
		{
			ChangeTextColour(Color.white);
		}
	}
}