# FactFoundry.BlazorDiagramExporter

[![NuGet](https://img.shields.io/nuget/v/FactFoundry.BlazorDiagramExporter.svg)](https://www.nuget.org/packages/FactFoundry.BlazorDiagramExporter)
[![CI](https://github.com/FactFoundry/blazor-diagram-exporter/actions/workflows/ci.yml/badge.svg)](https://github.com/FactFoundry/blazor-diagram-exporter/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Export [Z.Blazor.Diagrams](https://github.com/Blazor-Diagrams/Blazor.Diagrams) canvases to SVG, PNG, and PDF by rendering directly from the diagram model — bypassing the DOM entirely. Uses an SVG-first architecture: builds a complete SVG from the diagram snapshot, then optionally rasterizes to PNG or PDF. The output is pixel-perfect regardless of the current pan/zoom state and captures the entire diagram, not just the visible viewport. Zero external NuGet dependencies.

## The Problem

`Blazor.Diagrams` renders nodes as HTML/CSS and links as SVG paths inside a viewport with CSS transforms for pan/zoom. The common export workaround -- `html2canvas` -- is fundamentally broken for diagrams:

- **CSS transforms cause phantom artifacts and incorrect positioning** -- nodes appear offset or duplicated
- **SVG elements (links, arrows) are not rendered at all** -- only the nodes appear in the export
- **Large diagrams only partially capture** -- content outside the browser's rendering limits is clipped
- **Capture is viewport-dependent** -- only what is currently visible on screen gets rendered

FactFoundry.BlazorDiagramExporter solves all four problems by reading the diagram model directly and building a standalone SVG. PNG and PDF are produced by rasterizing that SVG via an offscreen canvas.

## Installation

```bash
dotnet add package FactFoundry.BlazorDiagramExporter
```

**Supported frameworks:** .NET 8.0, .NET 9.0, .NET 10.0

**Dependencies:** [Z.Blazor.Diagrams](https://www.nuget.org/packages/Z.Blazor.Diagrams) 3.x (tested minimum version)

## Quick Start

Register the exporter in `Program.cs`:

```csharp
using FactFoundry.BlazorDiagramExporter;

builder.Services.AddBlazorDiagramExporter();
```

Inject and use in a Blazor component:

```razor
@using FactFoundry.BlazorDiagramExporter
@inject DiagramExporter Exporter

<button @onclick="ExportSvg">Export SVG</button>
<button @onclick="ExportPng">Export PNG</button>

@code {
    private BlazorDiagram _diagram = new();

    private async Task ExportSvg()
    {
        await Exporter.ExportAsSvgAsync(_diagram, new DiagramExportOptions
        {
            FileName = "my-diagram"
        });
    }

    private async Task ExportPng()
    {
        await Exporter.ExportAsPngAsync(_diagram, new DiagramExportOptions
        {
            Scale = 2.0,
            FileName = "my-diagram"
        });
    }
}
```

## Export Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `Padding` | `double` | `20.0` | Padding in pixels around the diagram bounding box |
| `BackgroundColor` | `string` | `"#ffffff"` | Background color (any CSS color string) |
| `Scale` | `double` | `1.0` | Scale factor for PNG export (`2.0` = retina) |
| `FontFamily` | `string` | `"Arial"` | Font family for all text rendering |
| `FileName` | `string` | `"diagram"` | File name for browser downloads (without extension) |
| `NodeRenderer` | `Func<NodeModel, NodeRenderInfo>?` | `null` | Custom node renderer (sync) |
| `NodeRendererAsync` | `Func<NodeModel, Task<NodeRenderInfo>>?` | `null` | Custom node renderer (async, takes precedence) |
| `LinkRenderer` | `Func<BaseLinkModel, LinkRenderInfo>?` | `null` | Custom link renderer (sync) |
| `LinkRendererAsync` | `Func<BaseLinkModel, Task<LinkRenderInfo>>?` | `null` | Custom link renderer (async, takes precedence) |
| `Grid` | `GridOptions?` | `null` | Optional grid overlay (set `Enabled = true` to render) |

## API Methods

| Method | Description |
|--------|-------------|
| `ExportAsSvgAsync(diagram, options?)` | Triggers an SVG file download in the browser |
| `ExportAsPngAsync(diagram, options?)` | Triggers a PNG file download in the browser |
| `ExportAsPdfAsync(diagram, options?)` | Triggers a PDF file download in the browser |
| `RenderToPngBytesAsync(diagram, options?)` | Returns raw PNG bytes for custom processing |
| `DiagramExporter.CreateSnapshotAsync(diagram, options?)` | Static. Creates a serializable `DiagramSnapshot` without JS interop |

## Orthogonal Routing

> **Important:** Orthogonal (right-angle) link routing is **not automatic**. `Blazor.Diagrams` computes orthogonal waypoints at runtime in its rendering pipeline and does not persist them on the link model. The exporter reads from the model, so links without waypoints are drawn as straight lines. If you need orthogonal routing in exports, you must supply waypoints yourself via link vertices. See [docs/orthogonal-routing.md](docs/orthogonal-routing.md) for details and code examples.

## PDF Export

The library includes a built-in minimal PDF builder that requires **no external PDF library**. It creates a single-page PDF with the diagram rendered as an embedded PNG image.

For advanced PDF features (multi-page, text layers, metadata), call `RenderToPngBytesAsync` and pass the bytes to your preferred PDF library (QuestPDF, iText, PDFSharp, etc.). See [docs/pdf-export.md](docs/pdf-export.md) for examples.

## Documentation

Full documentation is available in the [`/docs`](docs/) directory:

- [Overview & Installation](docs/index.md)
- [Getting Started](docs/getting-started.md)
- [Export Options Reference](docs/export-options.md)
- [Custom Node Renderer](docs/custom-node-renderer.md)
- [Custom Link Renderer](docs/custom-link-renderer.md)
- [Diagram Snapshot](docs/diagram-snapshot.md)
- [PDF Export](docs/pdf-export.md)
- [Fonts](docs/fonts.md)
- [Orthogonal Routing](docs/orthogonal-routing.md)
- [Future Enhancements](docs/future-enhancements.md)

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on building, testing, and submitting pull requests.

## License

MIT License. See [LICENSE](LICENSE) for details.
