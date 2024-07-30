using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class SceneSelector : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> sceneNames = new List<string>();

    [MenuItem("Apartment_SDK/Select template")]
    public static void ShowWindow()
    {
        GetWindow<SceneSelector>("Scene Selector");
    }

    private void OnEnable()
    {
        RefreshSceneList();
    }

    private void RefreshSceneList()
    {
        sceneNames.Clear();
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            sceneNames.Add(sceneName);
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Available Scenes", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Scene List"))
        {
            RefreshSceneList();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (string sceneName in sceneNames)
        {
            if (GUILayout.Button(sceneName))
            {
                OpenScene(sceneName);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void OpenScene(string sceneName)
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene " + sceneName);
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
            }
        }
        else
        {
            Debug.LogError("Scene not found: " + sceneName);
        }
    }
}