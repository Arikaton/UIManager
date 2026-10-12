using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIManagement.Graph
{
    public class NodesGraphWindow : EditorWindow
    {
        private const double RefreshInterval = 0.5;

        [SerializeField] private NodesCollection collection;

        private UINodesGraphView _graphView;
        private ObjectField _collectionField;
        private ToolbarButton _issuesButton;
        private VisualElement _emptyState;
        private double _nextRefreshTime;
        private bool _statesDirty;

        [MenuItem("Window/UI Manager/Nodes Graph")]
        public static void OpenWindow()
        {
            Open(null);
        }

        public static void Open(NodesCollection target)
        {
            var window = GetWindow<NodesGraphWindow>();
            window.SetCollection(target != null ? target : window.collection != null ? window.collection : NodesCollectionDatabase.All.FirstOrDefault());
            window.Show();
            window.Focus();
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);
            var target = asset as NodesCollection;
            if (target == null && asset is UINode node)
                target = node.Collection;
            if (target == null)
                return false;
            Open(target);
            return true;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("UI Nodes Graph");
            Undo.undoRedoPerformed += OnUndoRedo;
            NodesCollectionActions.CollectionChanged += OnCollectionChanged;
            EditorApplication.hierarchyChanged += MarkStatesDirty;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            NodesCollectionActions.CollectionChanged -= OnCollectionChanged;
            EditorApplication.hierarchyChanged -= MarkStatesDirty;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void CreateGUI()
        {
            var toolbar = new Toolbar();
            _collectionField = new ObjectField { objectType = typeof(NodesCollection), allowSceneObjects = false, value = collection };
            _collectionField.style.width = 260;
            _collectionField.RegisterValueChangedCallback(evt => SetCollection(evt.newValue as NodesCollection));
            toolbar.Add(_collectionField);

            var addNode = new ToolbarButton { text = "+ Node", tooltip = "Create a UI Node: a screen state that shows a set of views" };
            addNode.clicked += () => PromptCreate(addNode, false);
            toolbar.Add(addNode);
            var addView = new ToolbarButton { text = "+ View", tooltip = "Declare a UI View id" };
            addView.clicked += () => PromptCreate(addView, true);
            toolbar.Add(addView);
            toolbar.Add(new ToolbarButton(() => _graphView.AutoLayout()) { text = "Auto Layout" });
            toolbar.Add(new ToolbarButton(() => _graphView.FrameAll()) { text = "Frame All" });

            var spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            _issuesButton = new ToolbarButton(() => Selection.activeObject = collection)
            {
                tooltip = "Select the collection to review and fix issues in the Inspector"
            };
            toolbar.Add(_issuesButton);
            toolbar.Add(new ToolbarButton(() => Selection.activeObject = collection) { text = "Inspect" });
            rootVisualElement.Add(toolbar);

            _graphView = new UINodesGraphView(ToScreenRect);
            _graphView.style.flexGrow = 1;
            rootVisualElement.Add(_graphView);

            _emptyState = new Label("Select a Nodes Collection in the toolbar, or create one via Assets > Create > UIManagement > Nodes Collection.");
            _emptyState.style.position = Position.Absolute;
            _emptyState.style.top = 40;
            _emptyState.style.left = 12;
            _emptyState.style.color = new Color(0.7f, 0.7f, 0.7f);
            rootVisualElement.Add(_emptyState);

            if (collection == null)
                collection = NodesCollectionDatabase.All.FirstOrDefault();
            ApplyCollection(true);
        }

        private void SetCollection(NodesCollection target)
        {
            if (target == collection && _graphView != null && _graphView.Collection == target)
                return;
            collection = target;
            ApplyCollection(true);
        }

        private void ApplyCollection(bool frame)
        {
            if (_graphView == null)
                return;
            _collectionField.SetValueWithoutNotify(collection);
            _graphView.SetCollection(collection, frame);
            _emptyState.style.display = collection == null ? DisplayStyle.Flex : DisplayStyle.None;
            RefreshToolbar();
        }

        private void RefreshToolbar()
        {
            if (_issuesButton == null)
                return;
            var issues = NodesCollectionValidator.Validate(collection);
            var errors = issues.Count(x => x.Type == MessageType.Error);
            var warnings = issues.Count - errors;
            _issuesButton.text = issues.Count == 0 ? "No issues" : $"Errors: {errors}  Warnings: {warnings}";
            _issuesButton.style.color = errors > 0 ? new Color(1f, 0.45f, 0.35f) : warnings > 0 ? new Color(1f, 0.8f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
        }

        private void PromptCreate(VisualElement anchor, bool view)
        {
            if (collection == null)
                return;
            var rect = ToScreenRect(anchor.worldBound.position + new Vector2(0, anchor.worldBound.height));
            var position = _graphView.VisibleCenter();
            _graphView.PrepareForStructuralChange();
            if (view)
                NodesCollectionActions.PromptCreateView(collection, rect, position);
            else
                NodesCollectionActions.PromptCreateNode(collection, rect, position);
        }

        private Rect ToScreenRect(Vector2 panelPosition)
        {
            return new Rect(position.position + panelPosition, Vector2.zero);
        }

        private void OnSelectionChange()
        {
            var selected = Selection.activeObject as NodesCollection;
            if (selected == null && Selection.activeObject is UINode node)
                selected = node.Collection;
            if (selected != null && selected != collection)
                SetCollection(selected);
        }

        private void OnUndoRedo()
        {
            _graphView?.Rebuild();
            RefreshToolbar();
        }

        private void OnCollectionChanged(NodesCollection changed)
        {
            if (changed != collection || _graphView == null)
                return;
            _graphView.Rebuild();
            RefreshToolbar();
        }

        private void MarkStatesDirty()
        {
            _statesDirty = true;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode || change == PlayModeStateChange.EnteredEditMode)
                _statesDirty = true;
        }

        private void Update()
        {
            if (_graphView == null || EditorApplication.timeSinceStartup < _nextRefreshTime)
                return;
            _nextRefreshTime = EditorApplication.timeSinceStartup + RefreshInterval;

            if (collection == null && _graphView.Collection != null)
            {
                ApplyCollection(false);
                return;
            }

            if (_graphView.RebuildIfChangedExternally())
                RefreshToolbar();
            if (_statesDirty)
            {
                _statesDirty = false;
                _graphView.RefreshStates();
                RefreshToolbar();
            }
            else if (Application.isPlaying)
            {
                _graphView.RefreshRuntimeState();
            }
        }
    }
}

