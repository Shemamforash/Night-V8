using System;
using DG.Tweening;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear.Weapons;
using Game.Global;
using Extensions;
using Extensions;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class UiBrandMenu : Menu
{
	private static UiBrandMenu           _instance;
	private static string                _titleString, _benefitString, _quoteString, _overviewString;
	private        bool                  _canHide;
	private        CloseButtonController _closeButton;
	private        CanvasGroup           _detailCanvas, _overviewCanvas;
	private        Menu                  _lastMenu;
	private        EnhancedText          _titleText, _benefitText, _quoteText, _overviewText;
	private        float                 _waitToHideTime;

	protected override void Awake()
	{
		base.Awake();
		_detailCanvas   = gameObject.FindChildWithName<CanvasGroup>("Detail");
		_overviewCanvas = gameObject.FindChildWithName<CanvasGroup>("Overview");
		_overviewText   = _overviewCanvas.GetComponent<EnhancedText>();
		_titleText      = gameObject.FindChildWithName<EnhancedText>("Title");
		_benefitText    = gameObject.FindChildWithName<EnhancedText>("Benefit");
		_quoteText      = gameObject.FindChildWithName<EnhancedText>("Quote");
		_instance       = this;
		_closeButton    = gameObject.FindChildWithName<CloseButtonController>("Close Button");
	}

	private void Hide()
	{
		if (!_canHide) return;
		if (_closeButton != null) _closeButton.Disable();
		string lastMenu = _lastMenu != null ? _lastMenu.name : "HUD";
		MenuStateMachine.ShowMenu(lastMenu);
		WorldState.Resume();
	}

	private void Show(bool showDetailOnly = false)
	{
		_lastMenu = MenuStateMachine.CurrentMenu();
		MenuStateMachine.ShowMenu("Brand Menu");
		WorldState.Pause();
		_closeButton.Enable();
		_closeButton.UseSpaceInput();
		if (showDetailOnly)
		{
			ShowDetail();
			return;
		}

		ShowOverview();
	}

	private void ShowOverview()
	{
		_overviewText.SetText(_overviewString);
		_overviewCanvas.alpha = 1;
		_detailCanvas.alpha   = 0;
		_closeButton.SetOnClick(ShowDetail);
	}

	private void ShowDetail()
	{
		_titleText.SetText(_titleString);
		_benefitText.SetText(_benefitString);
		_quoteText.SetText(_quoteString);
		_overviewCanvas.alpha = 0;
		_detailCanvas.alpha   = 1;
		_canHide              = false;
		Sequence sequence = DOTween.Sequence();
		sequence.SetUpdate(true);
		sequence.AppendInterval(0.25f);
		sequence.AppendCallback(() => _canHide = true);
		_closeButton.SetOnClick(Hide);
	}

	public static void ShowBrand(Brand brand)
	{
		switch (brand.Status)
		{
			case BrandStatus.Failed:
				_titleString   = "Failed";
				_benefitString = "You feel your body go weak";
				_quoteString   = "All attributes reduced to 1";
				break;
			case BrandStatus.Succeeded:
				_titleString   = "Passed";
				_benefitString = "You feel a warmth bloom inside you";
				_quoteString   = brand.GetEffectText();
				break;
		}

		_overviewString = "Rite "                + _titleString;
		_titleString    = _titleString + " The " + brand.GetDisplayName();
		_instance.Show();
	}

	public static void ShowCharacterSkill(Skill skill, int skillNum)
	{
		_overviewString = skill.Name;
		_titleString    = skill.Name;
		_benefitString  = skill.Description;
		_quoteString    = GetSkillUsageString(skill, skillNum);
		_instance.Show(true);
	}

	private static string GetSkillUsageString(Skill skill, int skillNum)
	{
		string       binding      = GetBindingForSkill(skillNum);
		string       requirements = skill.Cooldown  + " Charge";
		string       firstLine    = binding + " - " + requirements;
		const string secondLine   = "\n<size=16><color=#aaaaaa>Gain Charge by damaging enemies</color></size>";
		return firstLine + secondLine;
	}

	private static string GetBindingForSkill(int skillNum)
	{
		string keyName;
		switch (skillNum)
		{
			case 0:
				keyName = InputHandler.GetBindingForKey(InputAxis.SkillOne);
				break;
			case 1:
				keyName = InputHandler.GetBindingForKey(InputAxis.SkillTwo);
				break;
			case 2:
				keyName = InputHandler.GetBindingForKey(InputAxis.SkillThree);
				break;
			case 3:
				keyName = InputHandler.GetBindingForKey(InputAxis.SkillFour);
				break;
			default:
				throw new ArgumentOutOfRangeException("Invalid skill number passed " + skillNum);
		}

		return "Press [" + keyName + "] to use";
	}
}