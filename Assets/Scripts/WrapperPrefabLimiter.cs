using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Valt;

public class WrapperPrefabLimiter : EditorWindow
{
    private static readonly Dictionary<string, Dictionary<string, int>> sceneLimits = new Dictionary<string, Dictionary<string, int>>
    {
        {
            "skyapartment", new Dictionary<string, int>
            {
                {"ExitPortalWrapper", 4},
                {"Webview_Wrapper", 5},
                {"NFTView_Wrapper", 6}
            }
        },
        {
            "studioapartment", new Dictionary<string, int>
            {
                {"ExitPortalWrapper", 2},
                {"Webview_Wrapper", 3},
                {"NFTView_Wrapper", 2}
            }
        },
        {
            "penthouseapartment", new Dictionary<string, int>
            {
                {"ExitPortalWrapper", 2},
                {"Webview_Wrapper", 4},
                {"NFTView_Wrapper", 5}
            }
        },
        {
            "standardapartment", new Dictionary<string, int>
            {
                {"ExitPortalWrapper", 1},
                {"Webview_Wrapper", 2},
                {"NFTView_Wrapper", 3}
            }
        }
    };

    [MenuItem("Apartment_SDK/Verify_Scene")]
    public static void ShowWindow()
    {
        GetWindow<WrapperPrefabLimiter>("Wrapper Prefab Limiter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Wrapper Prefab Limiter", EditorStyles.boldLabel);

        if (GUILayout.Button("Check Current Scene Limits"))
        {
            CheckWrapperPrefabLimits();
        }

        if (GUILayout.Button("Check All Scenes"))
        {
            CheckAllScenes();
        }
    }

    public static void CheckWrapperPrefabLimits()
    {
        string currentSceneName = EditorSceneManager.GetActiveScene().name.ToLower();

        var matchingScene = FindMatchingScene(currentSceneName);
        if (matchingScene == null)
        {
            Debug.LogError($"Scene limit not available for: {currentSceneName}. Scene name must contain one of the predefined apartment types.");
            return;
        }

        var limits = sceneLimits[matchingScene];
        CheckWrapperLimit<NFTView_Wrapper>("NFTView_Wrapper", limits);
        CheckWrapperLimit<ExitPortalWrapper>("ExitPortalWrapper", limits);
        CheckWrapperLimit<Webview_Wrapper>("Webview_Wrapper", limits);
    }

    private static string FindMatchingScene(string sceneName)
    {
        return sceneLimits.Keys.FirstOrDefault(key => sceneName.Contains(key));
    }

    private static void CheckAllScenes()
    {
        string currentScenePath = EditorSceneManager.GetActiveScene().path;

        foreach (var scene in EditorBuildSettings.scenes)
        {
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            string sceneName = EditorSceneManager.GetActiveScene().name.ToLower();
            Debug.Log($"Checking scene: {sceneName}");
            CheckWrapperPrefabLimits();
        }

        EditorSceneManager.OpenScene(currentScenePath, OpenSceneMode.Single);
    }

    private static void CheckWrapperLimit<T>(string wrapperName, Dictionary<string, int> limits) where T : MonoBehaviour
    {
        if (!limits.TryGetValue(wrapperName, out int limit))
        {
            Debug.LogWarning($"No limit defined for {wrapperName} in this scene.");
            return;
        }

        T[] wrappers = GameObject.FindObjectsOfType<T>();
        int count = wrappers.Length;

        if (count > limit)
        {
            Debug.LogError($"Too many {wrapperName}s. Limit: {limit}, Current: {count}");
        }
        else
        {
            Debug.Log($"{wrapperName} count is within limits. Current: {count}, Limit: {limit}");
        }
    }
}