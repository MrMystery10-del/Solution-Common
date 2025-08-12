using Solution.Common.Misc;
using Solution.Common.Assets;

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Solution.Common.Editor {

    /**
    * <summary>
    * Automatically injects a reference to the Solution.Common assembly definition
    * into all other asmdef files in the project. Ensures all assembly definitions
    * reference the common assembly by GUID, converting name references to GUIDs if needed.
    * </summary>
    */
    [InitializeOnLoad]
    public static class GlobalAsmdefInjector {

        #region Constants

        private const string CommonAssemblyName = "Solution.Common.Runtime";
        private const string CommonAssemblyDefinition = "Solution.Common.Runtime.asmdef";

        #endregion

        #region Static Fields

        private static string CommonAssemblyPath;
        private static string CommonAssemblyGuid;
        private static string[] CachedAssemblyPaths;
        private static ICommonLogger Log => AssetProvider.GetAsset<LoggingSettings>("Logging Settings").commonLogger;

        #endregion

        #region Initialization

        static GlobalAsmdefInjector() => EditorApplication.delayCall += InitializeInjection;

        public static void InitializeInjection() {
            CommonAssemblyPath = FindCommonAssemblyDefinitionPath();
            if (string.IsNullOrEmpty(CommonAssemblyPath)) {
                Log.LogWarning($"Could not find {CommonAssemblyDefinition}. GlobalAsmdefInjector will not inject the common assembly reference into other assembly definitions!");
                return;
            }

            #pragma warning disable UNT0031
            CommonAssemblyGuid = AssetDatabase.AssetPathToGUID(CommonAssemblyPath);
            #pragma warning restore UNT0031

            CachedAssemblyPaths = GetAsmdefPaths();

            InjectIntoAllAssemblyDefinitions();
        }

        // Scan all asmdef files once and cache their names, paths, and GUIDs.
        private static string[] GetAsmdefPaths() {
            return Directory.EnumerateFiles("Assets/Scripts", "*.asmdef", SearchOption.AllDirectories).ToArray();
        }

        #endregion

        #region Utility Methods

        // Finds the path to the common assembly definition file.
        private static string FindCommonAssemblyDefinitionPath() {
            string[] guids = AssetDatabase.FindAssets(
                Path.GetFileNameWithoutExtension(CommonAssemblyDefinition) + " t:asmdef"
            );

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path).Equals(CommonAssemblyDefinition, StringComparison.OrdinalIgnoreCase)) {
                    return path;
                }
            }

            return null;
        }

        // Injects the common assembly reference into all asmdef files except the common one itself.
        private static void InjectIntoAllAssemblyDefinitions() {
            foreach (string path in CachedAssemblyPaths) {
                if (IsCommonAssemblyDefinition(path)) {
                    continue;
                }

                AsmdefDocument document = LoadAssemblyDefinitionDocument(path);
                string[] preparedReferences = PrepareReferences(document.references);
                string[] cleanedReferences = CleanReferences(preparedReferences);
                string[] sortedReferences = SortReferences(cleanedReferences);
                string[] withCommonGuid = AddCommonGuid(sortedReferences);
                string[] finalReferences = withCommonGuid ?? sortedReferences;

                if (!ReferencesEqual(document.references ?? new string[0], finalReferences)) {
                    document.references = finalReferences;
                    SaveAssemblyDefinitionDocument(path, document);
                }
            }
            AssetDatabase.Refresh();
        }

        // Checks if the given path is the common assembly definition.
        private static bool IsCommonAssemblyDefinition(string path) {
            return Path.GetFileName(path).Equals(CommonAssemblyDefinition, System.StringComparison.OrdinalIgnoreCase);
        }

        // Loads an asmdef document from the specified path.
        private static AsmdefDocument LoadAssemblyDefinitionDocument(string path) {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<AsmdefDocument>(json);
        }

        // Prepares references by converting name references to GUIDs if necessary.
        private static string[] PrepareReferences(string[] existingReferences) {
            string[] references = existingReferences ?? new string[0];
            if (!references.Any(reference => reference.StartsWith("GUID:"))) {
                references = references.Select(ConvertNameReferenceToGuid).ToArray();
            }
            return references;
        }

        // Converts a name reference to a GUID reference if possible.
        private static string ConvertNameReferenceToGuid(string reference) {
            string assemblyDefinition = GetPathByName(reference);
            if (!string.IsNullOrEmpty(assemblyDefinition)) {
                return "GUID:" + AssetDatabase.AssetPathToGUID(assemblyDefinition);
            }
            return reference;
        }

        // Gets the path of an assembly definition by its name.
        private static string GetPathByName(string reference) {
            return CachedAssemblyPaths.Where(path => path.EndsWith(reference + ".asmdef")).FirstOrDefault();
        }

        // Cleans references by ensuring they point to valid asmdef files.
        private static string[] CleanReferences(string[] references) {
            return references.Where(reference => {
                if (reference.StartsWith("GUID:")) {
                    string guid = reference[5..];
                    string asset = AssetDatabase.GUIDToAssetPath(guid);
                    return asset.EndsWith(".asmdef");
                } else {
                    string file = GetPathByName(reference);
                    return !string.IsNullOrEmpty(file);
                }
            }).ToArray();
        }

        // Sorts references alphabetically by their resolved name.
        private static string[] SortReferences(string[] references) {
            return references.Distinct()
                .OrderBy(GetReferenceName)
                .ToArray();
        }

        // Adds the common GUID reference if not already present.
        private static string[] AddCommonGuid(string[] sortedReferences) {
            string guidReference = "GUID:" + CommonAssemblyGuid;
            if (sortedReferences.Contains(guidReference)) {
                return null;
            }
            string[] combined = sortedReferences.Concat(new[] { guidReference }).Distinct().ToArray();
            return SortReferences(combined);
        }

        // Gets the display name for a reference (resolves GUID to file name).
        private static string GetReferenceName(string reference) {
            if (reference.StartsWith("GUID:")) {
                string guid = reference[5..];
                string path = AssetDatabase.GUIDToAssetPath(guid);
                return Path.GetFileNameWithoutExtension(path);
            }
            return reference;
        }

        // Checks if two reference arrays are equal.
        private static bool ReferencesEqual(string[] a, string[] b) {
            if (a.Length != b.Length) {
                return false;
            }
            for (int index = 0; index < a.Length; index++) {
                if (a[index] != b[index]) {
                    return false;
                }
            }
            return true;
        }

        // Saves the asmdef document to the specified path.
        private static void SaveAssemblyDefinitionDocument(string path, AsmdefDocument document) {
            string json = JsonUtility.ToJson(document, prettyPrint: true);
            File.WriteAllText(path, json);
        }

        #endregion

        #region Inner Classes

        [Serializable]
        private class AsmdefDocument {
            public string name;
            public string[] references;
        }

        #endregion
    }
}