using System;
using UnityEditor;
using UnityEngine;

namespace UIManagement
{
    public class NameInputWindow : EditorWindow
    {
        private const string ControlName = "UIManagerNameInput";

        private string _message;
        private string _confirmText;
        private string _value;
        private string _initialValue;
        private string _toggleLabel;
        private bool _toggleValue;
        private Func<string, string> _validate;
        private Action<string, bool> _confirm;
        private bool _focused;
        private GUIStyle _errorStyle;

        public static void Show(Rect screenRect, string title, string message, string confirmText, string initialValue,
            Func<string, string> validate, Action<string, bool> confirm, string toggleLabel = null, bool toggleValue = true)
        {
            var window = CreateInstance<NameInputWindow>();
            window.titleContent = new GUIContent(title);
            window._message = message;
            window._confirmText = confirmText;
            window._value = initialValue ?? string.Empty;
            window._initialValue = initialValue;
            window._validate = validate;
            window._confirm = confirm;
            window._toggleLabel = toggleLabel;
            window._toggleValue = toggleValue;
            var height = toggleLabel == null ? 96 : 116;
            window.ShowAsDropDown(screenRect, new Vector2(Mathf.Max(280, screenRect.width), height));
        }

        private void OnGUI()
        {
            var current = Event.current;
            if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Escape)
            {
                Close();
                GUIUtility.ExitGUI();
            }
            var submit = current.type == EventType.KeyDown &&
                         (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter);

            GUILayout.Space(4);
            EditorGUILayout.LabelField(titleContent.text, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(_message, EditorStyles.miniLabel);
            GUI.SetNextControlName(ControlName);
            _value = EditorGUILayout.TextField(_value);
            if (!_focused)
            {
                _focused = true;
                EditorGUI.FocusTextInControl(ControlName);
            }

            if (_toggleLabel != null)
                _toggleValue = EditorGUILayout.ToggleLeft(_toggleLabel, _toggleValue);

            var trimmed = _value.Trim();
            var unchanged = trimmed == _initialValue;
            var error = unchanged ? null : _validate?.Invoke(trimmed);
            var canConfirm = error == null && !unchanged;

            if (_errorStyle == null)
                _errorStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = new Color(1f, 0.45f, 0.35f) } };

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(error ?? string.Empty, _errorStyle);
            GUI.enabled = canConfirm;
            if (GUILayout.Button(_confirmText, GUILayout.Width(80)) || (submit && canConfirm))
            {
                var confirm = _confirm;
                var toggleValue = _toggleValue;
                Close();
                confirm?.Invoke(trimmed, toggleValue);
                GUIUtility.ExitGUI();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
    }
}
