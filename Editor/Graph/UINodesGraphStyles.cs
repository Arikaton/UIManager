using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace UIManagement.Graph
{
    public static class UINodesGraphStyles
    {
        private const string PackagePath = "Packages/com.arikaton.uimanager/Editor/Graph/UINodesGraph.uss";

        public static StyleSheet Load()
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(PackagePath);
            if (styleSheet != null)
                return styleSheet;
            var path = AssetDatabase.FindAssets("UINodesGraph t:StyleSheet")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault();
            return path == null ? null : AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
        }
    }
}
