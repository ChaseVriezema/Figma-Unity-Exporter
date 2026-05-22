using UnityEngine;
using UnityEditor;
using System.IO;

public class FigmaImporterWindow : EditorWindow
{
    private string jsonPath = "";
    private string statusMessage = "";

    [MenuItem("Tools/Figma Importer")]
    public static void ShowWindow()
    {
        GetWindow<FigmaImporterWindow>("Figma Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Figma Unity Importer", EditorStyles.boldLabel);
        GUILayout.Space(8);

        GUILayout.BeginHorizontal();
        jsonPath = EditorGUILayout.TextField("JSON Path", jsonPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            jsonPath = EditorUtility.OpenFilePanel(
                "Select Figma Export JSON", "", "json"
            );
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(8);

        if (GUILayout.Button("Import"))
        {
            if (string.IsNullOrEmpty(jsonPath) || !File.Exists(jsonPath))
            {
                statusMessage = "Error: please select a valid JSON file.";
                return;
            }
            Import(jsonPath);
        }

        GUILayout.Space(8);
        if (!string.IsNullOrEmpty(statusMessage))
            GUILayout.Label(statusMessage, EditorStyles.helpBox);
    }

    void Import(string path)
    {
        statusMessage = "Importing...";

        string json = File.ReadAllText(path);
        var export = JsonUtility.FromJson<FigmaExport>($"{{\"nodes\":{json}}}");

        if (export?.nodes == null || export.nodes.Count == 0)
        {
            statusMessage = "No nodes found in file.";
            return;
        }

        // Find or create a Canvas
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("FigmaCanvas");
            canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        foreach (var node in export.nodes)
            FigmaLayoutBuilder.Build(node, canvas.transform);

        AssetDatabase.Refresh();
        statusMessage = $"✓ Imported {export.nodes.Count} root node(s).";
    }
}