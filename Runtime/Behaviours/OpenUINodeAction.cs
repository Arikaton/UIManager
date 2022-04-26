using UIManagement.Core;
using UnityEngine;

namespace UIManagement.Behaviours
{
    public class OpenUINodeAction : MonoBehaviour
    {
        [SerializeField] private UINodeId nodeId;

        public void Invoke()
        {
            // UIManager.Instance.ShowViewNode(_nodeId);
        }
    }
}