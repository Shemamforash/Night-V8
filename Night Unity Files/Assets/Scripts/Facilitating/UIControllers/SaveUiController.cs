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
        private EnhancedButton _button;

        public void Awake()
        {
            _realTime = gameObject.FindChildWithName<EnhancedText>("Real Time");
            _gameInfo = gameObject.FindChildWithName<EnhancedText>("Game Info");
            _button = GetComponent<EnhancedButton>();
        }

        public void SetSave(Save save)
        {
            _save = save;
            if (!_save.Valid())
            {
                GetComponent<CanvasGroup>().alpha = 0.3f;
                Button b = _button.Button();
                Destroy(_button);
                Destroy(b);
                Destroy(gameObject.FindChildWithName("Border(Clone)"));
            }

            if (_save.IsMostRecent()) _button.Select();

            _realTime.SetText(_save.GetRealTime());
            _gameInfo.SetText(_save.GetGameInfo());
        }

        public void Load()
        {
            _save.LoadFromSave();
        }
    }
}