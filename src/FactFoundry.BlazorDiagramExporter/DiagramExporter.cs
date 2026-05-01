using Blazor.Diagrams;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using LinkMarker = Blazor.Diagrams.Core.Models.LinkMarker;

namespace FactFoundry.BlazorDiagramExporter;

public sealed class DiagramExporter : IAsyncDisposable
{
    private readonly ExporterJsInterop _jsInterop;

    internal DiagramExporter(ExporterJsInterop jsInterop)
    {
        _jsInterop = jsInterop;
    }

    public async Task<byte[]> RenderToPngBytesAsync(
        BlazorDiagram diagram,
        DiagramExportOptions? options = null)
    {
        var snapshot = await CreateSnapshotAsync(diagram, options);
        return await _jsInterop.RenderToPngBytesAsync(snapshot);
    }

    public async Task ExportAsPngAsync(
        BlazorDiagram diagram,
        DiagramExportOptions? options = null)
    {
        options ??= new DiagramExportOptions();
        var snapshot = await CreateSnapshotAsync(diagram, options);
        await _jsInterop.ExportAsPngAsync(snapshot, options.FileName);
    }

    public async Task ExportAsPdfAsync(
        BlazorDiagram diagram,
        DiagramExportOptions? options = null)
    {
        options ??= new DiagramExportOptions();
        var snapshot = await CreateSnapshotAsync(diagram, options);
        await _jsInterop.ExportAsPdfAsync(snapshot, options.FileName);
    }

    public async Task ExportAsSvgAsync(
        BlazorDiagram diagram,
        DiagramExportOptions? options = null)
    {
        options ??= new DiagramExportOptions();
        var snapshot = await CreateSnapshotAsync(diagram, options);
        await _jsInterop.ExportAsSvgAsync(snapshot, options.FileName);
    }

    public static async Task<DiagramSnapshot> CreateSnapshotAsync(
        BlazorDiagram diagram,
        DiagramExportOptions? options = null)
    {
        options ??= new DiagramExportOptions();
        var snapshot = new DiagramSnapshot { Options = options };

        foreach (var node in diagram.Nodes)
        {
            var renderInfo = await GetNodeRenderInfoAsync(node, options);
            var snapshotNode = new SnapshotNode
            {
                Id = node.Id,
                X = node.Position.X,
                Y = node.Position.Y,
                Width = node.Size?.Width ?? 200,
                Height = node.Size?.Height ?? 100,
                RenderInfo = renderInfo
            };
            snapshot.Nodes.Add(snapshotNode);
        }

        foreach (var link in diagram.Links)
        {
            var renderInfo = await GetLinkRenderInfoAsync(link, options);

            var sourcePort = (link.Source as SinglePortAnchor)?.Port;
            var targetPort = (link.Target as SinglePortAnchor)?.Port;
            var sourceNode = sourcePort?.Parent;
            var targetNode = targetPort?.Parent;

            var snapshotLink = new SnapshotLink
            {
                Id = link.Id,
                SourceNodeId = sourceNode?.Id ?? string.Empty,
                TargetNodeId = targetNode?.Id ?? string.Empty,
                SourcePortId = sourcePort?.Id,
                TargetPortId = targetPort?.Id,
                SvgPath = link.PathGeneratorResult?.FullPath?.ToString(),
                RenderInfo = renderInfo
            };

            if (link.Route is { Length: > 0 } route)
            {
                foreach (var point in route)
                {
                    snapshotLink.Waypoints.Add(new SnapshotPoint
                    {
                        X = point.X,
                        Y = point.Y
                    });
                }
            }
            else
            {
                foreach (var vertex in link.Vertices)
                {
                    snapshotLink.Waypoints.Add(new SnapshotPoint
                    {
                        X = vertex.Position.X,
                        Y = vertex.Position.Y
                    });
                }
            }

            snapshot.Links.Add(snapshotLink);
        }

        ComputeCanvasDimensions(snapshot, options.Padding);
        return snapshot;
    }

    private static async Task<NodeRenderInfo> GetNodeRenderInfoAsync(
        NodeModel node, DiagramExportOptions options)
    {
        if (options.NodeRendererAsync is not null)
            return await options.NodeRendererAsync(node);

        if (options.NodeRenderer is not null)
            return options.NodeRenderer(node);

        return DefaultNodeRenderer(node);
    }

    private static async Task<LinkRenderInfo> GetLinkRenderInfoAsync(
        BaseLinkModel link, DiagramExportOptions options)
    {
        if (options.LinkRendererAsync is not null)
            return await options.LinkRendererAsync(link);

        if (options.LinkRenderer is not null)
            return options.LinkRenderer(link);

        return DefaultLinkRenderer(link);
    }

    private static LinkRenderInfo DefaultLinkRenderer(BaseLinkModel link)
    {
        var info = new LinkRenderInfo
        {
            SourceArrow = MapMarker(link.SourceMarker),
            TargetArrow = MapMarker(link.TargetMarker)
        };

        if (link is LinkModel typedLink)
        {
            if (!string.IsNullOrEmpty(typedLink.Color))
                info.StrokeColor = typedLink.Color;
            if (typedLink.Width > 0)
                info.StrokeWidth = typedLink.Width;
        }

        return info;
    }

    private static ArrowStyle MapMarker(LinkMarker? marker)
    {
        if (marker is null)
            return ArrowStyle.None;

        if (ReferenceEquals(marker, LinkMarker.Arrow))
            return ArrowStyle.FilledArrow;

        if (ReferenceEquals(marker, LinkMarker.Circle))
            return ArrowStyle.Diamond;

        if (ReferenceEquals(marker, LinkMarker.Square))
            return ArrowStyle.FilledDiamond;

        return ArrowStyle.Arrow;
    }

    private static NodeRenderInfo DefaultNodeRenderer(NodeModel node)
    {
        var info = new NodeRenderInfo
        {
            HeaderText = node.Title ?? node.Id,
            BodyText = "Sample node"
        };

        return info;
    }

    public static double CalculatePortYOffset(NodeRenderInfo renderInfo, string portId, double nodeHeight)
    {
        if (renderInfo.PortYOffsets is not null &&
            renderInfo.PortYOffsets.TryGetValue(portId, out var explicitOffset))
        {
            return explicitOffset;
        }

        const double headerHeight = 36.0;
        const double sectionLabelHeight = 24.0;
        double y = headerHeight;

        foreach (var section in renderInfo.Sections)
        {
            if (section.SectionLabel is not null)
                y += sectionLabelHeight;

            foreach (var row in section.Rows)
            {
                if (row.PortId == portId)
                    return y + (row.RowHeight / 2.0);

                y += row.RowHeight;
            }
        }

        return nodeHeight / 2.0;
    }

    private static void ComputeCanvasDimensions(DiagramSnapshot snapshot, double padding)
    {
        if (snapshot.Nodes.Count == 0)
        {
            snapshot.CanvasWidth = 0;
            snapshot.CanvasHeight = 0;
            return;
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var node in snapshot.Nodes)
        {
            minX = Math.Min(minX, node.X);
            minY = Math.Min(minY, node.Y);
            maxX = Math.Max(maxX, node.X + node.Width);
            maxY = Math.Max(maxY, node.Y + node.Height);
        }

        var offsetX = minX - padding;
        var offsetY = minY - padding;

        snapshot.OffsetX = offsetX;
        snapshot.OffsetY = offsetY;

        foreach (var node in snapshot.Nodes)
        {
            node.X -= offsetX;
            node.Y -= offsetY;
        }

        foreach (var link in snapshot.Links)
        {
            foreach (var wp in link.Waypoints)
            {
                wp.X -= offsetX;
                wp.Y -= offsetY;
            }
        }

        snapshot.CanvasWidth = (maxX - minX) + (padding * 2);
        snapshot.CanvasHeight = (maxY - minY) + (padding * 2);
    }

    public async ValueTask DisposeAsync()
    {
        await _jsInterop.DisposeAsync();
    }
}
