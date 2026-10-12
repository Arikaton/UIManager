using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    public static class NodesCollectionEditing
    {
        public static string ValidateId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return "Id can't be empty";
            if (!id.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return "Use only letters, digits and '_'";
            return null;
        }

        public static string ValidateCollectionId(NodesCollection collection, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return "Id can't be empty";
            if (id != id.Trim() || id.StartsWith("-"))
                return "Id can't start with '-' or a space";
            if (!id.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == ' '))
                return "Use only letters, digits, '_', '-' and spaces";
            if (NodesCollectionDatabase.All.Any(x => x != collection && x.Id == id))
                return $"Collection '{id}' already exists";
            return null;
        }

        public static string ValidateNewNodeId(NodesCollection collection, string id)
        {
            var error = ValidateId(id);
            if (error != null)
                return error;
            return collection.UINodes.Any(x => x != null && x.Id == id) ? $"Node '{id}' already exists" : null;
        }

        public static string ValidateNewViewId(NodesCollection collection, string id)
        {
            var error = ValidateId(id);
            if (error != null)
                return error;
            return collection.UIViews.Contains(id) ? $"View '{id}' already exists" : null;
        }

        public static UINode CreateNode(NodesCollection collection, string id, Vector2? position = null)
        {
            var node = ScriptableObject.CreateInstance<UINode>();
            node.name = id;
            node.Id = id;
            node.Collection = collection;
            node.ViewIds = new string[0];
            AssetDatabase.AddObjectToAsset(node, collection);
            Undo.RegisterCreatedObjectUndo(node, "Create UI Node");

            Undo.RecordObject(collection, "Create UI Node");
            collection.UINodes.Add(node);
            if (position.HasValue)
                SetLayoutEntry(collection, id, false, position.Value);
            Save(collection);
            return node;
        }

        public static void DeleteNode(NodesCollection collection, UINode node)
        {
            Undo.RecordObject(collection, "Delete UI Node");
            collection.UINodes.Remove(node);
            RemoveLayoutEntry(collection, node.Id, false);
            Undo.DestroyObjectImmediate(node);
            Save(collection);
        }

        public static void RenameNode(NodesCollection collection, UINode node, string newId, bool updateReferences)
        {
            var oldId = node.Id;
            if (oldId == newId)
                return;
            Undo.RecordObjects(new Object[] { collection, node }, "Rename UI Node");
            node.Id = newId;
            node.name = newId;
            RenameLayoutEntry(collection, oldId, newId, false);
            EditorUtility.SetDirty(node);
            Save(collection);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(collection));

            if (updateReferences)
                IdReferenceUpdater.Rename(collection.Id, IdKind.Node, oldId, newId);
        }

        public static void CreateView(NodesCollection collection, string viewId, Vector2? position = null)
        {
            Undo.RecordObject(collection, "Create UI View");
            if (!collection.UIViews.Contains(viewId))
                collection.UIViews.Add(viewId);
            if (position.HasValue)
                SetLayoutEntry(collection, viewId, true, position.Value);
            Save(collection);
        }

        public static void DeleteView(NodesCollection collection, string viewId)
        {
            var nodes = NodesUsing(collection, viewId).ToList();
            Undo.RecordObjects(nodes.Cast<Object>().Append(collection).ToArray(), "Delete UI View");
            collection.UIViews.RemoveAll(x => x == viewId);
            foreach (var node in nodes)
            {
                node.ViewIds = node.ViewIds.Where(x => x != viewId).ToArray();
                EditorUtility.SetDirty(node);
            }
            RemoveLayoutEntry(collection, viewId, true);
            Save(collection);
        }

        public static void RenameView(NodesCollection collection, string oldId, string newId, bool updateReferences)
        {
            if (oldId == newId)
                return;
            var nodes = NodesUsing(collection, oldId).ToList();
            Undo.RecordObjects(nodes.Cast<Object>().Append(collection).ToArray(), "Rename UI View");
            for (var i = 0; i < collection.UIViews.Count; i++)
            {
                if (collection.UIViews[i] == oldId)
                    collection.UIViews[i] = newId;
            }
            foreach (var node in nodes)
            {
                node.ViewIds = node.ViewIds.Select(x => x == oldId ? newId : x).ToArray();
                EditorUtility.SetDirty(node);
            }
            RenameLayoutEntry(collection, oldId, newId, true);
            Save(collection);

            if (updateReferences)
                IdReferenceUpdater.Rename(collection.Id, IdKind.View, oldId, newId);
        }

        public static void RenameCollection(NodesCollection collection, string newId, bool updateReferences)
        {
            var oldId = collection.Id;
            if (oldId == newId)
                return;
            Undo.RecordObject(collection, "Rename Nodes Collection");
            collection.Id = newId;
            Save(collection);

            if (updateReferences)
                IdReferenceUpdater.Rename(oldId, IdKind.Collection, oldId, newId);
        }

        public static bool Connect(NodesCollection collection, UINode node, string viewId)
        {
            var viewIds = node.ViewIds ?? new string[0];
            if (viewIds.Contains(viewId))
                return false;
            Undo.RecordObject(node, "Add View To UI Node");
            node.ViewIds = viewIds.Append(viewId).ToArray();
            EditorUtility.SetDirty(node);
            Save(collection);
            return true;
        }

        public static void Disconnect(NodesCollection collection, UINode node, string viewId)
        {
            if (node.ViewIds == null || !node.ViewIds.Contains(viewId))
                return;
            Undo.RecordObject(node, "Remove View From UI Node");
            node.ViewIds = node.ViewIds.Where(x => x != viewId).ToArray();
            EditorUtility.SetDirty(node);
            Save(collection);
        }

        public static IEnumerable<UINode> NodesUsing(NodesCollection collection, string viewId)
        {
            return collection.UINodes.Where(x => x != null && x.ViewIds != null && x.ViewIds.Contains(viewId));
        }

        public static bool TryGetPosition(NodesCollection collection, string id, bool isView, out Vector2 position)
        {
            var entry = FindLayoutEntry(collection, id, isView);
            position = entry?.Position ?? Vector2.zero;
            return entry != null;
        }

        public static void SetPositions(NodesCollection collection, IEnumerable<(string id, bool isView, Vector2 position)> positions)
        {
            Undo.RecordObject(collection, "Move UI Graph Elements");
            foreach (var (id, isView, position) in positions)
                SetLayoutEntry(collection, id, isView, position);
            EditorUtility.SetDirty(collection);
        }

        private static GraphLayoutEntry FindLayoutEntry(NodesCollection collection, string id, bool isView)
        {
            return collection.GraphLayout?.FirstOrDefault(x => x.Id == id && x.IsView == isView);
        }

        private static void SetLayoutEntry(NodesCollection collection, string id, bool isView, Vector2 position)
        {
            if (collection.GraphLayout == null)
                collection.GraphLayout = new List<GraphLayoutEntry>();
            var entry = FindLayoutEntry(collection, id, isView);
            if (entry == null)
            {
                entry = new GraphLayoutEntry { Id = id, IsView = isView };
                collection.GraphLayout.Add(entry);
            }
            entry.Position = position;
        }

        private static void RemoveLayoutEntry(NodesCollection collection, string id, bool isView)
        {
            collection.GraphLayout?.RemoveAll(x => x.Id == id && x.IsView == isView);
        }

        private static void RenameLayoutEntry(NodesCollection collection, string oldId, string newId, bool isView)
        {
            var entry = FindLayoutEntry(collection, oldId, isView);
            if (entry == null)
                return;
            RemoveLayoutEntry(collection, newId, isView);
            entry.Id = newId;
        }

        private static void Save(NodesCollection collection)
        {
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }
    }
}
