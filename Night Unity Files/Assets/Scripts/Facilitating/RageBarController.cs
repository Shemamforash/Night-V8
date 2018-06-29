using System;
using System.Collections;
using System.Collections.Generic;
using SamsHelper.Libraries;
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

        public void Awake()
        {
            RoseProngs = new List<Image>();
            _instance = this;
            _dashFlash = Helper.FindChildWithName<Image>(gameObject, "Ready");
            _dashRing = Helper.FindChildWithName<Image>(gameObject, "Ring");
        }

        public void Start()
        {
            for (int i = 1; i < 17; ++i) RoseProngs.Add(Helper.FindChildWithName<Image>(gameObject, i.ToString()));
            SetRageBarFill(0f);
        }

        public static void SetRageBarFill(float value)
        {
            if (_instance == null) return;
            int completeProngs = (int) Math.Floor(value / 0.0625f);
            float remainder = value - completeProngs * 0.0625f;
            float normalisedRemainder = remainder / 0.0625f;
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
    }
}