# Future Enhancements

Potential features for future versions of FactFoundry.BlazorDiagramExporter.

## Server-Side / Headless Rendering

Render diagrams on the server without a browser (e.g., in a background job, API endpoint, or console application). Currently the library requires a browser context for SVG rasterization to PNG/PDF. `CreateSnapshotAsync` already works without a browser — a headless renderer could consume that snapshot directly.

## Automatic Orthogonal Waypoint Resolution

Automatically compute right-angle link paths to avoid node overlaps during export. Currently, `Blazor.Diagrams` computes orthogonal waypoints at runtime in its rendering pipeline and does not persist them on the link model. The exporter reads from the model, so consumers must supply waypoints themselves. A built-in router that operates on the snapshot could solve this.

See [Orthogonal Routing](orthogonal-routing.md) for the current workaround.

## Animated Export (GIF, Video)

Export diagram animations, transitions, or step-by-step build sequences as GIF or video files. Would require capturing multiple frames over time from different diagram states.

## Custom Font Loading Automation

Automatically download and register fonts so they are available for rendering. Currently, consumers must ensure fonts are loaded via CSS `@font-face` or similar mechanisms before calling export methods.

## Streaming / Progressive Rendering

Render the diagram incrementally or stream partial results for very large diagrams. Could be useful when exporting diagrams with hundreds of nodes where full rendering takes noticeable time.

## React, Angular, or Non-Blazor Framework Support

A standalone JavaScript API that accepts a JSON snapshot and produces SVG/PNG/PDF output, decoupled from the .NET/Blazor ecosystem. The SVG-first architecture makes this feasible since the core rendering logic is already in JavaScript.
