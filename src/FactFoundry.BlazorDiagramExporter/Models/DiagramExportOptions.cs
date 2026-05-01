using System.Text.Json.Serialization;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;

namespace FactFoundry.BlazorDiagramExporter;

public class DiagramExportOptions
{
    public double Padding { get; set; } = 20.0;

    public string BackgroundColor { get; set; } = "#ffffff";

    public double Scale { get; set; } = 1.0;

    public string FontFamily { get; set; } = "Arial";

    public string FileName { get; set; } = "diagram";

    public GridOptions? Grid { get; set; }

    [JsonIgnore]
    public Func<NodeModel, NodeRenderInfo>? NodeRenderer { get; set; }

    [JsonIgnore]
    public Func<NodeModel, Task<NodeRenderInfo>>? NodeRendererAsync { get; set; }

    [JsonIgnore]
    public Func<BaseLinkModel, LinkRenderInfo>? LinkRenderer { get; set; }

    [JsonIgnore]
    public Func<BaseLinkModel, Task<LinkRenderInfo>>? LinkRendererAsync { get; set; }
}

public class GridOptions
{
    public bool Enabled { get; set; }

    public double Spacing { get; set; } = 20.0;

    public string Color { get; set; } = "rgba(255,255,255,0.12)";

    public double LineWidth { get; set; } = 1.0;
}
