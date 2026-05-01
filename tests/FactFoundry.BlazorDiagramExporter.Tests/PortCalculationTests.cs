namespace FactFoundry.BlazorDiagramExporter.Tests;

public class PortCalculationTests
{
    private const double HeaderHeight = 32.0;
    private const double SectionLabelHeight = 24.0;
    private const double DefaultRowHeight = 28.0;

    [Fact]
    public void PortYOffset_AutoCalculated_FromSectionRows()
    {
        var renderInfo = new NodeRenderInfo();
        var section = new NodeSection();
        section.Rows.Add(new NodeRow { Label = "Col1", PortId = "port1" });
        section.Rows.Add(new NodeRow { Label = "Col2", PortId = "port2" });
        renderInfo.Sections.Add(section);

        var offset = DiagramExporter.CalculatePortYOffset(renderInfo, "port2", 200);

        var expected = HeaderHeight + DefaultRowHeight + (DefaultRowHeight / 2.0);
        Assert.Equal(expected, offset);
    }

    [Fact]
    public void PortYOffset_ExplicitOverride_TakesPrecedence()
    {
        var renderInfo = new NodeRenderInfo
        {
            PortYOffsets = new Dictionary<string, double> { ["port1"] = 99.0 }
        };
        var section = new NodeSection();
        section.Rows.Add(new NodeRow { Label = "Col1", PortId = "port1" });
        renderInfo.Sections.Add(section);

        var offset = DiagramExporter.CalculatePortYOffset(renderInfo, "port1", 200);

        Assert.Equal(99.0, offset);
    }

    [Fact]
    public void PortYOffset_NoMatch_FallsBackToNodeCenter()
    {
        var renderInfo = new NodeRenderInfo();
        var section = new NodeSection();
        section.Rows.Add(new NodeRow { Label = "Col1", PortId = "port1" });
        renderInfo.Sections.Add(section);

        var offset = DiagramExporter.CalculatePortYOffset(renderInfo, "unknown-port", 200);

        Assert.Equal(100.0, offset);
    }

    [Fact]
    public void PortYOffset_MultipleSections_AccumulatesHeight()
    {
        var renderInfo = new NodeRenderInfo();

        var section1 = new NodeSection { SectionLabel = "Columns" };
        section1.Rows.Add(new NodeRow { Label = "Col1", PortId = "port1" });
        section1.Rows.Add(new NodeRow { Label = "Col2", PortId = "port2" });
        renderInfo.Sections.Add(section1);

        var section2 = new NodeSection { SectionLabel = "Measures" };
        section2.Rows.Add(new NodeRow { Label = "Measure1", PortId = "port3" });
        renderInfo.Sections.Add(section2);

        var offset = DiagramExporter.CalculatePortYOffset(renderInfo, "port3", 300);

        var expected = HeaderHeight
            + SectionLabelHeight           // "Columns" label
            + DefaultRowHeight             // Col1
            + DefaultRowHeight             // Col2
            + SectionLabelHeight           // "Measures" label
            + (DefaultRowHeight / 2.0);    // center of Measure1

        Assert.Equal(expected, offset);
    }

    [Fact]
    public void PortYOffset_CustomRowHeight_Respected()
    {
        var renderInfo = new NodeRenderInfo();
        var section = new NodeSection();
        section.Rows.Add(new NodeRow { Label = "Col1", PortId = "port1", RowHeight = 40.0 });
        section.Rows.Add(new NodeRow { Label = "Col2", PortId = "port2", RowHeight = 50.0 });
        renderInfo.Sections.Add(section);

        var offset = DiagramExporter.CalculatePortYOffset(renderInfo, "port2", 200);

        var expected = HeaderHeight + 40.0 + (50.0 / 2.0);
        Assert.Equal(expected, offset);
    }
}
