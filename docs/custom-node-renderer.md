# Custom Node Renderer

By default, the exporter draws each node as a rounded rectangle with the node's title as a header and one row per port. For most real-world applications, you will want to supply a custom node renderer that maps your domain-specific node models to the export's visual representation.

## How It Works

During export, the library calls your renderer function once for each node in the diagram. Your function receives the `NodeModel` and returns a `NodeRenderInfo` that describes how to draw that node.

Set the renderer on `DiagramExportOptions`:

```csharp
var options = new DiagramExportOptions
{
    NodeRenderer = RenderMyNode
};
```

Or use an async renderer if your rendering logic requires asynchronous operations (such as loading metadata from a service):

```csharp
var options = new DiagramExportOptions
{
    NodeRendererAsync = RenderMyNodeAsync
};
```

If both `NodeRenderer` and `NodeRendererAsync` are set, the async version takes precedence.

## NodeRenderInfo Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `HeaderText` | `string` | `""` | Text displayed in the node header bar. |
| `HeaderColor` | `string` | `"#4a6fa5"` | Background color of the header bar (CSS color). |
| `HeaderTextColor` | `string` | `"#ffffff"` | Text color for the header label. |
| `BodyColor` | `string` | `"#ffffff"` | Background color of the node body. |
| `BorderColor` | `string` | `"#cccccc"` | Border color of the node. |
| `BorderRadius` | `double` | `8.0` | Corner radius in pixels. |
| `Sections` | `List<NodeSection>` | `[]` | Ordered sections of rows (e.g., column lists, measure groups). |
| `PortYOffsets` | `Dictionary<string, double>?` | `null` | Explicit port Y offset overrides, keyed by port ID. Values are in pixels from the top of the node. |

## NodeSection

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SectionLabel` | `string?` | `null` | Optional label displayed above this section's rows (e.g., "Measures"). |
| `Rows` | `List<NodeRow>` | `[]` | Ordered list of rows in this section. |

## NodeRow

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Icon` | `string?` | `null` | Icon before the label. Accepts a Unicode character/emoji (e.g., `"key_emoji"`, `"star"`) or an SVG path data string (rendered as a nested SVG element). |
| `Label` | `string` | `""` | Primary label text. |
| `SecondaryText` | `string?` | `null` | Optional secondary text displayed after the label (e.g., a data type). |
| `TextColor` | `string` | `"#1f2937"` | Text color for the label. |
| `RowBackgroundColor` | `string?` | `null` | Background color of this row. `null` inherits from `BodyColor`. |
| `RowHeight` | `double` | `28.0` | Height of this row in pixels. |
| `PortId` | `string?` | `null` | ID of the port this row corresponds to. Used for auto-calculating port Y offsets for link endpoints. |

## Full Working Example

This example shows a data modeling application where nodes represent database tables with typed columns and optional measure sections:

```csharp
@page "/diagram"
@using Blazor.Diagrams
@using Blazor.Diagrams.Core.Models
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
            FontFamily = "Cascadia Code, Consolas, monospace",
            FileName = "data-model",
            NodeRenderer = RenderTableNode
        };

        await Exporter.ExportAsPngAsync(_diagram, options);
    }

    private NodeRenderInfo RenderTableNode(NodeModel nodeModel)
    {
        // Cast to your custom node model
        var table = (TableNodeModel)nodeModel;

        var info = new NodeRenderInfo
        {
            HeaderText = table.TableName,
            HeaderColor = "#2d2d3d",
            HeaderTextColor = "#e0e0e0",
            BodyColor = "#1e1e28",
            BorderColor = table.IsHidden ? "#3d3d4d80" : "#3d3d4d",
            BorderRadius = 6.0
        };

        // Add a section for columns
        var columnSection = new NodeSection();
        foreach (var col in table.Columns)
        {
            columnSection.Rows.Add(new NodeRow
            {
                Icon = col.IsKey ? "🔑" : "▪",
                Label = col.Name,
                SecondaryText = col.DataType,
                TextColor = col.IsHidden ? "#888888" : "#e0e0e0",
                PortId = $"{table.TableName}.{col.Name}"
            });
        }
        info.Sections.Add(columnSection);

        // Add a section for measures (if any)
        if (table.Measures.Count > 0)
        {
            var measureSection = new NodeSection { SectionLabel = "Measures" };
            foreach (var measure in table.Measures)
            {
                measureSection.Rows.Add(new NodeRow
                {
                    Icon = "Σ",
                    Label = measure.Name,
                    SecondaryText = measure.DataType,
                    TextColor = measure.IsHidden ? "#888888" : "#e0e0e0"
                });
            }
            info.Sections.Add(measureSection);
        }

        return info;
    }
}
```

## Sections and Layout

The library renders nodes with this vertical layout:

```
+----------------------------------+
| Header (HeaderText, HeaderColor) |   <- 32px tall
+----------------------------------+
| [Section Label, if present]      |   <- 24px tall (only if SectionLabel is set)
| [Icon] Label   SecondaryText     |   <- RowHeight (default 28px)
| [Icon] Label   SecondaryText     |
+----------------------------------+
| [Section Label, if present]      |   <- next section
| [Icon] Label   SecondaryText     |
+----------------------------------+
```

Each row's `PortId` is used to auto-calculate where link endpoints attach to the node. When a link connects to port `"Sales.Amount"`, the library finds the row with `PortId = "Sales.Amount"` and draws the link endpoint at the vertical center of that row.

## Port Y Offsets

By default, port Y offsets are auto-calculated by walking through the sections and rows. The library adds up:
- Header height (32px)
- Section label heights (24px each, when present)
- Row heights (default 28px each)

The link endpoint is placed at the vertical center of the matching row.

If you already know the exact pixel positions (for example, from your rendered node component), you can override the auto-calculation:

```csharp
var info = new NodeRenderInfo
{
    HeaderText = "Sales",
    PortYOffsets = new Dictionary<string, double>
    {
        ["Sales.SalesKey"] = 46.0,   // 32 header + 14 (center of first row)
        ["Sales.Amount"] = 74.0,     // 32 header + 28 + 14 (center of second row)
        ["Sales.ProductKey"] = 102.0  // 32 header + 56 + 14 (center of third row)
    }
};
```

When `PortYOffsets` contains an entry for a given port ID, that value is used directly. For any port not in the dictionary, the auto-calculation is used as a fallback.

## Icons

The `Icon` property on `NodeRow` accepts two formats:

### Unicode Characters and Emoji

```csharp
new NodeRow { Icon = "🔑", Label = "Primary Key" }  // key emoji
new NodeRow { Icon = "★", Label = "Favorite" }             // star
new NodeRow { Icon = "fx", Label = "Calculated" }               // plain text
```

Unicode strings are rendered as text at the appropriate icon size.

### SVG Path Data

```csharp
new NodeRow
{
    Icon = "M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2z",
    Label = "Custom Icon"
}
```

SVG path data strings are detected automatically and rendered as nested `<svg>` elements with the path data. You can use path data from any SVG icon set (Material Icons, Heroicons, etc.).

## Async Node Renderer

Use the async variant when your rendering logic needs to fetch data:

```csharp
var options = new DiagramExportOptions
{
    NodeRendererAsync = async nodeModel =>
    {
        var metadata = await _metadataService.GetAsync(nodeModel.Id);
        return new NodeRenderInfo
        {
            HeaderText = metadata.DisplayName,
            HeaderColor = metadata.CategoryColor,
            Sections = BuildSectionsFromMetadata(metadata)
        };
    }
};
```
