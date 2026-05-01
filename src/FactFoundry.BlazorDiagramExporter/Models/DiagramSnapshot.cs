namespace FactFoundry.BlazorDiagramExporter;

public class DiagramSnapshot
{
    public List<SnapshotNode> Nodes { get; set; } = new();

    public List<SnapshotLink> Links { get; set; } = new();

    public DiagramExportOptions Options { get; set; } = new();

    public double CanvasWidth { get; set; }

    public double CanvasHeight { get; set; }

    public double OffsetX { get; set; }

    public double OffsetY { get; set; }
}

public class SnapshotNode
{
    public string Id { get; set; } = string.Empty;

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public NodeRenderInfo RenderInfo { get; set; } = new();
}

public class SnapshotLink
{
    public string Id { get; set; } = string.Empty;

    public string SourceNodeId { get; set; } = string.Empty;

    public string TargetNodeId { get; set; } = string.Empty;

    public string? SourcePortId { get; set; }

    public string? TargetPortId { get; set; }

    public string? SvgPath { get; set; }

    public List<SnapshotPoint> Waypoints { get; set; } = new();

    public LinkRenderInfo RenderInfo { get; set; } = new();
}

public class SnapshotPoint
{
    public double X { get; set; }

    public double Y { get; set; }
}
