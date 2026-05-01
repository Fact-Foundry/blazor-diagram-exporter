# FactFoundry.BlazorDiagramExporter

## Overview

**FactFoundry.BlazorDiagramExporter** is an open-source .NET Razor Class Library that exports [Z.Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams) canvases to SVG, PNG, or PDF. It reads directly from the diagram model and builds a standalone SVG, with optional rasterization to PNG/PDF via an offscreen canvas — bypassing the DOM entirely.

The output is pixel-perfect regardless of the current pan/zoom state and captures the entire diagram -- not just the visible viewport area.

## The Problem

`Blazor.Diagrams` renders nodes as HTML/CSS and links as SVG paths inside a viewport with CSS transforms for pan/zoom. The common export workaround -- `html2canvas` -- is fundamentally broken for diagrams:

1. **CSS transforms cause phantom artifacts and incorrect positioning** -- nodes appear offset or duplicated.
2. **SVG elements (links, arrows) are not rendered at all** -- only the nodes appear in the export.
3. **Large diagrams only partially capture** -- content outside the browser's rendering limits is clipped.
4. **Capture is viewport-dependent** -- only what is currently visible on screen gets rendered.

FactFoundry.BlazorDiagramExporter solves all four problems by reading the diagram model directly and building a standalone SVG. PNG and PDF are produced by rasterizing that SVG via an offscreen canvas.

## Features

- SVG-first rendering architecture — builds standalone SVG from diagram model
- Export to **SVG** for lossless vector output
- Export to **PNG** via SVG rasterization (configurable scale for retina)
- Export to **PDF** using a built-in minimal PDF builder (no external PDF library)
- **`RenderToPngBytesAsync`** for raw byte access -- integrate with your own PDF library
- **`CreateSnapshotAsync`** for testable, serializable diagram snapshots
- Dark and light theme support via `BackgroundColor` option
- Custom node rendering via `NodeRenderer` / `NodeRendererAsync` delegates
- Custom link rendering via `LinkRenderer` / `LinkRendererAsync` delegates
- Node sections with icons (SVG path data), port dots, and auto-calculated port Y offsets
- Explicit `PortYOffsets` override for pixel-perfect link endpoints
- Link waypoints for polyline/curved routing
- Cardinality labels (source/target) positioned at link endpoints
- Direction arrows at link midpoint with cubic bezier evaluation
- Bidirectional arrow support
- Optional grid overlay via `GridOptions`
- Configurable scale, padding, font family, and file name
- **Zero external NuGet dependencies** beyond `Z.Blazor.Diagrams`

## Installation

```bash
dotnet add package FactFoundry.BlazorDiagramExporter
```

### Supported Frameworks

| Framework | Status |
|-----------|--------|
| .NET 8.0  | Supported |
| .NET 9.0  | Supported |
| .NET 10.0 | Supported |

### Dependencies

| Package | Version |
|---------|---------|
| `Z.Blazor.Diagrams` | 3.x |

The library has **zero** additional runtime NuGet dependencies. SVG is built entirely in JavaScript; PNG/PDF rasterization uses the browser's built-in Canvas API -- no SkiaSharp, no jsPDF, no third-party rendering libraries.

## Quick Start

Register the exporter in your DI container:

```csharp
builder.Services.AddBlazorDiagramExporter();
```

Inject and use in any Blazor component:

```razor
@using FactFoundry.BlazorDiagramExporter
@inject DiagramExporter Exporter

<button @onclick="ExportSvg">Export SVG</button>
<button @onclick="ExportPng">Export PNG</button>

@code {
    private BlazorDiagram _diagram = new();

    private async Task ExportSvg()
    {
        await Exporter.ExportAsSvgAsync(_diagram);
    }

    private async Task ExportPng()
    {
        await Exporter.ExportAsPngAsync(_diagram);
    }
}
```

See the [Getting Started](getting-started.md) guide for a complete walkthrough.

## Documentation

| Guide | Description |
|-------|-------------|
| [Getting Started](getting-started.md) | Minimal working example, step by step |
| [Export Options](export-options.md) | Full reference for `DiagramExportOptions` |
| [Custom Node Renderer](custom-node-renderer.md) | How to implement `NodeRenderer` with a full working example |
| [Custom Link Renderer](custom-link-renderer.md) | How to implement `LinkRenderer` with a full working example |
| [Diagram Snapshot](diagram-snapshot.md) | What `DiagramSnapshot` is, how to use it for testing and serialization |
| [PDF Export](pdf-export.md) | How PDF export works, limitations, and integration with third-party PDF libraries |
| [Fonts](fonts.md) | How to use custom fonts and browser availability requirements |
| [Orthogonal Routing](orthogonal-routing.md) | Why orthogonal waypoints are not automatic and how to supply them manually |
| [Future Enhancements](future-enhancements.md) | Potential features for future versions |

## License

MIT License. See [LICENSE](../LICENSE) for details.
