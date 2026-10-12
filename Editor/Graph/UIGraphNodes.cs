using System;
using UIManagement.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIManagement.Graph
{
    public abstract class UIGraphNode : Node
    {
        private readonly Label _info;

        public abstract string Id { get; }
        public abstract bool IsView { get; }
        public event Action<UIGraphNode> DoubleClicked;

        protected UIGraphNode(string id, string className)
        {
            title = id;
            AddToClassList("ui-graph-node");
            AddToClassList(className);
            _info = new Label();
            _info.AddToClassList("ui-graph-node__info");
            extensionContainer.Add(_info);
            RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        public Vector2 Position => new Vector2(style.left.value.value, style.top.value.value);

        public void SetInfo(string text)
        {
            _info.text = text;
            _info.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
            RefreshExpandedState();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0 && evt.clickCount == 2)
            {
                DoubleClicked?.Invoke(this);
                evt.StopPropagation();
            }
        }
    }

    public class ScreenNodeElement : UIGraphNode
    {
        public readonly UINode Data;
        public readonly Port Output;

        public override string Id => Data.Id;
        public override bool IsView => false;

        public ScreenNodeElement(UINode data) : base(data.Id, "ui-screen-node")
        {
            Data = data;
            tooltip = "UI Node: opening it shows every connected view and hides all other views.";
            Output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(string));
            Output.portName = "Shows";
            outputContainer.Add(Output);
            RefreshPorts();
            RefreshExpandedState();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (Data != null)
                Selection.activeObject = Data;
        }
    }

    public class ViewNodeElement : UIGraphNode
    {
        private readonly string _viewId;
        public readonly Port Input;

        public override string Id => _viewId;
        public override bool IsView => true;
        public UIView SceneView { get; set; }

        public ViewNodeElement(string viewId) : base(viewId, "ui-view-node")
        {
            _viewId = viewId;
            Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(string));
            Input.portName = "Shown by";
            inputContainer.Add(Input);
            RefreshPorts();
            RefreshExpandedState();
        }

        public override void OnSelected()
        {
            base.OnSelected();
            if (SceneView != null)
                Selection.activeGameObject = SceneView.gameObject;
        }
    }
}
