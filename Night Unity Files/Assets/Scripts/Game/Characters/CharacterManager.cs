using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using World;
using Persistence;
using UnityEngine.UI;
using UI.GameOnly;
using UnityEngine.EventSystems;

namespace Characters
{
    public class CharacterManager : MonoBehaviour
    {
        private static List<Character> characters = new List<Character>();
        private TimeListener timeListener = new TimeListener();
        public GameObject driverObject;
        public GameObject characterPrefab;

        public void Awake()
        {
            Traits.LoadTraits();
            ClassCharacter.LoadCharacterClasses();
            if (Settings.party != null)
            {
                characters = Settings.party;
            }
            else
            {
                characters = CharacterGenerator.LoadInitialParty();
            }
            characters[0].SetCharacterUI(driverObject);
            PopulateCharacterUI();
        }

        private void PopulateCharacterUI()
        {
            float yMax = 0.9f, yMin = 0.8f;
            for (int i = 1; i < characters.Count; ++i)
            {
                GameObject newCharacterUI = Instantiate(characterPrefab);
                newCharacterUI.transform.SetParent(transform);
                characters[i].SetCharacterUI(newCharacterUI);
                RectTransform uiRect = newCharacterUI.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, yMin);
                uiRect.anchorMax = new Vector2(1, yMax);
                yMax -= 0.1f;
                yMin -= 0.1f;
                Button b = characters[i].GetCharacterUI().simpleViewButton;
                b.onClick.AddListener(delegate
                {
                    GetComponent<CharacterSelect>().SelectCharacter(b);
                });
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                Button b = characters[i].GetCharacterUI().simpleViewButton;
                Navigation n = b.navigation;
                if (i == 0)
                {
                    if (i + 1 < characters.Count)
                    {
                        n.selectOnDown = characters[i + 1].GetCharacterUI().simpleViewButton;
                    }
                }
                else if (i == characters.Count - 1)
                {
                    n.selectOnUp = characters[i - 1].GetCharacterUI().simpleViewButton;
                }
                else
                {
                    n.selectOnUp = characters[i - 1].GetCharacterUI().simpleViewButton;
                    n.selectOnDown = characters[i + 1].GetCharacterUI().simpleViewButton;
                }
                b.navigation = n;
            }
        }

        private static Character FindCharacterFromGameObject(GameObject g)
        {
            foreach (Character c in characters)
            {
                if (c.GetCharacterUI().gameObject == g)
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
                    RectTransform rect = characters[i].GetCharacterUI().gameObject.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (characters[i].GetCharacterUI().gameObject == g)
                {
                    foundCharacter = true;
                    if (expand)
                    {
                        characters[i].GetCharacterUI().eatButton.Select();
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
