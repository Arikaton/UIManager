using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Behaviours
{
    public class CloseLastPopupAction : MonoBehaviour
    {
        public void Invoke()
        {
            UIManager.Instance.HideLastPopup();
        }
    }
}