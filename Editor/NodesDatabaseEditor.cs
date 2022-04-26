using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    [CustomEditor(typeof(NodesCollection))]
    public class NodesDatabaseEditor : Editor
    {
        private NodesCollection tgt;
        private Rect _popupRect;

        private void OnEnable()
        {
            tgt = (NodesCollection)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawIdProperty();

            var nodesProperty = serializedObject.FindProperty(nameof(NodesCollection.UINodes));
            EditorGUILayout.BeginHorizontal();
            nodesProperty.isExpanded = EditorGUILayout.Foldout(nodesProperty.isExpanded, nodesProperty.displayName, true);
            if (GUILayout.Button("Add UI Node", GUILayout.Width(100)))
            {
                PopupWindow.Show(_popupRect, new PopupWithTextField("Create UI Node", "UI Node name:", "Create", CreateUINode));
            }
            EditorGUILayout.EndHorizontal();
            DrawNodesList(nodesProperty);
            
            EditorGUILayout.Space();
            
            var uiViewsProperty = serializedObject.FindProperty(nameof(NodesCollection.UIViews));
            EditorGUILayout.BeginHorizontal();
            uiViewsProperty.isExpanded = EditorGUILayout.Foldout(uiViewsProperty.isExpanded, uiViewsProperty.displayName, true);
            if (GUILayout.Button("Add UI View", GUILayout.Width(100)))
            {
                PopupWindow.Show(_popupRect, new PopupWithTextField("Create UI View", "UI View name:", "Create", CreateUIView));
            }
            EditorGUILayout.EndHorizontal();
            DrawUIViewsList(uiViewsProperty);

            if (Event.current.type == EventType.Repaint) _popupRect = GUILayoutUtility.GetLastRect();
        }

        private void CreateUINode(string id)
        {
            var node = CreateInstance<UINode>();
            node.Collection = tgt;
            node.name = id;
            node.Id = id;
            tgt.UINodes.Add(node);
            AssetDatabase.AddObjectToAsset(node, tgt);
            AssetDatabase.SaveAssets();
        }
        
        private void CreateUIView(string viewId)
        {
            tgt.UIViews.Add(viewId);
            EditorUtility.SetDirty(tgt);
            AssetDatabase.SaveAssetIfDirty(tgt);
        }

        private void DrawIdProperty()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(NodesCollection.Id)));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawNodesList(SerializedProperty listProperty)
        {
            EditorGUI.indentLevel += 1;
            if (listProperty.isExpanded)
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    var node = element.objectReferenceValue as UINode;
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(element, new GUIContent());
                    GUI.enabled = true;
                    if(GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog($"Delete \"{node.Id}\" node?",
                            "Warning! This can't be undone!",
                            $"Delete {node.Id}",
                            "Cancel"))
                        {
                            tgt.UINodes.Remove(node);
                            AssetDatabase.RemoveObjectFromAsset(node);
                            AssetDatabase.SaveAssets();
                        }
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel -= 1;
        }
        
        private void DrawUIViewsList(SerializedProperty listProperty)
        {
            EditorGUI.indentLevel += 1;
            if (listProperty.isExpanded)
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(element, new GUIContent());
                    GUI.enabled = true;
                    if(GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(30)))
                    {
                        if (EditorUtility.DisplayDialog($"Delete \"{element.stringValue}\" ui view?",
                            $"Are you sure you want to delete \"{element.stringValue}\" ui view? This can't be undone!",
                            "Delete",
                            "Cancel"))
                        {
                            tgt.UIViews.Remove(element.stringValue);
                            EditorUtility.SetDirty(tgt);
                            AssetDatabase.SaveAssetIfDirty(tgt);
                        }
                        GUIUtility.ExitGUI();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}