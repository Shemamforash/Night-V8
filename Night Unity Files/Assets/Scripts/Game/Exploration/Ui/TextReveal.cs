using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Exploration.Ui
{
    public class TextReveal : MonoBehaviour
    {
        private const float LetterFadeInDuration = 0.5f;
        private readonly List<Letter> _letters = new List<Letter>();
        private bool _doneFading;
        private string _completeWord;
        private int _currentLetter;
        private Action<string> _onStringChange;
        private float _nextLetterStartRatio = 4;

        public float LetterStartRatio
        {
            set => _nextLetterStartRatio = value;
        }

        public Coroutine Reveal(string text, Action<string> onStringChange)
        {
            _onStringChange = onStringChange;
            for (int i = 0; i < text.Length; ++i)
            {
                _letters.Add(new Letter(text[i].ToString(), this));
                if (i > 0) _letters[i - 1].SetNextLetter(_letters[i]);
            }

            _letters[0]?.StartFade();
            return StartCoroutine(DoReveal());
        }

        private IEnumerator DoReveal()
        {
            while (_doneFading == false)
            {
                _doneFading = true;
                _completeWord = "";
                _letters.ForEach(l => l.Update());
                _onStringChange(_completeWord);
                yield return null;
            }
        } 

        private class Letter
        {
            private readonly string _letter;
            private float _age;
            private bool _fading;
            private Letter _nextLetter;
            private readonly TextReveal _textReveal;

            public Letter(string letter, TextReveal textReveal)
            {
                _letter = letter;
                _textReveal = textReveal;
            }

            public void SetNextLetter(Letter nextLetter)
            {
                _nextLetter = nextLetter;
            }

            public void StartFade()
            {
                _fading = true;
            }

            public void Update()
            {
                string letterWithAlpha = "";
                if (_fading)
                {
                    if (_age > LetterFadeInDuration / _textReveal._nextLetterStartRatio) _nextLetter?.StartFade();

                    if (_age > LetterFadeInDuration)
                    {
                        letterWithAlpha = _letter;
                    }
                    else
                    {
                        float alpha = _age / LetterFadeInDuration;
                        _age += Time.deltaTime;
                        letterWithAlpha += LetterToHex(alpha);
                        _textReveal._doneFading = false;
                    }
                }
                else
                {
                    letterWithAlpha = LetterToHex(0f);
                }

                _textReveal._completeWord += letterWithAlpha;
            }

            private string LetterToHex(float alpha)
            {
                string hexString = "<color=#FFFFFF" + ((int) (alpha * 255)).ToString("X2") + ">" + _letter + "</color>";
                return hexString;
            }
        }
    }
}