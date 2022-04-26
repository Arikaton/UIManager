using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Behaviours
{
    [RequireComponent(typeof(Button))]
    public class OpenPopupButton : MonoBehaviour
    {
        [SerializeField] private UIViewId popupId;
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ShowPopup);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(ShowPopup);
        }

        private void ShowPopup()
        {
            UIManager.Instance.ShowPopup(popupId.ViewId);
        }
    }
}