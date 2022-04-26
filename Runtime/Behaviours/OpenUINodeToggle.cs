using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Behaviours
{
    [RequireComponent(typeof(Toggle))]
    public class OpenUINodeToggle : MonoBehaviour
    {
        [SerializeField] private UINodeId nodeId;
        
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
            UIManager.Instance.ShowViewNode(nodeId.NodeId);
        }
    }
}