# Changelog

All notable changes to FactFoundry.BlazorDiagramExporter are documented in this file.

## v1.0.0

Initial release.

### Features
- SVG-first rendering architecture — builds standalone SVG from diagram model
- Export to SVG for lossless vector output
- Export to PNG via SVG rasterization (configurable scale for retina)
- Export to PDF using built-in minimal PDF builder (no external PDF library)
- `RenderToPngBytesAsync` for raw byte access (integrate with your own PDF library)
- `CreateSnapshotAsync` for testable serializable diagram snapshots
- Custom node rendering via `NodeRenderer`/`NodeRendererAsync` delegates
- Custom link rendering via `LinkRenderer`/`LinkRendererAsync` delegates
- Node sections with icons (SVG path data), port dots, and auto-calculated port Y offsets
- Explicit `PortYOffsets` override for pixel-perfect link endpoints
- Link waypoints for polyline/curved routing
- Cardinality labels (source/target) positioned at link endpoints
- Direction arrows at link midpoint with cubic bezier evaluation
- Bidirectional arrow support
- Optional grid overlay via `GridOptions`
- Icons rendered as nested SVG elements for cross-platform compatibility
- Configurable scale, padding, font family, background color, and file name
- Zero external NuGet dependencies beyond Z.Blazor.Diagrams
- Multi-target: .NET 8.0, .NET 9.0, .NET 10.0
