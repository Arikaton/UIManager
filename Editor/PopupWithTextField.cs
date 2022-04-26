using System;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace UIManagement
{
    public class PopupWithTextField : PopupWindowContent
    {
        private Action<string> _confirmAction;
        private string _label;
        private string _message;
        private string _buttonText;

        private string _inputFieldText;

        public PopupWithTextField(string label, string message, string buttonText, Action<string> confirmAction)
        {
            _label = label;
            _message = message;
            _buttonText = buttonText;
            _confirmAction = confirmAction;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(200, 150);
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label(_label, EditorStyles.boldLabel);
            GUILayout.Label(_message);
            
            _inputFieldText = GUILayout.TextField(_inputFieldText);
            _inputFieldText = RemoveSpecialCharacters(_inputFieldText);
            
            GUI.enabled = !string.IsNullOrEmpty(_inputFieldText);
            if (GUILayout.Button(_buttonText))
            {
                _confirmAction?.Invoke(_inputFieldText);
                editorWindow.Close();
            }
            GUI.enabled = true;
        }

        public override void OnOpen()
        {
            
        }

        public override void OnClose()
        {
            
        }

        private static string RemoveSpecialCharacters(string str)
        {
            var sb = new StringBuilder();
            if (string.IsNullOrEmpty(str)) return str;
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}