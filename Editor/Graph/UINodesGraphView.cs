using System;
using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIManagement.Graph
{
    public class UINodesGraphView : GraphView
    {
        private const float ScreenColumnX = 0f;
        private const float ViewColumnX = 360f;
        private const float ScreenSpacing = 90f;
        private const float ViewSpacing = 80f;

        private readonly Func<Vector2, Rect> _toScreenRect;
        private readonly List<ScreenNodeElement> _screens = new List<ScreenNodeElement>();
        private readonly List<ViewNodeElement> _views = new List<ViewNodeElement>();
        private NodesCollection _collection;
        private bool _rebuilding;
        private bool _layoutIncomplete;
        private string _signature;

        public NodesCollection Collection => _collection;

        public UINodesGraphView(Func<Vector2, Rect> toScreenRect)
        {
            _toScreenRect = toScreenRect;
            SetupZoom(0.2f, 2f);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            var styleSheet = UINodesGraphStyles.Load();
            if (styleSheet != null)
                styleSheets.Add(styleSheet);

            graphViewChanged = OnGraphViewChanged;
        }

        public void SetCollection(NodesCollection collection, bool frame)
        {
            _collection = collection;
            Rebuild();
            if (frame)
                schedule.Execute(() => FrameAll()).ExecuteLater(50);
        }

        public void Rebuild()
        {
            _rebuilding = true;
            DeleteElements(graphElements.ToList());
            _screens.Clear();
            _views.Clear();
            if (_collection != null)
                Populate();
            _rebuilding = false;
            _signature = ComputeSignature();
            RefreshStates();
        }

        public bool RebuildIfChangedExternally()
        {
            if (ComputeSignature() == _signature)
                return false;
            Rebuild();
            return true;
        }

        public void AutoLayout()
        {
            if (_collection == null)
                return;
            LayoutAll();
            SavePositions();
            schedule.Execute(() => FrameAll()).ExecuteLater(50);
        }

        public Vector2 VisibleCenter()
        {
            return contentViewContainer.WorldToLocal(worldBound.center);
        }

        public void PrepareForStructuralChange()
        {
            if (_layoutIncomplete)
                SavePositions();
        }

        public void RefreshStates()
        {
            if (_collection == null)
                return;
            var sceneViews = FindSceneViews();
            foreach (var view in _views)
            {
                sceneViews.TryGetValue(view.Id, out var sceneView);
                view.SceneView = sceneView;
                var declared = _collection.UIViews.Contains(view.Id);
                var usedBy = NodesCollectionEditing.NodesUsing(_collection, view.Id).Count();
                var isPopup = sceneView != null && sceneView.IsPopup;

                view.EnableInClassList("missing", !declared);
                view.EnableInClassList("popup", isPopup);
                view.EnableInClassList("unused", declared && usedBy == 0 && !isPopup);

                var info = new List<string>();
                if (!declared)
                    info.Add("Not declared in the collection");
                if (isPopup)
                    info.Add("Popup");
                else if (declared && usedBy == 0)
                    info.Add("Not shown by any node");
                if (sceneView == null)
                    info.Add("No UIView in open scenes");
                view.SetInfo(string.Join("\n", info));
                view.tooltip = sceneView != null
                    ? $"UI View '{view.Id}' on '{sceneView.name}'. Click to select it in the scene."
                    : $"UI View '{view.Id}'. Open the scene that contains it to link it.";
            }
            RefreshRuntimeState();
        }

        public void RefreshRuntimeState()
        {
            var manager = Application.isPlaying ? UIManager.Instance : null;
            var activeNode = manager != null ? manager.ActiveNode : null;
            foreach (var screen in _screens)
            {
                var active = screen.Id == activeNode;
                screen.EnableInClassList("active", active);
                screen.SetInfo(active ? "Active" : null);
            }
            foreach (var view in _views)
            {
                var shown = Application.isPlaying && view.SceneView != null &&
                            (view.SceneView.State == UIViewState.Shown || view.SceneView.State == UIViewState.Showing);
                view.EnableInClassList("shown", shown);
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()
                .Where(x => x.direction != startPort.direction && x.node.GetType() != startPort.node.GetType())
                .ToList();
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_collection == null)
                return;
            var target = evt.target as VisualElement;
            var graphPosition = contentViewContainer.WorldToLocal(evt.mousePosition);
            var screenRect = _toScreenRect(evt.mousePosition);

            var screen = target?.GetFirstOfType<ScreenNodeElement>();
            if (screen != null)
            {
                BuildScreenMenu(evt.menu, screen, screenRect);
                return;
            }

            var view = target?.GetFirstOfType<ViewNodeElement>();
            if (view != null)
            {
                BuildViewMenu(evt.menu, view, screenRect);
                return;
            }

            if (target?.GetFirstOfType<Edge>() != null)
            {
                evt.menu.AppendAction("Delete", _ => DeleteSelection());
                return;
            }

            evt.menu.AppendAction("Create UI Node", _ =>
            {
                PrepareForStructuralChange();
                NodesCollectionActions.PromptCreateNode(_collection, screenRect, graphPosition);
            });
            evt.menu.AppendAction("Create UI View", _ =>
            {
                PrepareForStructuralChange();
                NodesCollectionActions.PromptCreateView(_collection, screenRect, graphPosition);
            });
            evt.menu.AppendSeparator();
            evt.menu.AppendAction("Auto Layout", _ => AutoLayout());
            evt.menu.AppendAction("Frame All", _ => FrameAll());
        }

        private void BuildScreenMenu(DropdownMenu menu, ScreenNodeElement screen, Rect screenRect)
        {
            var playing = Application.isPlaying;
            menu.AppendAction("Open In Play Mode", _ => UIManager.Instance.ShowViewNode(screen.Id),
                playing && UIManager.Instance != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendSeparator();
            menu.AppendAction("Rename...", _ => NodesCollectionActions.PromptRenameNode(_collection, screen.Data, screenRect),
                playing ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
            menu.AppendAction("Find References", _ => IdReferenceUpdater.LogReferences(_collection.Id, IdKind.Node, screen.Id));
            menu.AppendAction("Select Asset", _ => Selection.activeObject = screen.Data);
            menu.AppendSeparator();
            menu.AppendAction("Delete", _ => DeleteSelection());
        }

        private void BuildViewMenu(DropdownMenu menu, ViewNodeElement view, Rect screenRect)
        {
            var declared = _collection.UIViews.Contains(view.Id);
            var sceneView = view.SceneView;
            var canPreview = Application.isPlaying && sceneView != null;
            menu.AppendAction("Show In Play Mode", _ => ShowViewInPlayMode(sceneView),
                canPreview ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendAction("Hide In Play Mode", _ => sceneView.Hide(),
                canPreview ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendSeparator();
            if (!declared)
            {
                menu.AppendAction("Declare View", _ =>
                {
                    NodesCollectionEditing.CreateView(_collection, view.Id);
                    NodesCollectionActions.NotifyChanged(_collection);
                });
            }
            menu.AppendAction("Rename...", _ => NodesCollectionActions.PromptRenameView(_collection, view.Id, screenRect),
                declared && !Application.isPlaying ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendAction("Find References", _ => IdReferenceUpdater.LogReferences(_collection.Id, IdKind.View, view.Id));
            menu.AppendAction("Select In Scene", _ =>
            {
                Selection.activeGameObject = sceneView.gameObject;
                EditorGUIUtility.PingObject(sceneView.gameObject);
            }, sceneView != null ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
            menu.AppendSeparator();
            menu.AppendAction("Delete", _ => DeleteSelection());
        }

        private static void ShowViewInPlayMode(UIView view)
        {
            if (view.IsPopup && UIManager.Instance != null)
                UIManager.Instance.ShowPopup(view.ViewId);
            else
                view.Show();
        }

        private void Populate()
        {
            var unplaced = new List<UIGraphNode>();
            var screenIds = new HashSet<string>();
            foreach (var node in _collection.UINodes)
            {
                if (node == null || string.IsNullOrEmpty(node.Id) || !screenIds.Add(node.Id))
                    continue;
                var element = new ScreenNodeElement(node);
                element.DoubleClicked += OnNodeDoubleClicked;
                _screens.Add(element);
                Place(element, unplaced);
            }

            var viewIds = _collection.UIViews
                .Concat(_collection.UINodes.Where(x => x != null && x.ViewIds != null).SelectMany(x => x.ViewIds))
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct();
            foreach (var viewId in viewIds)
            {
                var element = new ViewNodeElement(viewId);
                element.DoubleClicked += OnNodeDoubleClicked;
                _views.Add(element);
                Place(element, unplaced);
            }

            foreach (var screen in _screens)
            {
                foreach (var viewId in (screen.Data.ViewIds ?? new string[0]).Distinct())
                {
                    var view = _views.FirstOrDefault(x => x.Id == viewId);
                    if (view != null)
                        AddElement(screen.Output.ConnectTo(view.Input));
                }
            }

            _layoutIncomplete = unplaced.Count > 0;
            if (unplaced.Count == _screens.Count + _views.Count)
                LayoutAll();
            else
                AppendUnplaced(unplaced);
        }

        private void Place(UIGraphNode element, List<UIGraphNode> unplaced)
        {
            AddElement(element);
            if (NodesCollectionEditing.TryGetPosition(_collection, element.Id, element.IsView, out var position))
                element.SetPosition(new Rect(position, Vector2.zero));
            else
                unplaced.Add(element);
        }

        private void LayoutAll()
        {
            for (var i = 0; i < _screens.Count; i++)
                _screens[i].SetPosition(new Rect(ScreenColumnX, i * ScreenSpacing, 0, 0));

            var desired = new Dictionary<ViewNodeElement, float>();
            foreach (var view in _views)
            {
                var indices = _screens.Select((screen, index) => (screen, index))
                    .Where(x => x.screen.Data.ViewIds != null && x.screen.Data.ViewIds.Contains(view.Id))
                    .Select(x => (float)x.index)
                    .ToList();
                desired[view] = indices.Count > 0 ? indices.Average() * ScreenSpacing : float.MaxValue;
            }

            float? previous = null;
            foreach (var view in _views.OrderBy(x => desired[x]))
            {
                var unused = desired[view] == float.MaxValue;
                var target = unused ? (previous ?? -ViewSpacing) + ViewSpacing * 1.5f : desired[view];
                var y = previous.HasValue ? Mathf.Max(target, previous.Value + ViewSpacing) : target;
                previous = y;
                view.SetPosition(new Rect(ViewColumnX, y, 0, 0));
            }
        }

        private void AppendUnplaced(List<UIGraphNode> unplaced)
        {
            var placed = _screens.Cast<UIGraphNode>().Concat(_views).Except(unplaced).ToList();
            foreach (var isView in new[] { false, true })
            {
                var sameKind = placed.Where(x => x.IsView == isView).ToList();
                var x = sameKind.Count > 0 ? sameKind.Min(e => e.Position.x) : isView ? ViewColumnX : ScreenColumnX;
                var y = sameKind.Count > 0 ? sameKind.Max(e => e.Position.y) : 0f;
                var spacing = isView ? ViewSpacing : ScreenSpacing;
                foreach (var element in unplaced.Where(e => e.IsView == isView))
                {
                    y += spacing;
                    element.SetPosition(new Rect(x, y, 0, 0));
                }
            }
        }

        private void SavePositions()
        {
            var positions = _screens.Cast<UIGraphNode>().Concat(_views)
                .Select(x => (x.Id, x.IsView, x.Position))
                .ToList();
            NodesCollectionEditing.SetPositions(_collection, positions);
            _layoutIncomplete = false;
        }

        private void OnNodeDoubleClicked(UIGraphNode element)
        {
            if (Application.isPlaying)
            {
                if (element is ScreenNodeElement screen && UIManager.Instance != null)
                    UIManager.Instance.ShowViewNode(screen.Id);
                else if (element is ViewNodeElement view && view.SceneView != null)
                    ShowViewInPlayMode(view.SceneView);
                return;
            }

            var rect = _toScreenRect(element.worldBound.position + new Vector2(0, element.worldBound.height));
            if (element is ScreenNodeElement screenNode)
                NodesCollectionActions.PromptRenameNode(_collection, screenNode.Data, rect);
            else if (_collection.UIViews.Contains(element.Id))
                NodesCollectionActions.PromptRenameView(_collection, element.Id, rect);
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            if (_rebuilding || _collection == null)
                return change;

            if (change.elementsToRemove != null && change.elementsToRemove.Count > 0)
                HandleRemoval(change);

            if (change.edgesToCreate != null && change.edgesToCreate.Count > 0)
            {
                PrepareForStructuralChange();
                foreach (var edge in change.edgesToCreate.ToList())
                {
                    if (edge.output?.node is ScreenNodeElement screen && edge.input?.node is ViewNodeElement view &&
                        NodesCollectionEditing.Connect(_collection, screen.Data, view.Id))
                        continue;
                    change.edgesToCreate.Remove(edge);
                }
            }

            if (change.movedElements != null && change.movedElements.Count > 0)
                SavePositions();

            _signature = ComputeSignature();
            schedule.Execute(RefreshStates);
            return change;
        }

        private void HandleRemoval(GraphViewChange change)
        {
            var screens = change.elementsToRemove.OfType<ScreenNodeElement>().ToList();
            var views = change.elementsToRemove.OfType<ViewNodeElement>().ToList();
            if (!NodesCollectionActions.ConfirmDelete(_collection, screens.Select(x => x.Data).ToArray(), views.Select(x => x.Id).ToArray()))
            {
                change.elementsToRemove.Clear();
                return;
            }

            PrepareForStructuralChange();
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            foreach (var edge in change.elementsToRemove.OfType<Edge>())
            {
                if (edge.output?.node is ScreenNodeElement screen && edge.input?.node is ViewNodeElement view && !screens.Contains(screen))
                    NodesCollectionEditing.Disconnect(_collection, screen.Data, view.Id);
            }
            foreach (var screen in screens)
            {
                _screens.Remove(screen);
                NodesCollectionEditing.DeleteNode(_collection, screen.Data);
            }
            foreach (var view in views)
            {
                _views.Remove(view);
                NodesCollectionEditing.DeleteView(_collection, view.Id);
            }
            Undo.SetCurrentGroupName("Delete UI Graph Elements");
            Undo.CollapseUndoOperations(undoGroup);
        }

        private Dictionary<string, UIView> FindSceneViews()
        {
            var result = new Dictionary<string, UIView>();
            foreach (var view in Resources.FindObjectsOfTypeAll<UIView>())
            {
                if (EditorUtility.IsPersistent(view) || !view.gameObject.scene.IsValid())
                    continue;
                if (view.CollectionId != _collection.Id || string.IsNullOrEmpty(view.ViewId))
                    continue;
                if (!result.ContainsKey(view.ViewId))
                    result.Add(view.ViewId, view);
            }
            return result;
        }

        private string ComputeSignature()
        {
            if (_collection == null)
                return string.Empty;
            var nodes = _collection.UINodes.Select(x => x == null ? "null" : x.Id + ":" + string.Join(",", x.ViewIds ?? new string[0]));
            return _collection.Id + "|" + string.Join(";", nodes) + "|" + string.Join(",", _collection.UIViews);
        }
    }
}
