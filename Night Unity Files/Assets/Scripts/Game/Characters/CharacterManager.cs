using System.Collections;
using System.Collections.Generic;
using Facilitating.Persistence;
using Facilitating.UI.GameOnly;
using Game.Characters;
using UnityEngine;
using World;
using Persistence;
using UnityEngine.UI;
using UI.GameOnly;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        private static List<Character> characters = new List<Character>();
        private TimeListener timeListener = new TimeListener();
		private PersistenceListener persistenceListener;

		private void Awake(){
			persistenceListener = new PersistenceListener(Load, Save, "Character Manager");
		    timeListener.OnHour(UpdateCharacterThirstAndHunger);
		}

        private void UpdateCharacterThirstAndHunger()
        {
            foreach (Character c in characters)
            {
                c.Dehydration.Value += c.Thirst / 12f;
                c.Starvation.Value += c.Hunger / 12f;
            }
        }

        private void Load()
        {
            Traits.LoadTraits();
            ClassCharacter.LoadCharacterClasses();
            if (GameData.Party != null)
            {
                characters = GameData.Party;
            }
            else
            {
                characters = CharacterGenerator.LoadInitialParty();
            }
            PopulateCharacterUI();
        }

		private void Save(){
			GameData.Party = characters;
		}

        private void PopulateCharacterUI()
        {
            float currentY = 1f;
            for (int i = 0; i < characters.Count; ++i)
            {
                GameObject newCharacterUI = characters[i].CharacterUi.GameObject;
                RectTransform uiRect = newCharacterUI.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, currentY - 0.1f);
                uiRect.anchorMax = new Vector2(1, currentY);
                currentY -= 0.1f;
                newCharacterUI.transform.localScale = new Vector2(1, 1);
                Button b = characters[i].GetCharacterUi().SimpleViewButton;
                b.onClick.AddListener(delegate
                {
                    GetComponent<CharacterSelect>().SelectCharacter(b);
                });
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                Button b = characters[i].GetCharacterUi().SimpleViewButton;
                Navigation n = b.navigation;
                if (i == 0)
                {
                    if (i + 1 < characters.Count)
                    {
                        n.selectOnDown = characters[i + 1].GetCharacterUi().SimpleViewButton;
                    }
                }
                else if (i == characters.Count - 1)
                {
                    n.selectOnUp = characters[i - 1].GetCharacterUi().SimpleViewButton;
                }
                else
                {
                    n.selectOnUp = characters[i - 1].GetCharacterUi().SimpleViewButton;
                    n.selectOnDown = characters[i + 1].GetCharacterUi().SimpleViewButton;
                }
                b.navigation = n;
            }
        }

        public static Character FindCharacterFromGameObject(GameObject g)
        {
            foreach (Character c in characters)
            {
                if (c.GetCharacterUi().GameObject == g)
                {
                    return c;
                }
            }
            return null;
        }
        
        private static void ChangeCharacterPanel(GameObject g, bool expand)
        {
            bool foundCharacter = false;
            float moveAmount = 0.3f;
            if (expand)
            {
                moveAmount = -moveAmount;
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                if (foundCharacter)
                {
                    RectTransform rect = characters[i].GetCharacterUi().GameObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (characters[i].GetCharacterUi().GameObject == g)
                {
                    foundCharacter = true;
                    if (expand)
                    {
                        characters[i].GetCharacterUi().EatButton.Select();
                    }
                }
            }
        }

        public static void ExpandCharacter(GameObject g)
        {
            ChangeCharacterPanel(g, true);
        }

        public static void CollapseCharacter(GameObject g)
        {
            ChangeCharacterPanel(g, false);
        }
    }

}
