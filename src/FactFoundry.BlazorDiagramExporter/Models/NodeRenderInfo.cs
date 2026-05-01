namespace FactFoundry.BlazorDiagramExporter;

public class NodeRenderInfo
{
    public string HeaderText { get; set; } = string.Empty;

    public string? BodyText { get; set; }

    public string BodyTextColor { get; set; } = "#555555";

    public string HeaderColor { get; set; } = "#4a6fa5";

    public string HeaderTextColor { get; set; } = "#ffffff";

    public string BodyColor { get; set; } = "#ffffff";

    public string BorderColor { get; set; } = "#cccccc";

    public double BorderRadius { get; set; } = 8.0;

    public List<NodeSection> Sections { get; set; } = new();

    public Dictionary<string, double>? PortYOffsets { get; set; }
}

public class NodeSection
{
    public string? SectionLabel { get; set; }

    public List<NodeRow> Rows { get; set; } = new();
}

public class NodeRow
{
    public string? Icon { get; set; }

    public string Label { get; set; } = string.Empty;

    public string? SecondaryText { get; set; }

    public string TextColor { get; set; } = "#1f2937";

    public string? RowBackgroundColor { get; set; }

    public double RowHeight { get; set; } = 28.0;

    public string? PortId { get; set; }
}
