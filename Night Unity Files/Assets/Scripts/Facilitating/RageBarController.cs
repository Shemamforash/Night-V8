using System;
using System.Collections.Generic;
using Game.Combat;
using SamsHelper;
using UnityEngine;
using UnityEngine.UI;

public class RageBarController : MonoBehaviour
{
    private static float _barFillAmount;
    private static ParticleSystem _rageFire;

    // Use this for initialization
    public void Start()
    {
        _rageFire = GameObject.Find("Rage Fire").GetComponent<ParticleSystem>();
        SetRageBarFill(0f, false);
    }

    public static void SetRageBarFill(float value, bool rageActive)
    {
        Color targetColor = new Color(1, 1, 1, 1);
        if (rageActive && !_rageFire.isPlaying)
        {
            _rageFire.Play();
        }
        else if(_rageFire.isPlaying && !rageActive)
        {
            _rageFire.Stop();
        }
        int barsFilled = (int) Math.Floor(value / _barFillAmount);
//        for (int i = 0; i < _bars.Count; ++i)
//        {
//            float alpha = 0f;
//            if (i < barsFilled)
//            {
//                alpha = 1;
//            }
//            else if (i == barsFilled)
//            {
//                float remainder = value - barsFilled * _barFillAmount;
//                alpha = remainder / _barFillAmount;
//            }
//            targetColor.a = alpha;
//            _bars[i].color = targetColor;
//        }
    }
}