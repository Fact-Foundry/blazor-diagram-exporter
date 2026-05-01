using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using FactFoundry.BlazorDiagramExporter.Tests.Helpers;

namespace FactFoundry.BlazorDiagramExporter.Tests;

public class DefaultRendererTests
{
    [Fact]
    public async Task DefaultNodeRenderer_ProducesValidRenderInfo()
    {
        var diagram = new TestDiagramBuilder()
            .AddNode("Sales", x: 0, y: 0, width: 250, height: 200,
                columns: [("SalesKey", "Int64", true), ("Amount", "Decimal", false)])
            .Build();

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram);

        var node = Assert.Single(snapshot.Nodes);
        Assert.NotNull(node.RenderInfo);
        Assert.Equal("Sales", node.RenderInfo.HeaderText);
        Assert.NotEmpty(node.RenderInfo.Sections);
    }

    [Fact]
    public async Task DefaultLinkRenderer_ProducesValidRenderInfo()
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
        Assert.NotNull(link.RenderInfo);
        Assert.NotNull(link.RenderInfo.StrokeColor);
        Assert.True(Enum.IsDefined(link.RenderInfo.TargetArrow));
    }

    [Fact]
    public async Task CustomNodeRenderer_CalledForEachNode()
    {
        var callCount = 0;
        var diagram = new TestDiagramBuilder()
            .AddNode("A", x: 0, y: 0)
            .AddNode("B", x: 300, y: 0)
            .AddNode("C", x: 600, y: 0)
            .Build();

        var options = new DiagramExportOptions
        {
            NodeRenderer = node =>
            {
                callCount++;
                return new NodeRenderInfo { HeaderText = node.Title ?? node.Id };
            }
        };

        await DiagramExporter.CreateSnapshotAsync(diagram, options);

        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task CustomNodeRendererAsync_TakesPrecedence()
    {
        var syncCalled = false;
        var asyncCalled = false;

        var diagram = new TestDiagramBuilder()
            .AddNode("A", x: 0, y: 0)
            .Build();

        var options = new DiagramExportOptions
        {
            NodeRenderer = _ =>
            {
                syncCalled = true;
                return new NodeRenderInfo();
            },
            NodeRendererAsync = _ =>
            {
                asyncCalled = true;
                return Task.FromResult(new NodeRenderInfo { HeaderText = "Async" });
            }
        };

        var snapshot = await DiagramExporter.CreateSnapshotAsync(diagram, options);

        Assert.False(syncCalled);
        Assert.True(asyncCalled);
        Assert.Equal("Async", snapshot.Nodes[0].RenderInfo.HeaderText);
    }

    [Fact]
    public async Task CustomLinkRenderer_CalledForEachLink()
    {
        var callCount = 0;
        var diagram = new TestDiagramBuilder()
            .AddNode("A", x: 0, y: 0, columns: [("Key", "Int64", true)])
            .AddNode("B", x: 300, y: 0, columns: [("Key", "Int64", true)])
            .AddNode("C", x: 600, y: 0, columns: [("Key", "Int64", true)])
            .AddLink("A", "Key", "B", "Key")
            .AddLink("B", "Key", "C", "Key")
            .Build();

        var options = new DiagramExportOptions
        {
            LinkRenderer = _ =>
            {
                callCount++;
                return new LinkRenderInfo();
            }
        };

        await DiagramExporter.CreateSnapshotAsync(diagram, options);

        Assert.Equal(2, callCount);
    }
}
