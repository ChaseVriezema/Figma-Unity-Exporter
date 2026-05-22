using System;
using System.Collections.Generic;

[Serializable]
public class FigmaNode
{
    public string id;
    public string name;
    public string type;
    public float x;
    public float y;
    public float width;
    public float height;
    public float rotation;
    public float opacity;
    public float parentWidth;
    public float parentHeight;
    public string characters;
    public float fontSize;
    public string imageData;   // base64 encoded PNG
    public string imageName;   // safe filename
    public List<FigmaNode> children;
}

[Serializable]
public class FigmaExport
{
    public List<FigmaNode> nodes;
}