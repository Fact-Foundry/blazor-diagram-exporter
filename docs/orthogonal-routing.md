# Orthogonal Routing

This page explains an important limitation of the exporter regarding orthogonal (right-angle) link routing.

## The Limitation

**FactFoundry.BlazorDiagramExporter does not automatically compute orthogonal waypoints.** If you do not supply waypoints, links are drawn as straight lines between their source and target port positions.

This is not a bug -- it is a deliberate design decision driven by how `Blazor.Diagrams` works.

## Why Orthogonal Routing Is Not Automatic

`Blazor.Diagrams` supports orthogonal routing through its router system. However, the router computes waypoints **at runtime in the browser** as part of the rendering pipeline. These computed waypoints are **not persisted** on the link model -- they exist only in the rendering layer.

This means:

1. When you inspect a `BaseLinkModel` in C#, there are no orthogonal waypoints available.
2. The link's `Vertices` collection contains only manually-added vertices, not router-computed waypoints.
3. The exporter reads from the model, not the DOM, so it cannot access the router's computed path.

Since the exporter bypasses the DOM entirely (which is what makes it work correctly unlike `html2canvas`), it also bypasses the router.

## What Happens Without Waypoints

When a link has no waypoints in the snapshot, the exporter draws a **straight line** from the source port position to the target port position:

```
[Source Node]-----------------------------[Target Node]
     port                                      port
```

## Supplying Waypoints Manually

If your application needs orthogonal routing in exports, you must supply the waypoints yourself. You can do this by adding vertices to the link model, which the exporter reads during snapshot creation.

### Option 1: Add Vertices to Links

`Blazor.Diagrams` links support manual vertices. Add them to your link to create a waypointed path:

```csharp
var link = diagram.Links.Add(new LinkModel(sourcePort, targetPort));

// Add vertices to create an orthogonal path
link.Vertices.Add(new LinkVertexModel(link, new Point(sourcePort.Position.X + 50, sourcePort.Position.Y)));
link.Vertices.Add(new LinkVertexModel(link, new Point(sourcePort.Position.X + 50, targetPort.Position.Y)));
```

The exporter reads `link.Vertices` and converts them to `SnapshotLink.Waypoints`.

### Option 2: Compute Waypoints in Your Renderer

If you want to compute orthogonal paths programmatically, you can do so before exporting:

```csharp
private void AddOrthogonalWaypoints(BlazorDiagram diagram)
{
    foreach (var link in diagram.Links)
    {
        var sourcePort = (link.Source as SinglePortAnchor)?.Port;
        var targetPort = (link.Target as SinglePortAnchor)?.Port;
        if (sourcePort == null || targetPort == null) continue;

        var sx = sourcePort.Position.X;
        var sy = sourcePort.Position.Y;
        var tx = targetPort.Position.X;
        var ty = targetPort.Position.Y;

        // Simple L-shaped orthogonal route
        var midX = (sx + tx) / 2.0;

        link.Vertices.Clear();
        link.Vertices.Add(new LinkVertexModel(link, new Point(midX, sy)));
        link.Vertices.Add(new LinkVertexModel(link, new Point(midX, ty)));
    }
}

// Call before exporting
AddOrthogonalWaypoints(diagram);
await Exporter.ExportAsPngAsync(diagram, options);
```

### Option 3: Post-Process the Snapshot

You can also create a snapshot first and then add waypoints to the snapshot links before rendering:

```csharp
var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

foreach (var link in snapshot.Links)
{
    var sourceNode = snapshot.Nodes.FirstOrDefault(n => n.Id == link.SourceNodeId);
    var targetNode = snapshot.Nodes.FirstOrDefault(n => n.Id == link.TargetNodeId);
    if (sourceNode == null || targetNode == null) continue;

    // Compute orthogonal waypoints based on node positions
    var midX = (sourceNode.X + sourceNode.Width + targetNode.X) / 2.0;
    var sourceY = sourceNode.Y + sourceNode.Height / 2.0;
    var targetY = targetNode.Y + targetNode.Height / 2.0;

    link.Waypoints.Clear();
    link.Waypoints.Add(new SnapshotPoint { X = midX, Y = sourceY });
    link.Waypoints.Add(new SnapshotPoint { X = midX, Y = targetY });
}

// Then pass the snapshot to the JS interop for rendering
// (This requires using the lower-level API)
```

## Orthogonal Routing Result

With properly supplied waypoints, the rendered link follows a right-angle path:

```
[Source Node]---+
     port       |
                |
                +---[Target Node]
                         port
```

## Why Not Extract Waypoints from the DOM?

One might think the exporter could read the rendered SVG path from the DOM to capture the router's computed waypoints. This approach was rejected because:

1. **It would reintroduce DOM dependency** -- the whole point of this library is to bypass the DOM.
2. **Viewport dependency** -- the DOM only contains paths for links that are currently rendered in the viewport.
3. **Timing issues** -- the router may not have computed paths yet, or paths may be stale.
4. **Consistency** -- reading from the model produces deterministic, reproducible results regardless of viewport state.

## Summary

| Scenario | Result |
|----------|--------|
| No waypoints on link | Straight line between ports |
| Manual vertices on link | Polyline through the vertex positions |
| Orthogonal routing desired | You must supply waypoints (vertices) yourself |
| Router-computed paths | Not accessible -- they are not persisted on the model |
