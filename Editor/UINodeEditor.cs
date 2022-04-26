using System.Linq;
using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    [CustomEditor(typeof(UINode))]
    public class UINodeEditor : Editor
    {
        private static readonly Color RedColor = new Color(1f, 0.36f, 0.23f);
        
        private UINode tgt;

        private void OnEnable()
        {
            tgt = (UINode) target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginVertical(new GUIStyle("helpBox"));
            EditorGUILayout.LabelField("Node", EditorStyles.whiteLargeLabel);
            GUI.enabled = false;
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UINode.Id)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UINode.Collection)));
            GUI.enabled = true;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10);
            var uiViewsProperty = serializedObject.FindProperty(nameof(UINode.ViewIds));
            uiViewsProperty.isExpanded =
                EditorGUILayout.Foldout(uiViewsProperty.isExpanded, uiViewsProperty.displayName, true);
            if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(20)))
            {
                uiViewsProperty.InsertArrayElementAtIndex(uiViewsProperty.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            DrawUIViewsList(uiViewsProperty);

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void DrawUIViewsList(SerializedProperty listProperty)
        {
            EditorGUI.indentLevel += 1;
            if (listProperty.isExpanded)
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    var viewId = element.stringValue;
                    var availableViewIds = tgt.Collection.UIViews.ToList();
                    if (!availableViewIds.Contains(viewId))
                    {
                        availableViewIds.Add(viewId);
                        GUI.color = RedColor;
                    }
                    int viewIdIndex = availableViewIds.IndexOf(viewId);
                    EditorGUILayout.BeginHorizontal();
                    viewIdIndex = EditorGUILayout.Popup("View ID", viewIdIndex, availableViewIds.ToArray());
                    element.stringValue = availableViewIds[viewIdIndex];
                    GUI.color = Color.white;
                    if (GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20)))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                }
            }
            EditorGUI.indentLevel -= 1;
        }
    }
}