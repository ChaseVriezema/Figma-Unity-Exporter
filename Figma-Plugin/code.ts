figma.showUI(__html__, { width: 320, height: 480, title: "Unity Exporter" });

figma.ui.onmessage = async (msg: { type: string }) => {
  if (msg.type === 'export') {
    const selection = figma.currentPage.selection;

    if (selection.length === 0) {
      figma.ui.postMessage({
        type: 'error',
        message: 'Select at least one frame to export.'
      });
      return;
    }

    try {
      const nodes = await serializeNodes(selection);
      figma.ui.postMessage({ type: 'export-ready', data: nodes });
    } catch (err) {
      figma.ui.postMessage({
        type: 'error',
        message: `Export failed: ${err}`
      });
    }
  }

  if (msg.type === 'cancel') {
    figma.closePlugin();
  }
};

const IMAGE_TYPES = new Set([
  'RECTANGLE', 'ELLIPSE', 'VECTOR',
  'STAR', 'POLYGON', 'BOOLEAN_OPERATION', 'INSTANCE'
]);

async function serializeNodes(nodes: readonly SceneNode[]): Promise<object[]> {
  const results = [];

  for (const node of nodes) {
    const parentWidth = node.parent && 'width' in node.parent
      ? (node.parent as FrameNode).width : 0;
    const parentHeight = node.parent && 'height' in node.parent
      ? (node.parent as FrameNode).height : 0;

    const base: Record<string, unknown> = {
      id: node.id,
      name: node.name,
      type: node.type,
      x: node.x,
      y: node.y,
      width: 'width' in node ? node.width : 0,
      height: 'height' in node ? node.height : 0,
      rotation: 'rotation' in node ? node.rotation : 0,
      opacity: 'opacity' in node ? node.opacity : 1,
      parentWidth,
      parentHeight,
    };

    if (node.type === 'TEXT') {
      base.characters = node.characters;
      base.fontSize = node.fontSize;
      base.textAlignHorizontal = node.textAlignHorizontal;
      base.textAlignVertical = node.textAlignVertical;
    }

    const isLeaf = !('children' in node) ||
      (node as FrameNode).children.length === 0;
    const isImageType = IMAGE_TYPES.has(node.type);

    if ('exportAsync' in node && (isLeaf || isImageType)) {
      try {
        const bytes = await node.exportAsync({
          format: 'PNG',
          constraint: { type: 'SCALE', value: 2 }
        });
        const safeName = node.name.replace(/[^a-zA-Z0-9]/g, '_');
        const safeId = node.id.replace(/[^a-zA-Z0-9]/g, '_');
        base.imageData = figma.base64Encode(bytes);
        base.imageName = `${safeName}_${safeId}.png`;
      } catch (_e) {
        base.imageData = null;
        base.imageName = null;
      }
    }

    if ('children' in node) {
      base.children = await serializeNodes(
        (node as FrameNode).children
      );
    }

    results.push(base);
  }

  return results;
}