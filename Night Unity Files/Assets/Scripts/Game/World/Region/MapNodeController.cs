using System.Collections;
using System.Collections.Generic;
using SamsHelper;
using TMPro;
using UnityEngine;

public class MapNodeController : MonoBehaviour
{
    private TextMeshProUGUI _fadeText;
    private const float LetterFadeInDuration = 0.5f;
    private float _currentTime;
    private int _currentLetter;
    private string _completeWord;
    private readonly List<Letter> _letters = new List<Letter>();
    private bool _doneFading;

    private Transform _ring1, _ring2, _ring3;

    public void SetName(string name)
    {
        _fadeText = Helper.FindChildWithName<TextMeshProUGUI>(gameObject, "Fade");
        for (int i = 0; i < name.Length; ++i)
        {
            _letters.Add(new Letter(name[i].ToString()));
            if (i > 0)
            {
                _letters[i - 1].SetNextLetter(_letters[i]);
            }
        }

        _ring1 = Helper.FindChildWithName(gameObject, "Ring 1").transform;
        _ring2 = Helper.FindChildWithName(gameObject, "Ring 2").transform;
        _ring3 = Helper.FindChildWithName(gameObject, "Ring 3").transform;

        _letters[0]?.StartFade();
        StartCoroutine(FadeInLetters());
    }

    public void Update()
    {
        _ring1.Rotate(new Vector3(0, 0, 1), 5 * Time.deltaTime);
        _ring2.Rotate(new Vector3(0, 0, 1), 3 * Time.deltaTime);
        _ring3.Rotate(new Vector3(0, 0, 1), -4 * Time.deltaTime);
    }

    private IEnumerator FadeInLetters()
    {
        while (_doneFading == false)
        {
            _doneFading = true;
            _completeWord = "";
            _letters.ForEach(l => l.Update(this));
            _fadeText.text = _completeWord;
            yield return null;
        }
    }

    private class Letter
    {
        private readonly string _letter;
        private float _age;
        private Letter _nextLetter;
        private bool _fading;

        public Letter(string letter)
        {
            _letter = letter;
        }

        public void SetNextLetter(Letter nextLetter)
        {
            _nextLetter = nextLetter;
        }

        public void StartFade()
        {
            _fading = true;
        }

        public void Update(MapNodeController fadeIn)
        {
            string letterWithAlpha = "";
            if (_fading)
            {
                if (_age > LetterFadeInDuration / 4)
                {
                    _nextLetter?.StartFade();
                }

                if (_age > LetterFadeInDuration)
                {
                    letterWithAlpha = _letter;
                }
                else
                {
                    float alpha = _age / LetterFadeInDuration;
                    _age += Time.deltaTime;
                    letterWithAlpha += LetterToHex(alpha);
                    fadeIn._doneFading = false;
                }
            }
            else
            {
                letterWithAlpha = LetterToHex(0f);
            }

            fadeIn._completeWord += letterWithAlpha;
        }

        private string LetterToHex(float alpha)
        {
            string hexString = "<color=#" + ColorUtility.ToHtmlStringRGBA(new Color(1, 1, 1, alpha)) + ">";
            return hexString + _letter + "</color>";
        }
    }
}