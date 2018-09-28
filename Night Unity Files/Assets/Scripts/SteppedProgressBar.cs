﻿using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SteppedProgressBar : MonoBehaviour
{
//    private static readonly List<Fader> _faderPool = new List<Fader>();
    private RectTransform _rect;
    private Image _slider;
    private RectTransform _sliderRect;

    public void Awake()
    {
        _slider = GetComponent<Image>();
        _rect = GetComponent<RectTransform>();
        ResetValue();
    }

    public void ResetValue(float value = 1f)
    {
        _slider.fillAmount = 1;
    }

    public void SetValue(float newValue)
    {
        float oldValue = _slider.fillAmount;
        if (oldValue == newValue) return;
        _slider.fillAmount = newValue;
        Fader fader = CreateNewFadeBlock();
        RectTransform faderTransform = fader.GetComponent<RectTransform>();
        if (_slider.fillOrigin == (int) Image.OriginHorizontal.Right)
        {
            float temp = oldValue;
            oldValue = 1 - newValue;
            newValue = 1 - temp;
        }

        faderTransform.anchorMin = new Vector2(newValue, 0);
        faderTransform.anchorMax = new Vector2(oldValue, 1);
        faderTransform.offsetMin = Vector2.zero;
        faderTransform.offsetMax = Vector2.zero;
        fader.Restart();
    }

    private Fader CreateNewFadeBlock()
    {
        Fader fader;
        GameObject faderObject;
//        if (_faderPool.Count == 0)
//        {
        faderObject = new GameObject();
        faderObject.name = "Fader";
        faderObject.transform.SetParent(_rect, false);
        faderObject.AddComponent<Image>();
        fader = faderObject.AddComponent<Fader>();
//        }
//        else
//        {
//            fader = _faderPool[0];
//            _faderPool.RemoveAt(0);
//            faderObject = fader.gameObject;
//            faderObject.SetActive(true);
//        }

        faderObject.transform.SetSiblingIndex(1);
        return fader;
    }

    private class Fader : MonoBehaviour
    {
        private const float Duration = 0.5f;
        private Image _faderImage;

        public void Awake()
        {
            _faderImage = GetComponent<Image>();
            Sequence seq = DOTween.Sequence();
            _faderImage.color = Color.white;
            seq.Append(_faderImage.DOColor(new Color(0, 0, 0, 0), Duration));
            seq.AppendCallback(() =>
            {
                Destroy(gameObject);
//                gameObject.SetActive(false);
//                _faderPool.Add(this);
            });
        }

        private void OnDestroy()
        {
//            _faderPool.Remove(this);
        }

        public void Restart()
        {
//            Sequence seq = DOTween.Sequence();
//            _faderImage.color = Color.white;
//            seq.Append(_faderImage.DOColor(new Color(0, 0, 0, 0), Duration));
//            seq.AppendCallback(() =>
//            {
//                Destroy(gameObject);
////                gameObject.SetActive(false);
////                _faderPool.Add(this);
//            });
        }
    }
}