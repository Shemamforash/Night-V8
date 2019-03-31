using System;
using System.Collections;
using System.Collections.Generic;
using SamsHelper.Libraries;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating
{
    public class RageBarController : MonoBehaviour
    {
        private static float _barFillAmount;
        private static List<Image> RoseProngs;
        private static Image _dashFlash;
        private static RageBarController _instance;
        private static float _currentTime;
        private static readonly float _dashFlashTime = 1f;
        private static Image _dashRing;
        private static RectTransform _adrenalineRect;

        public void Awake()
        {
            RoseProngs = new List<Image>();
            _instance = this;
            _dashFlash = gameObject.FindChildWithName<Image>("Ready");
            _dashRing = gameObject.FindChildWithName<Image>("Ring");
            _adrenalineRect = GetComponent<RectTransform>();
        }

        public void Start()
        {
            GameObject activeObject = gameObject.FindChildWithName("Active");
            for (int i = 1; i < 9; ++i) RoseProngs.Add(activeObject.FindChildWithName<Image>(i.ToString()));
            SetRageBarFill(0f);
        }

        public static void SetRageBarFill(float value)
        {
            if (_instance == null) return;
            float frac = 1 / 8f;
            int completeProngs = (int) Math.Floor(value / frac);
            float remainder = value - completeProngs * frac;
            float normalisedRemainder = remainder / frac;
            for (int i = 0; i < RoseProngs.Count; ++i)
            {
                float alpha = 0f;
                if (i < completeProngs)
                    alpha = 1f;
                else if (i == completeProngs)
                    alpha = normalisedRemainder;

                RoseProngs[i].color = new Color(1, 1, 1, alpha);
            }
        }

        public static void UpdateDashTimer(float amount)
        {
            _dashRing.fillAmount = amount;
        }

        public static void PlayFlash()
        {
            _currentTime = _dashFlashTime;
            _instance.StartCoroutine(_instance.Flash());
        }

        private IEnumerator Flash()
        {
            while (_currentTime > 0)
            {
                float normalisedTime = _currentTime / _dashFlashTime;
                normalisedTime *= 0.8f;
                _dashFlash.color = new Color(1, 1, 1, normalisedTime);
                _currentTime -= Time.deltaTime;
                yield return null;
            }
        }

        public static RectTransform AdrenalineRect() => _adrenalineRect;
    }
}