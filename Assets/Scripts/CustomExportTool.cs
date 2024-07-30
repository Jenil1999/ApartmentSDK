using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class CustomExportTool : EditorWindow
{
    //private const string UserContentFolder = "Assets/UserContent"; // Adjust this path as needed
    private const string BakeryFolder = "Assets/Bakery";
    private List<string> allowedScripts = new List<string>
    {
        "Webview_Wrapper",
        "NFTView_Wrapper",
        "ExitPortalWrapper",
        // Add other allowed script names here
    };

    private Vector2 scrollPosition;
    private List<string> warnedScripts = new List<string>();

    [MenuItem("Apartment_SDK/Custom Export")]
    public static void ShowWindow()
    {
        GetWindow<CustomExportTool>("Custom Export");
    }

    void OnGUI()
    {
        GUILayout.Label("Custom Export Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Check and Export Scene"))
        {
            CheckSceneAndUserContent();
        }

        if (warnedScripts.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Warned Scripts:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
            foreach (var script in warnedScripts)
            {
                GUILayout.Label(script);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(10);
            if (GUILayout.Button("Export Anyway"))
            {
                ExportCurrentSceneAndUserContent();
            }
        }
    }

    void CheckSceneAndUserContent()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("Check cancelled: Scene not saved.");
            return;
        }

        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(currentScenePath))
        {
            EditorUtility.DisplayDialog("Error", "No active scene found. Please open a scene before checking.", "OK");
            return;
        }

        HashSet<string> processedAssets = new HashSet<string>();
        warnedScripts.Clear();

        CollectDependencies(currentScenePath, processedAssets);

        if (warnedScripts.Count > 0)
        {
            Debug.LogWarning($"Found {warnedScripts.Count} scripts that are not in the allowed list. Check the Custom Export Tool window for details.");
            EditorUtility.DisplayDialog("Check Complete", $"Found {warnedScripts.Count} unauthorized scripts. Review the list in the Custom Export Tool window.", "OK");
        }
        else
        {
            if (EditorUtility.DisplayDialog("Check Complete", "No unauthorized scripts found. Do you want to proceed with the export?", "Yes", "No"))
            {
                ExportCurrentSceneAndUserContent();
            }
        }
    }

    void CollectDependencies(string assetPath, HashSet<string> processedAssets)
    {
        if (processedAssets.Contains(assetPath) || !assetPath.StartsWith("Assets/"))
            return;

        processedAssets.Add(assetPath);

        if (assetPath.EndsWith(".cs"))
        {
            string scriptName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (!allowedScripts.Contains(scriptName) && !assetPath.StartsWith(BakeryFolder))
            {
                warnedScripts.Add(assetPath);
            }
        }

        string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
        foreach (string dependency in dependencies)
        {
            CollectDependencies(dependency, processedAssets);
        }
    }

    void ExportCurrentSceneAndUserContent()
    {
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        List<string> assetsToExport = new List<string> { currentScenePath };
        HashSet<string> processedAssets = new HashSet<string>();

        CollectAssetsForExport(currentScenePath, assetsToExport, processedAssets);

        // Include Bakery
        IncludeBakeryAssets(assetsToExport);

        string exportPath = EditorUtility.SaveFilePanel("Export Scene, User Content, and Bakery", "", "SceneUserContentAndBakery", "unitypackage");
        if (!string.IsNullOrEmpty(exportPath))
        {
            AssetDatabase.ExportPackage(assetsToExport.ToArray(), exportPath, ExportPackageOptions.Default);
            Debug.Log("Scene, user content, and Bakery exported successfully!");
        }
    }

   void IncludeBakeryAssets(List<string> assetsToExport)
    {
        if (Directory.Exists(BakeryFolder))
        {
            string[] bakeryAssets = AssetDatabase.FindAssets("", new[] { BakeryFolder });
            foreach (string asset in bakeryAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(asset);
                if (!assetsToExport.Contains(assetPath))
                {
                    assetsToExport.Add(assetPath);
                }
            }
        }
        else
        {
            Debug.LogWarning("Bakery folder not found at: " + BakeryFolder);
        }
    }

    void CollectAssetsForExport(string assetPath, List<string> assetsToExport, HashSet<string> processedAssets)
    {
        if (processedAssets.Contains(assetPath) || !assetPath.StartsWith("Assets/"))
            return;

        processedAssets.Add(assetPath);
        assetsToExport.Add(assetPath);

        string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);
        foreach (string dependency in dependencies)
        {
            CollectAssetsForExport(dependency, assetsToExport, processedAssets);
        }
    }
}