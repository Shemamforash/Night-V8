using System.Collections;
using Audio;
using Game.World;
using UnityEngine;
using World;

namespace Facilitating.UI.GameOnly
{
    public class DayChangeSequence : MonoBehaviour
    {
        public float TimeToRead = 3f;
        public float TimeBetweenLines = 1f;
        public float FadeTime = 2f;
        private CanvasGroup _menuScreen, _thisCanvasGroup;
        private ThunderClick _thunderClick;
        private readonly TimeListener _timeListener = new TimeListener();

        public void Awake()
        {
            _thisCanvasGroup = GetComponent<CanvasGroup>();
            _menuScreen = GameObject.Find("Game Menu").GetComponent<CanvasGroup>();
            _thunderClick = GetComponent<ThunderClick>();
            _timeListener.OnDay(ChangeDay);
        }

        public void ChangeDay()
        {
            WorldTime.Pause();
            _menuScreen.interactable = false;
            _menuScreen.alpha = 0;
            _thisCanvasGroup.interactable = true;
            _thisCanvasGroup.alpha = 1;
            _thunderClick.InitiateThunder();
            StartCoroutine("ShowLines");
        }

        private IEnumerator WaitToReadText()
        {
            float currentTime = 0;
            while (currentTime < TimeToRead)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            TransitionEnd();
        }

        private void TransitionEnd()
        {
            _thunderClick.InitiateThunder();
            _menuScreen.alpha = 1;
            _thisCanvasGroup.alpha = 0;
            WorldTime.UnPause();
            _menuScreen.interactable = true;
            _thisCanvasGroup.interactable = false;
        }

        private IEnumerator ShowLines()
        {
            float currentTime = 0f;
            int currentChild = 0;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            while (currentTime < TimeBetweenLines)
            {
                currentTime += Time.deltaTime;
                if (currentTime > TimeBetweenLines && currentChild < transform.childCount)
                {
                    transform.GetChild(currentChild).gameObject.SetActive(true);
                    ++currentChild;
                    currentTime = 0f;
                }
                yield return null;
            }
            yield return StartCoroutine("WaitToReadText");
        }
    }
}