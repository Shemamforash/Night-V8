using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UI
{
	public class ManualScrollbar : MonoBehaviour
	{
		private readonly bool       _selected = false;
		private          float      _scrollRectYPos;
		public           ScrollRect Scrollrect;

		public void Update()
		{
			if (_selected)
			{
				float scrollAmount = Input.GetAxis("Scroll Keyboard");
				_scrollRectYPos = Scrollrect.verticalNormalizedPosition;
				if (scrollAmount < 0)
				{
					_scrollRectYPos += 0.02f;
					if (_scrollRectYPos > 1) _scrollRectYPos = 1;
					Scrollrect.verticalNormalizedPosition = _scrollRectYPos;
				}
				else if (scrollAmount > 0)
				{
					_scrollRectYPos -= 0.02f;
					if (_scrollRectYPos < 0) _scrollRectYPos = 0;
					Scrollrect.verticalNormalizedPosition = _scrollRectYPos;
				}
			}
		}
	}
}