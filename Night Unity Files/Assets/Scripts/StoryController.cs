using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Global;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

public class StoryController : MonoBehaviour
{

	private static string _text =
		"I have come to you, wanderer, in this dark time. I know what it is you seek, and I can show you the way. The spell has been cast, but your fate is not certain yet. I will give you fair warning, then you must decide for yourself if what I offer is worth the price.\n" +
		"The tormented lands are the realm of the ancient  gods, the dead and the dying their only worshippers. Now sleeping and long abandonded by their followers, each have left their lands to fall into ruin. There are five ruined gates in this world, five portals to the tombs of the dead ones, hidden from view. They cast the veil between our world and theirs.\n" +
		"The journey to the gates is far, and each one you must enter. You see for me to give you what you seek, you must bring me something in return. We will make a deal, you will enter the gates of the gods, and you will return me their blackened souls. Across the twisted oasis with it's spoiled water and waning flora you will find the Gate Of The Black Water. Then to the steppe and the Gate of the Frozen Plains, endless fields wrapped in razored grass, you shal find no shelter there. Surviving this you must cross the valley of ruins, where timeless monuments watch over mortal souls, and where the Gate of the Ruined World stands impervious to the weathering of time. From the ruins  you will come to the great defiles, a natural labyrinth carved by rain over the centuries, deep within you will find the Gate of Penance. Then to the end you come, to the wastelands that stretch on to the end of time, to the Gate of the Black World, where I will wait for you.\n" +
		"Be wary, pilgrim, for as I have said these lands are transient and wicked. You see, there is a storm- the last trick of a cruel father. It sweeps away the unwary and the unwise, it comes for those who linger and wander without purpose. So dally not, even when hope seems lost, you must stay persistant. Do not start a journey you have no intention of finishing, Wanderer.\n" +
		"- The Necromancer";

	private const float _timePerWord = 0.2f;
	private Queue<string> _paragraphs;
	private TextMeshProUGUI _storyText;
	private static string _nextMenu;

	public void Awake()
	{
		_storyText = GetComponent<TextMeshProUGUI>();
	}
	
	public void Start ()
	{
		_paragraphs = new Queue<string>();
		foreach (string paragraph in _text.Split('\n'))
		{
			_paragraphs.Enqueue(paragraph);
		}

		StartCoroutine(DisplayParagraph());
	}

	public static void ShowText(string text, string nextMenu)
	{
		_text = text;
		_nextMenu = nextMenu;
		SceneChanger.ChangeScene("Story");
	}

	public static float GetTimeToRead(string paragraph)
	{
		int wordCount = paragraph.Split(' ').Length;
		float timeToRead = _timePerWord * wordCount;
		return timeToRead;
	}
	
	private IEnumerator DisplayParagraph()
	{
		if (_paragraphs.Count == 0)
		{
			SceneChanger.ChangeScene(_nextMenu);
			yield break;
		}
		string currentParagraph = _paragraphs.Dequeue();
		float timeToRead = GetTimeToRead(currentParagraph);
		_storyText.text = currentParagraph;
		_storyText.color = UiAppearanceController.InvisibleColour;
		Tween fade = _storyText.DOColor(Color.white, 3f);
		yield return fade.WaitForCompletion();
		while (timeToRead > 0)
		{
			timeToRead -= Time.deltaTime;
			yield return null;
		}

		fade = _storyText.DOColor(UiAppearanceController.InvisibleColour, 3f);
		yield return fade.WaitForCompletion();
		StartCoroutine(DisplayParagraph());
	}
}
