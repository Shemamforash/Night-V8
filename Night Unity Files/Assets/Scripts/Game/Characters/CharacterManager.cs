using System.Collections;
using System.Collections.Generic;
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
        public GameObject driverObject;
        public GameObject characterPrefab;

        public void Awake()
        {
            Traits.LoadTraits();
            CharacterClass.LoadCharacterClasses();
            if (Settings.party != null)
            {
                characters = Settings.party;
            }
            else
            {
                characters = CharacterGenerator.LoadInitialParty();
            }
            characters[0].characterUI = driverObject;
            PopulateCharacterUI();
        }

        private void PopulateCharacterUI()
        {
            float yMax = 0.9f, yMin = 0.8f;
            for (int i = 1; i < characters.Count; ++i)
            {
                GameObject newCharacterUI = Instantiate(characterPrefab);
                characters[i].characterUI = newCharacterUI;
                newCharacterUI.transform.SetParent(transform);
                RectTransform uiRect = newCharacterUI.GetComponent<RectTransform>();
                uiRect.offsetMin = new Vector2(5, 5);
                uiRect.offsetMax = new Vector2(-5, -5);
                uiRect.anchorMin = new Vector2(0, yMin);
                uiRect.anchorMax = new Vector2(1, yMax);
                yMax -= 0.1f;
                yMin -= 0.1f;
                Button b = newCharacterUI.transform.Find("Simple").GetComponent<Button>();
                b.onClick.AddListener(delegate
                {
                    GetComponent<CharacterSelect>().SelectCharacter(b);
                });
            }
            for (int i = 0; i < characters.Count; ++i)
            {
                Button b = characters[i].characterUI.transform.Find("Simple").GetComponent<Button>();
                Navigation n = b.navigation;

                if (i == 0)
                {
                    if (i + 1 < characters.Count)
                    {
                        n.selectOnDown = characters[i + 1].characterUI.GetComponent<Button>();
                    }
                }
                else if (i == characters.Count - 1)
                {
                    n.selectOnUp = characters[i - 1].characterUI.GetComponent<Button>();
                }
                else
                {
                    n.selectOnUp = characters[i - 1].characterUI.GetComponent<Button>();
                    n.selectOnDown = characters[i + 1].characterUI.GetComponent<Button>();
                }

            }
        }

        private static Character FindCharacterFromGameObject(GameObject g)
        {
            foreach (Character c in characters)
            {
                if (c.characterUI == g)
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
                    RectTransform rect = characters[i].characterUI.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(rect.anchorMin.x, rect.anchorMin.y + moveAmount);
                    rect.anchorMax = new Vector2(rect.anchorMax.x, rect.anchorMax.y + moveAmount);
                }
                else if (characters[i].characterUI == g)
                {
                    foundCharacter = true;
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
