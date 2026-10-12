using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UIManagement.Graph;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    [CustomEditor(typeof(NodesCollection))]
    public class NodesCollectionEditor : Editor
    {
        private const float RowButtonWidth = 60;

        private NodesCollection _collection;
        private List<NodesCollectionIssue> _issues = new List<NodesCollectionIssue>();
        private bool _nodesExpanded = true;
        private bool _viewsExpanded = true;

        private void OnEnable()
        {
            _collection = (NodesCollection)target;
            Undo.undoRedoPerformed += Revalidate;
            NodesCollectionActions.CollectionChanged += OnCollectionChanged;
            Revalidate();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Revalidate;
            NodesCollectionActions.CollectionChanged -= OnCollectionChanged;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Collection Id", _collection.Id, EditorStyles.boldLabel);
            if (GUILayout.Button("Rename", GUILayout.Width(RowButtonWidth)))
                NodesCollectionActions.PromptRenameCollection(_collection, LastRectOnScreen());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Graph Editor", GUILayout.Height(28)))
                NodesGraphWindow.Open(_collection);
            EditorGUILayout.Space();

            DrawIssues();
            DrawNodes();
            EditorGUILayout.Space();
            DrawViews();
        }

        private void DrawIssues()
        {
            if (_issues.Count == 0)
                return;
            foreach (var issue in _issues)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox(issue.Message, issue.Type);
                if (issue.Fix != null && GUILayout.Button(issue.FixLabel, GUILayout.Width(110), GUILayout.ExpandHeight(true)))
                {
                    issue.Fix();
                    AfterChange();
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            if (_issues.Count(x => x.Fix != null) > 1 && GUILayout.Button("Fix All"))
            {
                NodesCollectionValidator.FixAll(_collection);
                AfterChange();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.Space();
        }

        private void DrawNodes()
        {
            var nodes = _collection.UINodes.Where(x => x != null).ToList();
            EditorGUILayout.BeginHorizontal();
            _nodesExpanded = EditorGUILayout.Foldout(_nodesExpanded, $"UI Nodes ({nodes.Count})", true, EditorStyles.foldoutHeader);
            if (GUILayout.Button("Add Node", GUILayout.Width(90)))
                NodesCollectionActions.PromptCreateNode(_collection, LastRectOnScreen());
            EditorGUILayout.EndHorizontal();
            if (!_nodesExpanded)
                return;

            EditorGUI.indentLevel++;
            foreach (var node in nodes)
            {
                EditorGUILayout.BeginHorizontal();
                var viewCount = node.ViewIds?.Length ?? 0;
                if (GUILayout.Button(new GUIContent(node.Id, "Select the node asset"), EditorStyles.label, GUILayout.MinWidth(80)))
                    Selection.activeObject = node;
                GUILayout.FlexibleSpace();
                GUILayout.Label(viewCount == 1 ? "1 view" : $"{viewCount} views", EditorStyles.miniLabel, GUILayout.Width(60));
                if (GUILayout.Button("Rename", EditorStyles.miniButtonLeft, GUILayout.Width(RowButtonWidth)))
                    NodesCollectionActions.PromptRenameNode(_collection, node, LastRectOnScreen());
                if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, GUILayout.Width(RowButtonWidth)))
                {
                    NodesCollectionActions.DeleteNode(_collection, node);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void DrawViews()
        {
            EditorGUILayout.BeginHorizontal();
            _viewsExpanded = EditorGUILayout.Foldout(_viewsExpanded, $"UI Views ({_collection.UIViews.Count})", true, EditorStyles.foldoutHeader);
            if (GUILayout.Button("Add View", GUILayout.Width(90)))
                NodesCollectionActions.PromptCreateView(_collection, LastRectOnScreen());
            EditorGUILayout.EndHorizontal();
            if (!_viewsExpanded)
                return;

            EditorGUI.indentLevel++;
            foreach (var viewId in _collection.UIViews.Distinct().ToList())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(viewId, GUILayout.MinWidth(80));
                var usedBy = NodesCollectionEditing.NodesUsing(_collection, viewId).Count();
                GUILayout.Label(usedBy == 0 ? "no nodes" : usedBy == 1 ? "1 node" : $"{usedBy} nodes", EditorStyles.miniLabel, GUILayout.Width(60));
                if (GUILayout.Button("Rename", EditorStyles.miniButtonLeft, GUILayout.Width(RowButtonWidth)))
                    NodesCollectionActions.PromptRenameView(_collection, viewId, LastRectOnScreen());
                if (GUILayout.Button("Delete", EditorStyles.miniButtonRight, GUILayout.Width(RowButtonWidth)))
                {
                    NodesCollectionActions.DeleteView(_collection, viewId);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private static Rect LastRectOnScreen()
        {
            return GUIUtility.GUIToScreenRect(GUILayoutUtility.GetLastRect());
        }

        private void OnCollectionChanged(NodesCollection changed)
        {
            if (changed == _collection)
                AfterChange();
        }

        private void AfterChange()
        {
            Revalidate();
            Repaint();
        }

        private void Revalidate()
        {
            if (_collection != null)
                _issues = NodesCollectionValidator.Validate(_collection);
        }
    }
}
