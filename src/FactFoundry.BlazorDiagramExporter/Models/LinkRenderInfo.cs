namespace FactFoundry.BlazorDiagramExporter;

public class LinkRenderInfo
{
    public string StrokeColor { get; set; } = "#6b7280";

    public double StrokeWidth { get; set; } = 2.0;

    public double[] DashPattern { get; set; } = Array.Empty<double>();

    public ArrowStyle SourceArrow { get; set; } = ArrowStyle.None;

    public ArrowStyle TargetArrow { get; set; } = ArrowStyle.None;

    public string? Label { get; set; }

    public string LabelColor { get; set; } = "#374151";

    public string LabelBackgroundColor { get; set; } = "#ffffff";

    public string? SourceLabel { get; set; }

    public string? TargetLabel { get; set; }

    public bool ShowDirectionArrow { get; set; }

    public bool IsBidirectional { get; set; }
}

public enum ArrowStyle
{
    None,
    Arrow,
    FilledArrow,
    Diamond,
    FilledDiamond
}
