using UIManagement.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManagement.Behaviours
{
    [RequireComponent(typeof(Button))]
    public class OpenUINodeButton : MonoBehaviour
    {
        [SerializeField] private UINodeId nodeId;
        
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OpenUINode);
        }
        
        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OpenUINode);
        }
        
        private void OpenUINode()
        {
            UIManager.Instance.ShowViewNode(nodeId.NodeId);
        }
    }
}