using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating
{
	public class RageBarController : MonoBehaviour
	{
		private static          float             _barFillAmount;
		private static          Image             _dashFlash, _dashIcon;
		private static          RageBarController _instance;
		private static          float             _currentTime;
		private static readonly float             _dashFlashTime = 1f;
		private static          Image             _dashRing;

		public void Awake()
		{
			_instance       = this;
			_dashFlash      = gameObject.FindChildWithName<Image>("Ready");
			_dashIcon       = gameObject.FindChildWithName<Image>("Icon");
			_dashRing       = gameObject.FindChildWithName<Image>("Ring");
		}

		public static void UpdateDashTimer(float amount)
		{
			_dashIcon.SetAlpha(0.6f);
			_dashRing.fillAmount = amount;
		}

		public static void PlayFlash()
		{
			_currentTime = _dashFlashTime;
			_instance.StartCoroutine(_instance.Flash());
			_dashIcon.SetAlpha(1f);
		}

		private IEnumerator Flash()
		{
			while (_currentTime > 0)
			{
				float normalisedTime = _currentTime / _dashFlashTime;
				normalisedTime *= 0.8f;
				_dashFlash.SetAlpha(normalisedTime);
				_currentTime -= Time.deltaTime;
				yield return null;
			}
		}
	}
}