#if UNITY_EDITOR

using System.IO;

using UnityEditor;

using UnityEngine;

namespace Solution.Common.Assets {
    public class SettingsAssetPostprocessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] _, string[] __, string[] ___) {
            foreach (string assetPath in importedAssets) {
                if (!assetPath.StartsWith("Assets/Settings") || !assetPath.EndsWith(".asset")) {
                    continue;
                }

                ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                
                if (obj == null) {
                    continue;
                }

                string name = Path.GetFileNameWithoutExtension(assetPath);
                string jsonPath = Path.Combine(Application.streamingAssetsPath, "Settings", name + ".json");

                Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                string json = JsonUtility.ToJson(obj, true);
                File.WriteAllText(jsonPath, json);
            }
        }
    }
}
#endif