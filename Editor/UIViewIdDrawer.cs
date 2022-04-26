using System.Collections.Generic;
using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    [CustomPropertyDrawer(typeof(UIViewId))]
    public class UIViewIdDrawer : PropertyDrawer
    {
        private static readonly Color BlueColor = new Color(0.72f, 0.97f, 1f);
        private static readonly Color RedColor = new Color(1f, 0.36f, 0.23f);
        
        private bool _initialized;

        private void Initialize()
        {
            if(_initialized) return;
            _initialized = true;
            NodesCollection.UpdateInstances();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 70;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Initialize();
            var rect = new Rect(position.x + 5, position.y + 5, position.width - 5, 70);
            var labelRect = new Rect(rect.x, rect.y, rect.width - 20, 20);
            var collectionIdRect = new Rect(rect.x, rect.y + 20, rect.width - 20, 20);
            var viewIdRect = new Rect(rect.x, rect.y + 40, rect.width - 20, 20);
            
            EditorGUI.LabelField(position, "", new GUIStyle("helpBox") {});
            EditorGUI.LabelField(labelRect, "UI View Id");

            var nodesCollections = NodesCollection.Instances;
            if (nodesCollections.Count == 0)
            {
                EditorGUI.LabelField(position, new GUIContent("Create at least one node collection"));
                return;
            }

            var collectionIdProperty = property.FindPropertyRelative(nameof(UIViewId.CollectionId));
            var viewIdProperty = property.FindPropertyRelative(nameof(UIViewId.ViewId));
            var collectionIds = new List<string>(nodesCollections.Select(x => x.Id));
            DrawPopupForStringProperty(collectionIdRect, collectionIdProperty, collectionIds, "UINode collection");
            
            var collectionIdIsValid = string.IsNullOrEmpty(collectionIdProperty.stringValue) == false && collectionIds.Contains(collectionIdProperty.stringValue);
            if (!collectionIdIsValid)
            {
                EditorGUI.LabelField(viewIdRect, "View ID", "Select valid UI Node");
                return;
            }

            var viewIds = nodesCollections.Find(x => x.Id == collectionIdProperty.stringValue).UIViews.ToList();
            DrawPopupForStringProperty(viewIdRect,viewIdProperty, viewIds, "View ID");
        }

        private void DrawPopupForStringProperty(Rect rect, SerializedProperty property, List<string> availableOptions, string label)
        {
            var value = property.stringValue;
            var options = new List<string>(availableOptions);
            
            if (!options.Contains(value))
            {
                options.Add(value);
                GUI.color = RedColor;
            }
            if (string.IsNullOrEmpty(value))
            {
                GUI.color = BlueColor;
            }
            
            EditorGUI.BeginProperty(rect, new GUIContent(""), property);
            int index = options.IndexOf(value);
            index = EditorGUI.Popup(rect, label, index, options.ToArray());
            property.stringValue = options[index];
            EditorGUI.EndProperty();
            GUI.color = Color.white;
        }
    }
}