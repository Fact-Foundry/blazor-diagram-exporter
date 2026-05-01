# Custom Link Renderer

By default, the exporter draws each link with a gray stroke, 2px width, no source arrowhead, and an open arrowhead at the target. For most real-world applications, you will want to supply a custom link renderer to convey relationship semantics through visual styling.

## How It Works

During export, the library calls your renderer function once for each link in the diagram. Your function receives the `BaseLinkModel` and returns a `LinkRenderInfo` that describes how to draw that link.

Set the renderer on `DiagramExportOptions`:

```csharp
var options = new DiagramExportOptions
{
    LinkRenderer = RenderMyLink
};
```

Or use an async renderer if your rendering logic requires asynchronous operations:

```csharp
var options = new DiagramExportOptions
{
    LinkRendererAsync = RenderMyLinkAsync
};
```

If both `LinkRenderer` and `LinkRendererAsync` are set, the async version takes precedence.

## LinkRenderInfo Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `StrokeColor` | `string` | `"#6b7280"` | Stroke color of the link line (CSS color). |
| `StrokeWidth` | `double` | `2.0` | Stroke width in pixels. |
| `DashPattern` | `double[]` | `[]` (solid) | Dash pattern for the stroke. Empty = solid line. Example: `[5, 3]` = 5px dash, 3px gap. |
| `SourceArrow` | `ArrowStyle` | `None` | Arrowhead style at the source end of the link. |
| `TargetArrow` | `ArrowStyle` | `Arrow` | Arrowhead style at the target end of the link. |
| `Label` | `string?` | `null` | Optional label text displayed at the midpoint of the link. |
| `LabelColor` | `string` | `"#374151"` | Label text color. |
| `LabelBackgroundColor` | `string` | `"#ffffff"` | Label background color (drawn behind the text for readability). |

## ArrowStyle Enum

| Value | Description |
|-------|-------------|
| `None` | No arrowhead. |
| `Arrow` | Open arrowhead (two lines forming a "V"). |
| `FilledArrow` | Filled/solid triangular arrowhead. |
| `Diamond` | Open diamond shape. |
| `FilledDiamond` | Filled diamond shape. |

## Full Working Example

This example shows a data modeling application where links represent relationships between tables, with cardinality indicators and visual distinction between active and inactive relationships:

```csharp
@page "/diagram"
@using Blazor.Diagrams
@using Blazor.Diagrams.Core.Models.Base
@using FactFoundry.BlazorDiagramExporter
@inject DiagramExporter Exporter

<button @onclick="ExportDiagram">Export PNG</button>

@code {
    private BlazorDiagram _diagram = new();

    private async Task ExportDiagram()
    {
        var options = new DiagramExportOptions
        {
            Scale = 2.0,
            BackgroundColor = "#1e1e2e",
            FileName = "relationships",
            LinkRenderer = RenderRelationshipLink
        };

        await Exporter.ExportAsPngAsync(_diagram, options);
    }

    private LinkRenderInfo RenderRelationshipLink(BaseLinkModel linkModel)
    {
        // Cast to your custom link model
        var rel = (RelationshipLinkModel)linkModel;

        return new LinkRenderInfo
        {
            // Active relationships are solid, inactive are semi-transparent
            StrokeColor = rel.IsActive ? "#6b7280" : "#6b728060",
            StrokeWidth = 2.0,

            // Inactive relationships use a dashed line
            DashPattern = rel.IsActive
                ? Array.Empty<double>()
                : new double[] { 6, 4 },

            // Arrowhead style based on relationship type
            SourceArrow = ArrowStyle.None,
            TargetArrow = rel.IsBidirectional
                ? ArrowStyle.Arrow
                : ArrowStyle.FilledArrow,

            // Cardinality label at the midpoint
            Label = $"{GetCardinality(rel.FromCardinality)} → {GetCardinality(rel.ToCardinality)}",
            LabelColor = "#9ca3af",
            LabelBackgroundColor = "#1e1e2e"
        };
    }

    private static string GetCardinality(Cardinality c) => c switch
    {
        Cardinality.One => "1",
        Cardinality.Many => "*",
        _ => "?"
    };
}
```

## Dash Patterns

The `DashPattern` property accepts an array of doubles that define the dash-gap pattern for the link stroke. An empty array produces a solid line.

```csharp
// Solid line (default)
new LinkRenderInfo { DashPattern = Array.Empty<double>() }

// Standard dash: 5px dash, 3px gap
new LinkRenderInfo { DashPattern = new double[] { 5, 3 } }

// Long dash, short gap: 10px dash, 4px gap
new LinkRenderInfo { DashPattern = new double[] { 10, 4 } }

// Dot pattern: 2px dash, 4px gap
new LinkRenderInfo { DashPattern = new double[] { 2, 4 } }

// Dash-dot: 10px dash, 3px gap, 2px dot, 3px gap
new LinkRenderInfo { DashPattern = new double[] { 10, 3, 2, 3 } }
```

The pattern repeats along the length of the link.

## Arrowhead Styles

Here are common combinations for different relationship types:

```csharp
// One-to-many (arrow at target)
new LinkRenderInfo
{
    SourceArrow = ArrowStyle.None,
    TargetArrow = ArrowStyle.FilledArrow
}

// Bidirectional (arrows at both ends)
new LinkRenderInfo
{
    SourceArrow = ArrowStyle.Arrow,
    TargetArrow = ArrowStyle.Arrow
}

// Aggregation (diamond at source, arrow at target)
new LinkRenderInfo
{
    SourceArrow = ArrowStyle.Diamond,
    TargetArrow = ArrowStyle.Arrow
}

// Composition (filled diamond at source, arrow at target)
new LinkRenderInfo
{
    SourceArrow = ArrowStyle.FilledDiamond,
    TargetArrow = ArrowStyle.FilledArrow
}

// No arrowheads (undirected)
new LinkRenderInfo
{
    SourceArrow = ArrowStyle.None,
    TargetArrow = ArrowStyle.None
}
```

## Link Labels

Labels are drawn at the midpoint of the link (or the midpoint of the middle segment if waypoints are present). A background rectangle is drawn behind the text for readability.

```csharp
new LinkRenderInfo
{
    Label = "1 → *",
    LabelColor = "#9ca3af",
    LabelBackgroundColor = "#1e1e2e"
}
```

Set `Label = null` (the default) to hide the label entirely.

## Link Routing and Waypoints

Links are drawn as straight lines between their source and target port positions by default. If the link has waypoints (via `SnapshotLink.Waypoints`), the library draws a polyline through those points.

Waypoints come from `Blazor.Diagrams` link vertices. If you need orthogonal (right-angle) routing, you must supply the waypoints yourself -- the library does not compute them automatically. See [Orthogonal Routing](orthogonal-routing.md) for details.

## Async Link Renderer

Use the async variant when your rendering logic needs to fetch data:

```csharp
var options = new DiagramExportOptions
{
    LinkRendererAsync = async linkModel =>
    {
        var metadata = await _relationshipService.GetAsync(linkModel.Id);
        return new LinkRenderInfo
        {
            StrokeColor = metadata.IsActive ? "#6b7280" : "#6b728060",
            Label = metadata.Description,
            TargetArrow = ArrowStyle.FilledArrow
        };
    }
};
```

## Combining Node and Link Renderers

In practice, you will typically set both `NodeRenderer` and `LinkRenderer` together:

```csharp
var options = new DiagramExportOptions
{
    Scale = 2.0,
    BackgroundColor = "#1e1e2e",
    FileName = "full-model",
    NodeRenderer = RenderTableNode,
    LinkRenderer = RenderRelationshipLink
};

await Exporter.ExportAsPngAsync(diagram, options);
```
