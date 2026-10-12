using System;
using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    public static class NodesCollectionActions
    {
        private const string UpdateReferencesLabel = "Update references in scenes and prefabs";

        public static event Action<NodesCollection> CollectionChanged;

        public static void PromptCreateNode(NodesCollection collection, Rect screenRect, Vector2? position = null)
        {
            NameInputWindow.Show(screenRect, "Create UI Node", "Node id:", "Create", string.Empty,
                id => NodesCollectionEditing.ValidateNewNodeId(collection, id),
                (id, _) =>
                {
                    NodesCollectionEditing.CreateNode(collection, id, position);
                    CollectionChanged?.Invoke(collection);
                });
        }

        public static void PromptCreateView(NodesCollection collection, Rect screenRect, Vector2? position = null)
        {
            NameInputWindow.Show(screenRect, "Create UI View", "View id:", "Create", string.Empty,
                id => NodesCollectionEditing.ValidateNewViewId(collection, id),
                (id, _) =>
                {
                    NodesCollectionEditing.CreateView(collection, id, position);
                    CollectionChanged?.Invoke(collection);
                });
        }

        public static void PromptRenameNode(NodesCollection collection, UINode node, Rect screenRect)
        {
            NameInputWindow.Show(screenRect, "Rename UI Node", $"New id for node '{node.Id}':", "Rename", node.Id,
                id => NodesCollectionEditing.ValidateNewNodeId(collection, id),
                (id, updateReferences) =>
                {
                    if (updateReferences && !IdReferenceUpdater.PrepareForRename())
                        return;
                    NodesCollectionEditing.RenameNode(collection, node, id, updateReferences);
                    CollectionChanged?.Invoke(collection);
                }, UpdateReferencesLabel);
        }

        public static void PromptRenameView(NodesCollection collection, string viewId, Rect screenRect)
        {
            NameInputWindow.Show(screenRect, "Rename UI View", $"New id for view '{viewId}':", "Rename", viewId,
                id => NodesCollectionEditing.ValidateNewViewId(collection, id),
                (id, updateReferences) =>
                {
                    if (updateReferences && !IdReferenceUpdater.PrepareForRename())
                        return;
                    NodesCollectionEditing.RenameView(collection, viewId, id, updateReferences);
                    CollectionChanged?.Invoke(collection);
                }, UpdateReferencesLabel);
        }

        public static void PromptRenameCollection(NodesCollection collection, Rect screenRect)
        {
            NameInputWindow.Show(screenRect, "Rename Nodes Collection", "New collection id:", "Rename", collection.Id,
                id => NodesCollectionEditing.ValidateCollectionId(collection, id),
                (id, updateReferences) =>
                {
                    if (updateReferences && !IdReferenceUpdater.PrepareForRename())
                        return;
                    NodesCollectionEditing.RenameCollection(collection, id, updateReferences);
                    CollectionChanged?.Invoke(collection);
                }, UpdateReferencesLabel);
        }

        public static bool ConfirmDelete(NodesCollection collection, UINode[] nodes, string[] viewIds)
        {
            if (nodes.Length == 0 && viewIds.Length == 0)
                return true;
            var names = nodes.Select(x => $"node '{x.Id}'").Concat(viewIds.Select(x => $"view '{x}'"));
            var message = $"Delete {string.Join(", ", names)} from '{collection.Id}'?\n\n" +
                          "Scene and prefab references to these ids will become invalid. You can undo this with Ctrl+Z.";
            return EditorUtility.DisplayDialog("Delete UI graph elements", message, "Delete", "Cancel");
        }

        public static void DeleteNode(NodesCollection collection, UINode node)
        {
            if (!ConfirmDelete(collection, new[] { node }, new string[0]))
                return;
            NodesCollectionEditing.DeleteNode(collection, node);
            CollectionChanged?.Invoke(collection);
        }

        public static void DeleteView(NodesCollection collection, string viewId)
        {
            if (!ConfirmDelete(collection, new UINode[0], new[] { viewId }))
                return;
            NodesCollectionEditing.DeleteView(collection, viewId);
            CollectionChanged?.Invoke(collection);
        }

        public static void NotifyChanged(NodesCollection collection)
        {
            CollectionChanged?.Invoke(collection);
        }
    }
}
