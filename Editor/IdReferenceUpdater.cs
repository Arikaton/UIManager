using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UIManagement.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIManagement
{
    public enum IdKind
    {
        Collection,
        Node,
        View
    }

    public static class IdReferenceUpdater
    {
        private static readonly string[] AssetExtensions = { ".unity", ".prefab", ".asset" };

        public static bool PrepareForRename()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("Rename", "Exit play mode before renaming ids.", "OK");
                return false;
            }
            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        public static void Rename(string collectionId, IdKind kind, string oldId, string newId)
        {
            var loadedScenePaths = new HashSet<string>(LoadedScenes().Select(x => x.path));
            var changedFiles = new List<string>();
            foreach (var path in EnumerateAssetFiles())
            {
                if (loadedScenePaths.Contains(path))
                    continue;
                var text = File.ReadAllText(path);
                if (!text.StartsWith("%YAML") || !text.Contains(oldId))
                    continue;
                var replaced = ReplaceInYaml(text, collectionId, kind, oldId, newId);
                if (replaced == text)
                    continue;
                File.WriteAllText(path, replaced, new UTF8Encoding(false));
                changedFiles.Add(path);
            }
            if (changedFiles.Count > 0)
                AssetDatabase.Refresh();

            foreach (var scene in LoadedScenes())
            {
                if (RenameInLoadedScene(scene, collectionId, kind, oldId, newId))
                {
                    EditorSceneManager.SaveScene(scene);
                    changedFiles.Add(scene.path);
                }
            }

            var log = new StringBuilder($"[UIManager] Renamed {kind} '{oldId}' to '{newId}'. Updated {changedFiles.Count} file(s).");
            foreach (var file in changedFiles)
                log.Append("\n  ").Append(file);
            Debug.Log(log.ToString());

            foreach (var script in FindCodeReferences(oldId))
                Debug.LogWarning($"[UIManager] '{script}' contains the string \"{oldId}\". Code references are not renamed automatically.",
                    AssetDatabase.LoadAssetAtPath<Object>(script));
        }

        public static List<string> FindReferences(string collectionId, IdKind kind, string id)
        {
            var result = new List<string>();
            foreach (var path in EnumerateAssetFiles())
            {
                var text = File.ReadAllText(path);
                if (!text.StartsWith("%YAML") || !text.Contains(id))
                    continue;
                if (ReplaceInYaml(text, collectionId, kind, id, id + "\u0001") != text)
                    result.Add(path);
            }
            result.AddRange(FindCodeReferences(id));
            return result;
        }

        public static void LogReferences(string collectionId, IdKind kind, string id)
        {
            var references = FindReferences(collectionId, kind, id);
            Debug.Log($"[UIManager] {kind} '{id}' is referenced in {references.Count} file(s). Select an entry below to ping it.");
            foreach (var path in references)
                Debug.Log($"[UIManager]   {path}", AssetDatabase.LoadAssetAtPath<Object>(path));
        }

        private static string ReplaceInYaml(string text, string collectionId, IdKind kind, string oldId, string newId)
        {
            var old = Quoted(oldId);
            if (kind == IdKind.Collection)
            {
                text = Regex.Replace(text,
                    $@"(CollectionId:[ \t]*){old}([ \t]*\r?\n[ \t]*(?:NodeId|ViewId):)",
                    m => m.Groups[1].Value + newId + m.Groups[2].Value);
                return ReplaceOverrides(text, nameof(UINodeId.CollectionId), old, newId);
            }

            var field = kind == IdKind.Node ? nameof(UINodeId.NodeId) : nameof(UIViewId.ViewId);
            text = Regex.Replace(text,
                $@"(CollectionId:[ \t]*{Quoted(collectionId)}[ \t]*\r?\n[ \t]*{field}:[ \t]*){old}([ \t]*\r?\n)",
                m => m.Groups[1].Value + newId + m.Groups[2].Value);
            return ReplaceOverrides(text, field, old, newId);
        }

        private static string ReplaceOverrides(string text, string field, string oldPattern, string newId)
        {
            return Regex.Replace(text,
                $@"(propertyPath:[ \t]*\S*\.{field}[ \t]*\r?\n[ \t]*value:[ \t]*){oldPattern}([ \t]*\r?\n)",
                m => m.Groups[1].Value + newId + m.Groups[2].Value);
        }

        private static string Quoted(string value)
        {
            var escaped = Regex.Escape(value);
            return $@"(?:{escaped}|'{escaped}'|""{escaped}"")";
        }

        private static bool RenameInLoadedScene(Scene scene, string collectionId, IdKind kind, string oldId, string newId)
        {
            var changed = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
                {
                    if (behaviour == null)
                        continue;
                    var serializedObject = new SerializedObject(behaviour);
                    var property = serializedObject.GetIterator();
                    var objectChanged = false;
                    while (property.Next(true))
                    {
                        if (property.propertyType != SerializedPropertyType.Generic)
                            continue;
                        if (property.type != nameof(UINodeId) && property.type != nameof(UIViewId))
                            continue;
                        objectChanged |= RenameInIdProperty(property, kind, collectionId, oldId, newId);
                    }
                    if (objectChanged)
                    {
                        serializedObject.ApplyModifiedPropertiesWithoutUndo();
                        changed = true;
                    }
                }
            }
            if (changed)
                EditorSceneManager.MarkSceneDirty(scene);
            return changed;
        }

        private static bool RenameInIdProperty(SerializedProperty property, IdKind kind, string collectionId, string oldId, string newId)
        {
            var collectionProperty = property.FindPropertyRelative(nameof(UINodeId.CollectionId));
            if (collectionProperty == null || collectionProperty.stringValue != collectionId)
                return false;
            if (kind == IdKind.Collection)
            {
                collectionProperty.stringValue = newId;
                return true;
            }

            var isNodeId = property.type == nameof(UINodeId);
            if (isNodeId != (kind == IdKind.Node))
                return false;
            var idProperty = property.FindPropertyRelative(isNodeId ? nameof(UINodeId.NodeId) : nameof(UIViewId.ViewId));
            if (idProperty == null || idProperty.stringValue != oldId)
                return false;
            idProperty.stringValue = newId;
            return true;
        }

        private static IEnumerable<Scene> LoadedScenes()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && !string.IsNullOrEmpty(scene.path))
                    yield return scene;
            }
        }

        private static IEnumerable<string> EnumerateAssetFiles()
        {
            return Directory.EnumerateFiles("Assets", "*.*", SearchOption.AllDirectories)
                .Where(x => AssetExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
                .Select(x => x.Replace('\\', '/'));
        }

        private static IEnumerable<string> FindCodeReferences(string id)
        {
            var literal = $"\"{id}\"";
            return Directory.EnumerateFiles("Assets", "*.cs", SearchOption.AllDirectories)
                .Where(x => File.ReadAllText(x).Contains(literal))
                .Select(x => x.Replace('\\', '/'));
        }
    }
}
