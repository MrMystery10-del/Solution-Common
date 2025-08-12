using Solution.Common.Assets;
using Solution.Common.Misc;

using System;
using System.Linq;

using UnityEngine;

namespace Solution.Common.UnityExtension {

    /**
    * <summary>
    * Generic resolver that allows runtime lookup of registered scene objects by ID.
    * </summary>
    */
    [Serializable]
    public struct ReferenceResolver<T> where T : Component {

        [SerializeField] private string id;
        [SerializeField] private T reference;
        
        private readonly ICommonLogger Log => AssetProvider.GetAsset<LoggingSettings>("Logging Settings").commonLogger;
        
        private T cachedReference;

        public bool TryGet(Transform context, out T result) {
            result = null;

            if (context == null) {
                throw new ArgumentNullException("Context is null. Returning null.");
            }
            
            if (TryGetCachedReference(out result)) {
                return true;
            }

            if (TryGetByRelativePath(context, out result)) {
                return true;
            }

            if (TryGetFromAncestors(context, out result)) {
                Log.LogInfo($"Found {typeof(T).Name} by ancestors for ID '{id}'. Reference might be invalid. Please configure reference!");
                return true;
            }

            if (TryGetFromScene(out result)) {
                Log.LogInfo($"Found {typeof(T).Name} by scene for ID '{id}'. Reference might be invalid. Please configure reference!");
                return true;
            }

            return false;
        }

        public bool TryGetCachedReference(out T result) {
            if (cachedReference == null) {
                cachedReference = reference;
            }
            
            result = cachedReference;
            
            return cachedReference != null;
        }

        public bool TryGetByRelativePath(Transform context, out T result) {
            result = null;

            if (string.IsNullOrEmpty(id) || context == null) {
                return false;
            }

            var target = ResolveRelativePath(context, id);
            if (target == null) {
                return false;
            }
            
            result = target.GetComponent<T>();
            if (result != null) {
                cachedReference = result;
            }
            
            return result != null;
        }

        public bool TryGetFromAncestors(Transform context, out T result) {
            result = null;
            if (context == null) {
                return false;
            }

            var ancestor = context;
            while (ancestor != null) {
                var found = ancestor.GetComponentInChildren<T>(true);
                if (found != null) {
                    result = found;
                    cachedReference = result;
                    return true;
                }
                ancestor = ancestor.parent;
            }
            return false;
        }

        public bool TryGetFromScene(out T result) {
            result = null;
            var allObjects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
            if (allObjects == null || allObjects.Length == 0) {
                return false;
            }

            // Try to match by GameObject name if id is set and is a path
            if (!string.IsNullOrEmpty(id)) {
                string[] pathParts = id.Split('/');
                string targetName = pathParts.Length > 0 ? pathParts[^1] : id;

                foreach (var obj in allObjects) {
                    if (obj.name == targetName) {
                        result = obj;
                        cachedReference = result;
                        return true;
                    }
                }
            }

            // Otherwise, just return the first found
            result = allObjects[0];
            cachedReference = result;

            return true;
        }

        private readonly Transform ResolveRelativePath(Transform context, string relativePath) {
            if (context == null || string.IsNullOrEmpty(relativePath)) {
                Log.LogInfo("Relative path is empty. Returning null.");
                return null;
            }

            var current = context;
            var parts = relativePath.Split('/');

            foreach (var part in parts) {
                if (part == "..") {
                    if (current.parent != null) {
                        current = current.parent;
                    } else {
                        Log.LogInfo("Attempted to go above root in path traversal. Returning null.");
                        return null;
                    }
                } else {
                    var child = current.Find(part);
                    if (child != null) {
                        current = child;
                    } else {
                        Log.LogInfo($"Could not find child '{part}' in context '{current.name}'. Returning null.");

                        return null;
                    }
                }
            }

            return current;
        }

    }
}