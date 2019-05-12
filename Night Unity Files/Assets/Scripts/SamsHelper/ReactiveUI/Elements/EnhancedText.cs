﻿using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace SamsHelper.ReactiveUI.Elements
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class EnhancedText : MonoBehaviour
	{
		public enum FontSizes
		{
			Small,
			Medium,
			Large,
			Title,
			Custom
		}

		private bool            _initialised;
		private TextMeshProUGUI _text;
		public  float           CustomFontSize;

		public FontSizes FontSize;

		public bool UseUppercase;

		public void Awake()
		{
			Initialise();
		}

		private void Initialise()
		{
			if (_initialised) return;
			_initialised = true;
			_text        = GetComponent<TextMeshProUGUI>();
			Assert.IsNotNull(_text);
			_text.raycastTarget = false;
			_text.richText      = true;
			_text.extraPadding  = true;
			_text.font          = UiAppearanceController.GetFont();
			_text.fontSize      = FontSize == FontSizes.Custom ? CustomFontSize : FontSize.ToSize();
		}

		public void SetText(string text)
		{
			Initialise();
			if (_text == null) _text = GetComponent<TextMeshProUGUI>();
			if (UseUppercase) text   = text.ToUpper();
			_text.text = text;
		}

		public void SetColor(Color color)
		{
			Initialise();
			_text.color = color;
			_text.ForceMeshUpdate(true);
		}

		public TextMeshProUGUI GetText() => _text;
	}
}