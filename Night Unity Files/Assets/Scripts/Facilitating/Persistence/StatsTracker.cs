﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.Persistence
{
    public class StatsTracker : MonoBehaviour
    {
        private static float sessionTime;
        private TextMeshProUGUI _statsText;

        private void Awake()
        {
            _statsText = GetComponent<TextMeshProUGUI>();
        }
        
        private void Update()
        {
            sessionTime += Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(sessionTime);
            string statString = "Session Play Time: " + t.Hours + "hours " + t.Minutes + "mins " + t.Seconds + "secs";
            statString += "\nDeaths: 0";
            statString += "\nWeapons found: 0";
            statString += "\nEnemies eaten: 0";
            statString += "\nSurvivors lost: 0";
            statString += "\nBullets wasted: 0";
            statString += "\nRemnants found: 0";
            statString += "\nTears spilled: 0";
            statString += "\nKilometres travelled: 0";
            _statsText.text = statString;
        }
    }
}