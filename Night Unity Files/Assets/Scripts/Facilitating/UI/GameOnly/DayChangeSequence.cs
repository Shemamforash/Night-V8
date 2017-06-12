using UnityEngine;
using System.Collections;
using World;

namespace UI.GameOnly
{
    using Audio;
    public class DayChangeSequence : MonoBehaviour
    {
        public float timeToRead = 3f;
        public float timeBetweenLines = 1f;
        public float fadeTime = 2f;
        private CanvasGroup menuScreen, thisCanvasGroup;
        private ThunderClick thunderClick;
        private TimeListener timeListener = new TimeListener();

        public void Awake()
        {
            thisCanvasGroup = GetComponent<CanvasGroup>();
            menuScreen = GameObject.Find("Game Menu").GetComponent<CanvasGroup>();
            thunderClick = GetComponent<ThunderClick>();
            timeListener.OnDay(ChangeDay);
        }

        public void ChangeDay()
        {
            WorldTime.Pause();
            menuScreen.interactable = false;
            menuScreen.alpha = 0;
            thisCanvasGroup.interactable = true;
            thisCanvasGroup.alpha = 1;
            thunderClick.InitiateThunder();
            StartCoroutine("ShowLines");
        }

        private IEnumerator WaitToReadText()
        {
            float currentTime = 0;
            while (currentTime < timeToRead)
            {
                currentTime += Time.deltaTime;
                yield return null;
            }
            TransitionEnd();
        }

        private void TransitionEnd()
        {
            thunderClick.InitiateThunder();
            menuScreen.alpha = 1;
            thisCanvasGroup.alpha = 0;
            WorldTime.UnPause();
            menuScreen.interactable = true;
            thisCanvasGroup.interactable = false;
        }

        private IEnumerator ShowLines()
        {
            float currentTime = 0f;
            int currentChild = 0;
            for (int i = 0; i < transform.childCount; ++i)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
            while (currentTime < timeBetweenLines)
            {
                currentTime += Time.deltaTime;
                if (currentTime > timeBetweenLines && currentChild < transform.childCount)
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