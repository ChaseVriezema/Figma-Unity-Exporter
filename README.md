# Figma Unity Exporter

Export layouts, hierarchy, text, and images from Figma directly into Unity UI.

## Installation

### Figma Plugin
For development: Plugins → Development → Import plugin from manifest → select figma-plugin/manifest.json

### Unity Package
1. Open Window → Package Manager
2. Click + → Add package from git URL
3. Paste:
https://github.com/ChaseVriezema/figma-unity-exporter.git?path=unity-package

## Usage
1. Select frames in Figma
2. Run the plugin → Export Selection → saves a .json file
3. In Unity: Tools → Figma Importer → Browse → pick the .json file