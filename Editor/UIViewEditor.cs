using UIManagement.Core;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    [CustomEditor(typeof(UIView))]
    public class UIViewEditor : Editor
    {
        private UIView tgt;
        
        private void OnEnable()
        {
            NodesCollection.UpdateInstances();
            tgt = (UIView)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Get start position"))
                tgt.GetStartPosition();
            
            base.OnInspectorGUI();

            DrawShowHideButtons();
        }

        private void DrawShowHideButtons()
        {
            GUI.enabled = EditorApplication.isPlaying;
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show"))
                tgt.Show();
            if (GUILayout.Button("Hide"))
                tgt.Hide();
            if (GUILayout.Button("InstantShow"))
                tgt.InstantShow();
            if (GUILayout.Button("InstantHide"))
                tgt.InstantHide();
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }
    }
}