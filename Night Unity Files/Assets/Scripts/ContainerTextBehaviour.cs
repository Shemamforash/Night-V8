﻿using Game.Combat.Player;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

public class ContainerTextBehaviour : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private const float MaxShowInventoryDistance = 1;

    public void Awake()
    {
        _text = gameObject.FindChildWithName<TextMeshProUGUI>("Text");
    }

    public void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Position());
        _text.color = UiAppearanceController.InvisibleColour;
        transform.rotation = PlayerCombat.Instance.transform.rotation;
        if (distanceToPlayer > MaxShowInventoryDistance) return;
        _text.color = Color.white;
    }
}