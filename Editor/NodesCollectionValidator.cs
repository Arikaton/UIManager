using System;
using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;

namespace UIManagement
{
    public class NodesCollectionIssue
    {
        public readonly string Message;
        public readonly MessageType Type;
        public readonly string FixLabel;
        public readonly Action Fix;

        public NodesCollectionIssue(string message, MessageType type, string fixLabel = null, Action fix = null)
        {
            Message = message;
            Type = type;
            FixLabel = fixLabel;
            Fix = fix;
        }
    }

    public static class NodesCollectionValidator
    {
        public static List<NodesCollectionIssue> Validate(NodesCollection collection)
        {
            var issues = new List<NodesCollectionIssue>();
            if (collection == null)
                return issues;

            var collectionId = collection.Id;
            if (string.IsNullOrWhiteSpace(collectionId))
                issues.Add(new NodesCollectionIssue("Collection id is empty.", MessageType.Error));
            else if (NodesCollectionDatabase.All.Count(x => x.Id == collectionId) > 1)
                issues.Add(new NodesCollectionIssue($"Another collection also uses id '{collectionId}'. Ids must be unique.", MessageType.Error));

            if (collection.UINodes.Any(x => x == null))
            {
                issues.Add(new NodesCollectionIssue("Node list contains missing entries.", MessageType.Warning, "Remove",
                    () => Modify(collection, "Remove Missing UI Nodes", () => collection.UINodes.RemoveAll(x => x == null))));
            }

            var nodes = collection.UINodes.Where(x => x != null).ToList();
            foreach (var group in nodes.GroupBy(x => x.Id).Where(x => x.Count() > 1))
                issues.Add(new NodesCollectionIssue($"Several nodes use id '{group.Key}'. Rename the duplicates.", MessageType.Error));
            foreach (var group in collection.UIViews.GroupBy(x => x).Where(x => x.Count() > 1))
            {
                var viewId = group.Key;
                issues.Add(new NodesCollectionIssue($"View '{viewId}' is declared {group.Count()} times.", MessageType.Warning, "Remove duplicates",
                    () => Modify(collection, "Remove Duplicate UI Views", () =>
                    {
                        var distinct = collection.UIViews.Distinct().ToList();
                        collection.UIViews.Clear();
                        collection.UIViews.AddRange(distinct);
                    })));
            }

            foreach (var node in nodes)
            {
                if (node.Collection != collection)
                {
                    issues.Add(new NodesCollectionIssue($"Node '{node.Id}' points to a different collection.", MessageType.Warning, "Fix",
                        () => ModifyNode(node, "Fix UI Node Collection", () => node.Collection = collection)));
                }
                if (node.name != node.Id)
                {
                    issues.Add(new NodesCollectionIssue($"Node asset name '{node.name}' differs from id '{node.Id}'.", MessageType.Info, "Sync name",
                        () =>
                        {
                            ModifyNode(node, "Sync UI Node Name", () => node.name = node.Id);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(collection));
                        }));
                }
                var viewIds = node.ViewIds ?? new string[0];
                foreach (var viewId in viewIds.Distinct().Where(x => !collection.UIViews.Contains(x)))
                {
                    issues.Add(new NodesCollectionIssue($"Node '{node.Id}' shows view '{viewId}', which isn't declared in the collection.",
                        MessageType.Error, "Declare view", () => NodesCollectionEditing.CreateView(collection, viewId)));
                }
                if (viewIds.Length != viewIds.Distinct().Count())
                {
                    issues.Add(new NodesCollectionIssue($"Node '{node.Id}' lists the same view more than once.", MessageType.Warning, "Remove duplicates",
                        () => ModifyNode(node, "Remove Duplicate Views", () => node.ViewIds = viewIds.Distinct().ToArray())));
                }
            }

            var path = AssetDatabase.GetAssetPath(collection);
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var orphan in AssetDatabase.LoadAllAssetsAtPath(path).OfType<UINode>().Where(x => !collection.UINodes.Contains(x)))
                {
                    issues.Add(new NodesCollectionIssue($"Node asset '{orphan.name}' is stored in the collection but not listed in it.",
                        MessageType.Warning, "Restore", () => Modify(collection, "Restore UI Node", () => collection.UINodes.Add(orphan))));
                }
            }

            return issues;
        }

        public static void FixAll(NodesCollection collection)
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                var fixable = Validate(collection).Where(x => x.Fix != null).ToList();
                if (fixable.Count == 0)
                    return;
                fixable[0].Fix();
            }
        }

        private static void Modify(NodesCollection collection, string undoName, Action change)
        {
            Undo.RecordObject(collection, undoName);
            change();
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }

        private static void ModifyNode(UINode node, string undoName, Action change)
        {
            Undo.RecordObject(node, undoName);
            change();
            EditorUtility.SetDirty(node);
            AssetDatabase.SaveAssets();
        }
    }
}
