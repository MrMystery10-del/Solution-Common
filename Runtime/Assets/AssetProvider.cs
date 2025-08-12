using System.IO;

using UnityEngine;

using UnityEditor;

namespace Solution.Common.Assets {
    public static class AssetProvider {
        private const string SettingsFolder = "Assets/Settings";
        private const string StreamingSettingsFolder = "Assets/StreamingAssets/Settings";

        /**
        * <summary>
        * Returns the asset of type T, loaded differently based on build context.
        * </summary>
        */
        public static T GetAsset<T>(string assetName) where T : ScriptableObject {
            #if UNITY_EDITOR
                return LoadOrCreateAssetEditor<T>($"{SettingsFolder}/{assetName}.asset", assetName);
            #else
                return LoadAssetRuntime<T>(assetName);
            #endif
        }

        #if UNITY_EDITOR
        
        private static T LoadOrCreateAssetEditor<T>(string assetPath, string assetName) where T : ScriptableObject {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null) {
                string folder = Path.GetDirectoryName(assetPath);
                if (!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }

                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            ExportAssetToJson(asset, assetName);
            return asset;
        }

        private static void ExportAssetToJson<T>(T asset, string assetName) where T : ScriptableObject {
            string runtimePath = Path.Combine(StreamingSettingsFolder, $"{assetName}.json");

            if (!Directory.Exists(StreamingSettingsFolder)) {
                Directory.CreateDirectory(StreamingSettingsFolder);
            }

            string json = JsonUtility.ToJson(asset, true);
            File.WriteAllText(runtimePath, json);
        }
        
        #endif

        private static T LoadAssetRuntime<T>(string assetName) where T : ScriptableObject {
            string jsonPath = Path.Combine(Application.streamingAssetsPath, "Settings", $"{assetName}.json");

            if (!File.Exists(jsonPath)) {
                return null;
            }

            string json = File.ReadAllText(jsonPath);
            T instance = ScriptableObject.CreateInstance<T>();
            JsonUtility.FromJsonOverwrite(json, instance);
            
            return instance;
        }
    }
}
