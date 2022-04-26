using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Behaviours
{
    [RequireComponent(typeof(Toggle))]
    public class OpenPopupToggle : MonoBehaviour
    {
        [SerializeField] private UIViewId popupId;

        private Toggle _toggle;

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(ChangeValue);
        }

        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveListener(ChangeValue);
        }

        private void ChangeValue(bool value)
        {
            if(!value) return;
            UIManager.Instance.ShowPopup(popupId.ViewId);
        }
    }
}