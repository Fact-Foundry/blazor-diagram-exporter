using Blazor.Diagrams;
using Blazor.Diagrams.Options;
using FactFoundry.BlazorDiagramExporter.Tests.Helpers;

namespace FactFoundry.BlazorDiagramExporter.Tests;

public class SnapshotCreationTests
{
    [Fact]
    public async Task CreateSnapshot_WithNodes_CapturesAllNodes()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("Sales", x: 0, y: 0, width: 250, height: 200,
                columns: [("SalesKey", "Int64", true), ("Amount", "Decimal", false)])
            .AddNode("Product", x: 300, y: 0, width: 250, height: 150,
                columns: [("ProductKey", "Int64", true), ("Name", "String", false)])
            .Build();

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        Assert.Equal(2, snapshot.Nodes.Count);
    }

    [Fact]
    public async Task CreateSnapshot_WithLinks_CapturesAllLinks()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("Sales", x: 0, y: 0, width: 250, height: 200,
                columns: [("ProductKey", "Int64", false)])
            .AddNode("Product", x: 300, y: 0, width: 250, height: 150,
                columns: [("ProductKey", "Int64", true)])
            .AddLink("Sales", "ProductKey", "Product", "ProductKey")
            .Build();

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        Assert.Single(snapshot.Links);
    }

    [Fact]
    public async Task CreateSnapshot_EmptyDiagram_ReturnsEmptySnapshot()
    {
        var diagram = new BlazorDiagram(new BlazorDiagramOptions(), false);

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        Assert.Empty(snapshot.Nodes);
        Assert.Empty(snapshot.Links);
        Assert.Equal(0, snapshot.CanvasWidth);
        Assert.Equal(0, snapshot.CanvasHeight);
    }

    [Fact]
    public async Task CreateSnapshot_CalculatesCanvasDimensions()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("A", x: 100, y: 50, width: 200, height: 100)
            .AddNode("B", x: 400, y: 200, width: 200, height: 100)
            .Build();

        var options = new DiagramExportOptions { Padding = 20 };
        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

        Assert.Equal(540, snapshot.CanvasWidth);
        Assert.Equal(290, snapshot.CanvasHeight);
    }

    [Fact]
    public async Task CreateSnapshot_NodePositionsPreserved()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("Sales", x: 150, y: 75, width: 250, height: 200)
            .Build();

        var options = new DiagramExportOptions { Padding = 0 };
        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

        var node = Assert.Single(snapshot.Nodes);
        Assert.Equal(0, node.X);
        Assert.Equal(0, node.Y);
        Assert.Equal(250, node.Width);
        Assert.Equal(200, node.Height);
    }

    [Fact]
    public async Task CreateSnapshot_LinkEndpointsResolved()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("Sales", x: 0, y: 0, width: 250, height: 200,
                columns: [("ProductKey", "Int64", false)])
            .AddNode("Product", x: 300, y: 0, width: 250, height: 150,
                columns: [("ProductKey", "Int64", true)])
            .AddLink("Sales", "ProductKey", "Product", "ProductKey")
            .Build();

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        var link = Assert.Single(snapshot.Links);
        Assert.Equal("Sales", link.SourceNodeId);
        Assert.Equal("Product", link.TargetNodeId);
        Assert.Equal("Sales.ProductKey", link.SourcePortId);
        Assert.Equal("Product.ProductKey", link.TargetPortId);
    }
}
