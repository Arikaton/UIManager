using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIManagement.Core
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private NodesCollection nodesCollection;
        private Dictionary<string, UIView> _views = new Dictionary<string, UIView>();
        private Dictionary<string, UINode> _nodes = new Dictionary<string, UINode>(10);
        private HashSet<string> _activeViews = new HashSet<string>();
        private Stack<UIView> _popupStack = new Stack<UIView>();

        public string ActiveNode { get; private set; }
        public bool HasActivePopup => _popupStack.Count > 0;

        public override void AwakeInternal()
        {
            foreach (var node in nodesCollection.UINodes)
            {
                _nodes[node.Id] = node;
            }
        }

        public void Register(UIView view)
        {
            if (!_views.ContainsKey(view.ViewId))
            {
                //Debug.Log($"Register {view.ViewId} view");
                _views.Add(view.ViewId, view);
            }
        }

        public void Unregister(UIView view)
        {
            if (_views.ContainsKey(view.ViewId))
            {
                _views.Remove(view.ViewId);
            }
        }
        
        public void ShowViewNode(string nodeId, bool hidePopups = false)
        {
            if (nodesCollection.UINodes.All(x => x.Id != nodeId))
            {
                throw new InvalidOperationException(
                    $"Attempting to access node with id '{nodeId}' which does not exist in selected node collection (collection id '{nodesCollection.Id}'");
            }
            
            //Debug.Log($"Show {nodeId} UINode");
            if (hidePopups)
                HideAllPopups();
            
            ActiveNode = nodeId;
            var node = _nodes[nodeId];
            _activeViews.ExceptWith(node.ViewIds);
            foreach (var viewId in _activeViews)
            {
                //Debug.Log($"Hide {viewId} view");
                _views[viewId].Hide();
            }
            _activeViews.Clear();
            
            foreach (var viewId in node.ViewIds)
            {
                var view = _views[viewId];
                view.Show();
                _activeViews.Add(view.ViewId);
                //Debug.Log($"Show {view.ViewId} view");
            }
        }
        public void ShowPopup(string popupId)
        {
            var popup = _views[popupId];
            if(_popupStack.Count == 0 || _popupStack.Peek() != popup.GetUIView())
                _popupStack.Push(popup.GetUIView());
            popup.Show();
        }

        public void HideLastPopup()
        {
            if (_popupStack.Count == 0)
                return;
            var popup = _popupStack.Pop();
            popup.Hide();
        }

        public void HideAllPopups()
        {
            while (_popupStack.Count > 0)
            {
                var popup = _popupStack.Pop();
                popup.Hide();
            }
        }
    }
}