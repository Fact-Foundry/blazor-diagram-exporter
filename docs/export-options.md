# Export Options Reference

The `DiagramExportOptions` class controls every aspect of the export process. All properties have sensible defaults, so you can export a diagram without configuring anything.

## DiagramExportOptions

```csharp
using FactFoundry.BlazorDiagramExporter;

var options = new DiagramExportOptions
{
    Padding = 20.0,
    BackgroundColor = "#ffffff",
    Scale = 1.0,
    FontFamily = "Arial",
    FileName = "diagram"
};
```

## Property Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Padding` | `double` | `20.0` | Padding in pixels around the diagram bounding box. Applied on all four sides. |
| `BackgroundColor` | `string` | `"#ffffff"` | Background color of the exported image. Any valid CSS color string. |
| `Scale` | `double` | `1.0` | Scale factor for PNG export. `2.0` produces double resolution (retina). |
| `FontFamily` | `string` | `"Arial"` | Font family used for all text rendering. Must be available in the browser. See [Fonts](fonts.md). |
| `FileName` | `string` | `"diagram"` | File name for browser downloads (without extension). The extension `.svg`, `.png`, or `.pdf` is appended automatically. |
| `Grid` | `GridOptions?` | `null` | Optional grid overlay. Set `Enabled = true` to render grid lines behind the diagram. |
| `NodeRenderer` | `Func<NodeModel, NodeRenderInfo>?` | `null` | Synchronous custom renderer called once per node during snapshot creation. See [Custom Node Renderer](custom-node-renderer.md). |
| `NodeRendererAsync` | `Func<NodeModel, Task<NodeRenderInfo>>?` | `null` | Asynchronous custom renderer for nodes. Takes precedence over `NodeRenderer` if both are set. |
| `LinkRenderer` | `Func<BaseLinkModel, LinkRenderInfo>?` | `null` | Synchronous custom renderer called once per link during snapshot creation. See [Custom Link Renderer](custom-link-renderer.md). |
| `LinkRendererAsync` | `Func<BaseLinkModel, Task<LinkRenderInfo>>?` | `null` | Asynchronous custom renderer for links. Takes precedence over `LinkRenderer` if both are set. |

## Padding

Padding adds space around the diagram bounding box in the exported image. It prevents nodes at the edges from being flush against the image border.

```csharp
var options = new DiagramExportOptions { Padding = 40.0 };
```

Setting `Padding = 0` produces an image with no margin around the diagram content.

## Background Color

Any valid CSS color string is accepted:

```csharp
// White background (default)
new DiagramExportOptions { BackgroundColor = "#ffffff" }

// Dark theme
new DiagramExportOptions { BackgroundColor = "#1e1e2e" }

// Transparent (PNG only -- PDF will show white)
new DiagramExportOptions { BackgroundColor = "transparent" }

// Named CSS colors
new DiagramExportOptions { BackgroundColor = "cornflowerblue" }
```

## Scale

The scale factor controls the resolution of the exported PNG. This is useful for producing high-DPI images for print or retina displays.

```csharp
// Standard resolution
new DiagramExportOptions { Scale = 1.0 }

// Double resolution (retina)
new DiagramExportOptions { Scale = 2.0 }

// Half resolution (smaller file size)
new DiagramExportOptions { Scale = 0.5 }
```

At `Scale = 2.0`, a diagram that is 800x600 in logical pixels produces a 1600x1200 pixel PNG.

## Font Family

The font must be available in the browser at render time. The library does not load fonts automatically. See [Fonts](fonts.md) for details.

```csharp
// System font stack
new DiagramExportOptions { FontFamily = "Segoe UI, Roboto, sans-serif" }

// Monospace
new DiagramExportOptions { FontFamily = "Cascadia Code, Consolas, monospace" }
```

## File Name

The file name is used for browser downloads. Do not include the extension -- it is appended automatically based on the export method.

```csharp
var options = new DiagramExportOptions { FileName = "sales-model-diagram" };

await Exporter.ExportAsSvgAsync(diagram, options); // Downloads "sales-model-diagram.svg"
await Exporter.ExportAsPngAsync(diagram, options); // Downloads "sales-model-diagram.png"
await Exporter.ExportAsPdfAsync(diagram, options); // Downloads "sales-model-diagram.pdf"
```

## Grid

The optional grid overlay draws evenly-spaced lines behind the diagram content. Useful for visual alignment or when exporting diagrams that include a grid in their live view.

```csharp
var options = new DiagramExportOptions
{
    Grid = new GridOptions
    {
        Enabled = true,
        Spacing = 20.0,
        Color = "rgba(255,255,255,0.12)",
        LineWidth = 1.0
    }
};
```

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `false` | Whether to render the grid |
| `Spacing` | `double` | `20.0` | Distance in pixels between grid lines |
| `Color` | `string` | `"rgba(255,255,255,0.12)"` | Grid line color (any CSS color string) |
| `LineWidth` | `double` | `1.0` | Grid line width in pixels |

## Custom Renderers

Custom renderers let you control the visual appearance of every node and link in the export. The library provides a default renderer that draws a rounded rectangle with a header and port rows. For most real-world applications, you will want to supply custom renderers.

### Sync vs Async Renderers

Both synchronous and asynchronous renderer delegates are supported. If both are set for the same element type, the async renderer takes precedence:

```csharp
var options = new DiagramExportOptions
{
    // Async takes precedence when both are set
    NodeRenderer = node => new NodeRenderInfo { HeaderText = node.Title ?? "Node" },
    NodeRendererAsync = async node =>
    {
        var metadata = await LoadNodeMetadataAsync(node.Id);
        return new NodeRenderInfo { HeaderText = metadata.DisplayName };
    }
};
```

### Default Behavior

When no custom renderers are provided:

- **Nodes** are rendered as rounded rectangles with the node's `Title` (or `Id` if no title) as the header text. Each port gets a row in the body.
- **Links** are rendered with default gray stroke, 2px width, no source arrow, and a filled arrowhead at the target.

See the dedicated guides for full details:
- [Custom Node Renderer](custom-node-renderer.md)
- [Custom Link Renderer](custom-link-renderer.md)

## Full Example

```csharp
var options = new DiagramExportOptions
{
    Padding = 30.0,
    BackgroundColor = "#1a1b26",
    Scale = 2.0,
    FontFamily = "Inter, sans-serif",
    FileName = "data-model",
    NodeRenderer = node => new NodeRenderInfo
    {
        HeaderText = node.Title ?? node.Id,
        HeaderColor = "#2d2d3d",
        HeaderTextColor = "#e0e0e0",
        BodyColor = "#1e1e28",
        BorderColor = "#3d3d4d"
    },
    LinkRenderer = link => new LinkRenderInfo
    {
        StrokeColor = "#6b7280",
        TargetArrow = ArrowStyle.FilledArrow
    }
};

await Exporter.ExportAsPngAsync(diagram, options);
```
