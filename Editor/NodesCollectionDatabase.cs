using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;

namespace UIManagement
{
    public static class NodesCollectionDatabase
    {
        private static List<NodesCollection> _collections;

        public static IReadOnlyList<NodesCollection> All
        {
            get
            {
                if (_collections == null || _collections.Any(x => x == null))
                    Refresh();
                return _collections;
            }
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.projectChanged += () => _collections = null;
        }

        public static void Refresh()
        {
            _collections = AssetDatabase.FindAssets($"t:{nameof(NodesCollection)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<NodesCollection>)
                .Where(x => x != null)
                .ToList();
        }

        public static NodesCollection Find(string collectionId)
        {
            return All.FirstOrDefault(x => x.Id == collectionId);
        }
    }
}
