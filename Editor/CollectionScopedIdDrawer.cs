using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    public abstract class CollectionScopedIdDrawer : PropertyDrawer
    {
        private static readonly Color BlueColor = new Color(0.72f, 0.97f, 1f);
        private static readonly Color RedColor = new Color(1f, 0.36f, 0.23f);
        private const float Padding = 4;

        protected abstract string IdFieldName { get; }
        protected abstract string IdLabel { get; }
        protected abstract IEnumerable<string> GetIds(NodesCollection collection);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var line = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return line * 3 + Padding * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var step = line + EditorGUIUtility.standardVerticalSpacing;
            var rect = new Rect(position.x + Padding, position.y + Padding, position.width - Padding * 2, line);

            GUI.Box(position, GUIContent.none, EditorStyles.helpBox);
            EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);

            var collections = NodesCollectionDatabase.All;
            rect.y += step;
            if (collections.Count == 0)
            {
                EditorGUI.LabelField(rect, "Create at least one Nodes Collection");
                return;
            }

            var collectionIdProperty = property.FindPropertyRelative(nameof(UINodeId.CollectionId));
            var idProperty = property.FindPropertyRelative(IdFieldName);
            var collectionIds = collections.Select(x => x.Id).ToList();
            DrawPopup(rect, collectionIdProperty, collectionIds, "Collection");

            rect.y += step;
            var collection = collections.FirstOrDefault(x => x.Id == collectionIdProperty.stringValue);
            if (collection == null)
            {
                EditorGUI.LabelField(rect, IdLabel, "Select a valid collection");
                return;
            }
            DrawPopup(rect, idProperty, GetIds(collection).ToList(), IdLabel);
        }

        private static void DrawPopup(Rect rect, SerializedProperty property, List<string> availableOptions, string label)
        {
            var value = property.stringValue;
            var options = new List<string>(availableOptions);
            if (!options.Contains(value))
            {
                options.Add(value);
                GUI.color = string.IsNullOrEmpty(value) ? BlueColor : RedColor;
            }

            var content = EditorGUI.BeginProperty(rect, new GUIContent(label), property);
            var index = EditorGUI.Popup(rect, content.text, options.IndexOf(value), options.ToArray());
            if (index >= 0 && options[index] != value)
                property.stringValue = options[index];
            EditorGUI.EndProperty();
            GUI.color = Color.white;
        }
    }

    [CustomPropertyDrawer(typeof(UINodeId))]
    public class UINodeIdDrawer : CollectionScopedIdDrawer
    {
        protected override string IdFieldName => nameof(UINodeId.NodeId);
        protected override string IdLabel => "Node";

        protected override IEnumerable<string> GetIds(NodesCollection collection)
        {
            return collection.UINodes.Where(x => x != null).Select(x => x.Id);
        }
    }

    [CustomPropertyDrawer(typeof(UIViewId))]
    public class UIViewIdDrawer : CollectionScopedIdDrawer
    {
        protected override string IdFieldName => nameof(UIViewId.ViewId);
        protected override string IdLabel => "View";

        protected override IEnumerable<string> GetIds(NodesCollection collection)
        {
            return collection.UIViews;
        }
    }
}
