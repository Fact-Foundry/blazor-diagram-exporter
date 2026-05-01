using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Options;

namespace FactFoundry.BlazorDiagramExporter.Tests.Helpers;

public class TestDiagramBuilder
{
    private readonly BlazorDiagram _diagram = new(new BlazorDiagramOptions(), false);
    private readonly Dictionary<string, NodeModel> _nodes = new();
    private readonly Dictionary<string, PortModel> _ports = new();

    public TestDiagramBuilder AddNode(
        string name,
        double x = 0,
        double y = 0,
        double width = 250,
        double height = 200,
        (string Name, string DataType, bool IsKey)[]? columns = null)
    {
        var node = new NodeModel(name, new Point(x, y))
        {
            Title = name,
            Size = new Size(width, height)
        };
        _diagram.Nodes.Add(node);
        _nodes[name] = node;

        if (columns is not null)
        {
            foreach (var (colName, _, _) in columns)
            {
                var portId = $"{name}.{colName}";
                var port = node.AddPort(new PortModel(portId, node, PortAlignment.Right, new Point(0, 0), new Size(1, 1)));
                _ports[portId] = port;
            }
        }

        return this;
    }

    public TestDiagramBuilder AddLink(
        string sourceNode,
        string sourceColumn,
        string targetNode,
        string targetColumn)
    {
        var sourcePortId = $"{sourceNode}.{sourceColumn}";
        var targetPortId = $"{targetNode}.{targetColumn}";

        if (!_ports.TryGetValue(sourcePortId, out var sourcePort))
            throw new ArgumentException($"Port {sourcePortId} not found");
        if (!_ports.TryGetValue(targetPortId, out var targetPort))
            throw new ArgumentException($"Port {targetPortId} not found");

        var link = new LinkModel(sourcePort, targetPort);
        _diagram.Links.Add(link);
        return this;
    }

    public BlazorDiagram Build() => _diagram;
}
