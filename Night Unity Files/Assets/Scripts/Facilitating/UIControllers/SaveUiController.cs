using Facilitating.Persistence;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class SaveUiController : MonoBehaviour
    {
        private Save _save;
        private EnhancedText _realTime, _gameInfo;

        public void Awake()
        {
            _realTime = gameObject.FindChildWithName<EnhancedText>("Real Time");
            _gameInfo = gameObject.FindChildWithName<EnhancedText>("Game Info");
        }

        public void SetSave(Save save)
        {
            _save = save;
            if (!_save.Valid())
            {
                GetComponent<CanvasGroup>().alpha = 0.3f;
                EnhancedButton enhancedButton = GetComponent<EnhancedButton>();
                if (enhancedButton == null) gameObject.PrintHierarchy();
                Button button = enhancedButton.Button();
                Destroy(enhancedButton);
                Destroy(button);
                Destroy(gameObject.FindChildWithName("Border(Clone)"));
            }

            _realTime.SetText(_save.GetRealTime());
            _gameInfo.SetText(_save.GetGameInfo());
        }

        public void Load()
        {
            _save.LoadFromSave();
        }
    }
}