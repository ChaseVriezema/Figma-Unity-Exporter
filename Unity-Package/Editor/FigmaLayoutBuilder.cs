using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.IO;

public static class FigmaLayoutBuilder
{
    private const string IMAGE_OUTPUT_DIR = "Assets/FigmaImports/Images";

    public static void Build(FigmaNode node, Transform parent)
    {
        var go = new GameObject(node.name);
        go.transform.SetParent(parent, false);

        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(node.width, node.height);
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(node.x, -node.y);
        rt.localRotation = Quaternion.Euler(0, 0, -node.rotation);

        var cg = go.AddComponent<CanvasGroup>();
        cg.alpha = node.opacity;

        switch (node.type)
        {
            case "TEXT":
                BuildText(go, node);
                break;
            case "RECTANGLE":
            case "ELLIPSE":
            case "VECTOR":
            case "INSTANCE":
            case "BOOLEAN_OPERATION":
                BuildImage(go, node);
                break;
        }

        if (node.children != null)
            foreach (var child in node.children)
                Build(child, go.transform);
    }

    static void BuildText(GameObject go, FigmaNode node)
    {
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = node.characters;
        tmp.fontSize = node.fontSize;
    }

    static void BuildImage(GameObject go, FigmaNode node)
    {
        if (string.IsNullOrEmpty(node.imageData)) return;

        string texturePath = SaveTexture(node);
        if (texturePath == null) return;

        AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            AssetDatabase.WriteImportSettingsIfDirty(texturePath);
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
        }

        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (tex == null)
        {
            Debug.LogWarning($"Could not load texture at: {texturePath}");
            return;
        }

        var img = go.AddComponent<Image>();
        img.sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            Vector2.one * 0.5f
        );
    }

    static string SaveTexture(FigmaNode node)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(node.imageData);
            string fullDir = Path.Combine(Application.dataPath, "FigmaImports/Images");
            Directory.CreateDirectory(fullDir);

            string fileName = !string.IsNullOrEmpty(node.imageName)
                ? node.imageName
                : $"{node.id.Replace(":", "_")}.png";

            string fullPath = Path.Combine(fullDir, fileName);
            File.WriteAllBytes(fullPath, bytes);

            return $"{IMAGE_OUTPUT_DIR}/{fileName}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to decode image for {node.name}: {e.Message}");
            return null;
        }
    }
}