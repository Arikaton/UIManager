using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Behaviours
{
    public class OpenPopupAction : MonoBehaviour
    {
        [SerializeField] private UIViewId popupId;

        public void OpenPopup()
        {
            UIManager.Instance.ShowPopup(popupId.ViewId);
        }
    }
}