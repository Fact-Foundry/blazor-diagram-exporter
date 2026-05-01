# Getting Started

This guide walks you through adding FactFoundry.BlazorDiagramExporter to a Blazor application and exporting your first diagram.

## Prerequisites

- .NET 8.0, 9.0, or 10.0 SDK
- A Blazor application (WebAssembly, Server, or Hybrid)
- An existing `Z.Blazor.Diagrams` (`BlazorDiagram`) instance in your application

## Step 1: Install the Package

```bash
dotnet add package FactFoundry.BlazorDiagramExporter
```

## Step 2: Register Services

In your `Program.cs`, register the exporter with the dependency injection container:

```csharp
using FactFoundry.BlazorDiagramExporter;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// ... your other service registrations ...

builder.Services.AddBlazorDiagramExporter();

await builder.Build().RunAsync();
```

For Blazor Server, the registration is the same:

```csharp
using FactFoundry.BlazorDiagramExporter;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddBlazorDiagramExporter();

// ... rest of your setup ...
```

## Step 3: Create a Diagram

If you do not already have a diagram, here is a minimal example that creates a `BlazorDiagram` with two nodes and a link:

```csharp
@using Blazor.Diagrams
@using Blazor.Diagrams.Core.Models

@code {
    private BlazorDiagram _diagram = new();

    protected override void OnInitialized()
    {
        var node1 = _diagram.Nodes.Add(new NodeModel(new Point(50, 50)));
        node1.Title = "Customer";
        node1.AddPort(PortAlignment.Right);

        var node2 = _diagram.Nodes.Add(new NodeModel(new Point(350, 50)));
        node2.Title = "Order";
        node2.AddPort(PortAlignment.Left);

        _diagram.Links.Add(new LinkModel(node1.Ports[0], node2.Ports[0]));
    }
}
```

## Step 4: Inject the Exporter

Inject `DiagramExporter` into your Blazor component:

```razor
@using FactFoundry.BlazorDiagramExporter
@inject DiagramExporter Exporter
```

## Step 5: Export the Diagram

### Export as PNG (browser download)

```razor
<button @onclick="ExportPng">Export PNG</button>

@code {
    private async Task ExportPng()
    {
        await Exporter.ExportAsPngAsync(_diagram);
    }
}
```

This triggers a browser file download of `diagram.png`.

### Export as PDF (browser download)

```razor
<button @onclick="ExportPdf">Export PDF</button>

@code {
    private async Task ExportPdf()
    {
        await Exporter.ExportAsPdfAsync(_diagram);
    }
}
```

### Export with options

```razor
<button @onclick="ExportCustom">Export Custom PNG</button>

@code {
    private async Task ExportCustom()
    {
        var options = new DiagramExportOptions
        {
            Scale = 2.0,
            Padding = 40.0,
            BackgroundColor = "#1e1e2e",
            FontFamily = "Cascadia Code, Consolas, monospace",
            FileName = "my-diagram"
        };

        await Exporter.ExportAsPngAsync(_diagram, options);
    }
}
```

## Step 6: Get Raw PNG Bytes (Optional)

If you need the PNG data for further processing (such as passing to a third-party PDF library, uploading to a server, or displaying in a preview), use `RenderToPngBytesAsync`:

```csharp
byte[] pngBytes = await Exporter.RenderToPngBytesAsync(_diagram, options);

// Use pngBytes however you need -- upload, embed, convert, etc.
```

## Complete Minimal Example

Here is a complete, self-contained Blazor component:

```razor
@page "/diagram-export"
@using Blazor.Diagrams
@using Blazor.Diagrams.Core.Models
@using FactFoundry.BlazorDiagramExporter
@inject DiagramExporter Exporter

<h3>Diagram Export Demo</h3>

<div style="display: flex; gap: 8px; margin-bottom: 16px;">
    <button @onclick="ExportPng">Export PNG</button>
    <button @onclick="ExportPdf">Export PDF</button>
</div>

<CascadingValue Value="_diagram">
    <DiagramCanvas></DiagramCanvas>
</CascadingValue>

@code {
    private BlazorDiagram _diagram = new();

    protected override void OnInitialized()
    {
        var customer = _diagram.Nodes.Add(new NodeModel(new Point(50, 50)));
        customer.Title = "Customer";
        var customerPort = customer.AddPort(PortAlignment.Right);

        var order = _diagram.Nodes.Add(new NodeModel(new Point(350, 50)));
        order.Title = "Order";
        var orderPort = order.AddPort(PortAlignment.Left);

        _diagram.Links.Add(new LinkModel(customerPort, orderPort));
    }

    private async Task ExportPng()
    {
        await Exporter.ExportAsPngAsync(_diagram, new DiagramExportOptions
        {
            FileName = "my-diagram",
            Scale = 2.0
        });
    }

    private async Task ExportPdf()
    {
        await Exporter.ExportAsPdfAsync(_diagram, new DiagramExportOptions
        {
            FileName = "my-diagram"
        });
    }
}
```

## Next Steps

- [Export Options](export-options.md) -- configure padding, scale, colors, fonts, and file names
- [Custom Node Renderer](custom-node-renderer.md) -- control how each node appears in the export
- [Custom Link Renderer](custom-link-renderer.md) -- control link colors, dash patterns, and arrowheads
- [Diagram Snapshot](diagram-snapshot.md) -- use snapshots for testing and serialization
