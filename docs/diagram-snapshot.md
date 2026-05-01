# Diagram Snapshot

`DiagramSnapshot` is a serializable, plain-object representation of everything the renderer needs to draw the diagram. It is a first-class citizen of the public API and supports two key use cases: **unit testing without a browser** and **serialization/caching**.

## What Is a Snapshot?

A `DiagramSnapshot` captures the complete rendering data for a diagram at a moment in time:

- All node positions, sizes, and render info (colors, sections, rows, icons)
- All link endpoints, waypoints, and render info (stroke, arrows, labels)
- The computed canvas dimensions (width and height including padding)
- The export options that were used to create the snapshot

It is a plain C# object graph with no dependencies on `BlazorDiagram`, JS interop, or the browser.

## Creating a Snapshot

`CreateSnapshotAsync` is a **static method** on `DiagramExporter`. It does not require JS interop or a browser context:

```csharp
var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);
```

This walks the diagram model, calls your custom renderers (if configured), calculates port Y offsets, computes canvas dimensions, and returns the snapshot.

## DiagramSnapshot Structure

```csharp
public class DiagramSnapshot
{
    public List<SnapshotNode> Nodes { get; set; }
    public List<SnapshotLink> Links { get; set; }
    public DiagramExportOptions Options { get; set; }
    public double CanvasWidth { get; set; }
    public double CanvasHeight { get; set; }
}
```

### SnapshotNode

```csharp
public class SnapshotNode
{
    public string Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public NodeRenderInfo RenderInfo { get; set; }
}
```

### SnapshotLink

```csharp
public class SnapshotLink
{
    public string Id { get; set; }
    public string SourceNodeId { get; set; }
    public string TargetNodeId { get; set; }
    public string? SourcePortId { get; set; }
    public string? TargetPortId { get; set; }
    public List<SnapshotPoint> Waypoints { get; set; }
    public LinkRenderInfo RenderInfo { get; set; }
}
```

### SnapshotPoint

```csharp
public class SnapshotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}
```

## Use Case: Unit Testing

Because `CreateSnapshotAsync` is static and requires no browser, you can unit test your rendering logic without Selenium, Playwright, or bUnit JS mocks:

```csharp
using FactFoundry.BlazorDiagramExporter;
using Xunit;

public class ExportTests
{
    [Fact]
    public async Task Snapshot_CapturesAllNodes()
    {
        // Arrange -- build a diagram with 3 nodes
        var diagram = new BlazorDiagram();
        var node1 = diagram.Nodes.Add(new NodeModel(new Point(0, 0)));
        var node2 = diagram.Nodes.Add(new NodeModel(new Point(300, 0)));
        var node3 = diagram.Nodes.Add(new NodeModel(new Point(150, 200)));

        // Act
        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        // Assert
        Assert.Equal(3, snapshot.Nodes.Count);
    }

    [Fact]
    public async Task Snapshot_PreservesNodePositions()
    {
        var diagram = new BlazorDiagram();
        var node = diagram.Nodes.Add(new NodeModel(new Point(100, 200)));

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram,
            new DiagramExportOptions { Padding = 0 });

        var snapshotNode = snapshot.Nodes[0];
        Assert.Equal(0, snapshotNode.X);  // Normalized to 0 (min X - padding)
        Assert.Equal(0, snapshotNode.Y);  // Normalized to 0 (min Y - padding)
    }

    [Fact]
    public async Task Snapshot_CalculatesCanvasDimensions()
    {
        var diagram = new BlazorDiagram();
        diagram.Nodes.Add(new NodeModel(new Point(0, 0)));
        diagram.Nodes.Add(new NodeModel(new Point(500, 300)));

        var options = new DiagramExportOptions { Padding = 20.0 };
        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

        // Canvas should encompass all nodes plus padding on both sides
        Assert.True(snapshot.CanvasWidth > 0);
        Assert.True(snapshot.CanvasHeight > 0);
    }

    [Fact]
    public async Task Snapshot_CustomNodeRenderer_IsInvoked()
    {
        var diagram = new BlazorDiagram();
        diagram.Nodes.Add(new NodeModel(new Point(0, 0)));

        int callCount = 0;
        var options = new DiagramExportOptions
        {
            NodeRenderer = node =>
            {
                callCount++;
                return new NodeRenderInfo { HeaderText = "Custom" };
            }
        };

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

        Assert.Equal(1, callCount);
        Assert.Equal("Custom", snapshot.Nodes[0].RenderInfo.HeaderText);
    }

    [Fact]
    public async Task Snapshot_EmptyDiagram_ReturnsEmptySnapshot()
    {
        var diagram = new BlazorDiagram();

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        Assert.Empty(snapshot.Nodes);
        Assert.Empty(snapshot.Links);
        Assert.Equal(0, snapshot.CanvasWidth);
        Assert.Equal(0, snapshot.CanvasHeight);
    }
}
```

## Use Case: Serialization and Caching

Since `DiagramSnapshot` is a plain object graph, it can be serialized to JSON for storage or transmission:

```csharp
using System.Text.Json;

// Create snapshot
var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

// Serialize to JSON
string json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
{
    WriteIndented = true
});

// Store, transmit, or cache the JSON string
await File.WriteAllTextAsync("snapshot.json", json);
```

And deserialized later:

```csharp
// Load from JSON
string json = await File.ReadAllTextAsync("snapshot.json");
var snapshot = JsonSerializer.Deserialize<DiagramSnapshot>(json);
```

This enables scenarios such as:
- **Caching** -- store snapshots to avoid recomputing when the diagram has not changed
- **Audit trails** -- save snapshots to record what the diagram looked like at a given time
- **Server-side processing** -- create the snapshot on the client, serialize it, send it to a server for further processing (such as embedding in a report)
- **Diffing** -- compare two snapshots to detect changes

## Coordinate Normalization

When `CreateSnapshotAsync` builds the snapshot, it normalizes all coordinates so that the top-left of the bounding box (plus padding) is at `(0, 0)`. This means:

- Node `X` and `Y` values in the snapshot are relative to the canvas origin, not the diagram's original coordinate space
- Waypoint coordinates are similarly normalized
- `CanvasWidth` and `CanvasHeight` represent the total canvas size needed to render the snapshot

This normalization ensures the exported image always starts at the top-left corner regardless of where nodes were positioned in the original diagram.

## Stability Guarantee

`DiagramSnapshot` and its constituent types (`SnapshotNode`, `SnapshotLink`, `SnapshotPoint`) are part of the stable public API. Breaking changes to these types require a major version bump and prior discussion.
