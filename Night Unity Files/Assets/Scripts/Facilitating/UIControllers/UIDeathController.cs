using System;
using Facilitating.UIControllers;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class UIDeathController : MonoBehaviour
{
    public static DeathReason DeathReason;
    private static readonly string[] _thirstDeaths = {"The sun sets on the brittle bones of your parched body"};
    private static readonly string[] _hungerDeaths = {"Hungry predators encircle your emaciated remains"};
    private static readonly string[] _fireDeaths = {"The wind carries the ash of your burnt body into the wilderness beyond"};
    private static readonly string[] _voidDeaths = {"Even the lowest of scavengers avoid your cursed corpse"};
    private static readonly string[] _standardDeaths = {"You watch as the blood of your wounds soaks into the thirsty earth"};

    public void Awake()
    {
        GetComponent<EnhancedText>().SetText(GetDeathReasonText());
    }

    private string GetDeathReasonText()
    {
        string[] deathReasons;
        switch (DeathReason)
        {
            case DeathReason.Thirst:
                deathReasons = _thirstDeaths;
                break;
            case DeathReason.Hunger:
                deathReasons = _hungerDeaths;
                break;
            case DeathReason.Fire:
                deathReasons = _fireDeaths;
                break;
            case DeathReason.Void:
                deathReasons = _voidDeaths;
                break;
            case DeathReason.Standard:
                deathReasons = _standardDeaths;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return deathReasons.RandomElement();
    }
}