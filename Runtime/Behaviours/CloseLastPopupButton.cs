using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Behaviours
{
    [RequireComponent(typeof(Button))]
    public class CloseLastPopupButton : MonoBehaviour
    {
        private Button _button;
        
        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HideLastPopup);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(HideLastPopup);
        }

        private void HideLastPopup()
        {
            UIManager.Instance.HideLastPopup();
        }
    }
}