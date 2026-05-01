using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace FactFoundry.BlazorDiagramExporter;

internal sealed class ExporterJsInterop : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ExporterJsInterop(IJSRuntime jsRuntime)
    {
        _moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/FactFoundry.BlazorDiagramExporter/diagram-exporter.js").AsTask());
    }

    public async Task<byte[]> RenderToPngBytesAsync(DiagramSnapshot snapshot)
    {
        var module = await _moduleTask.Value;
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        return await module.InvokeAsync<byte[]>("renderToPngBytes", json);
    }

    public async Task ExportAsPngAsync(DiagramSnapshot snapshot, string fileName)
    {
        var module = await _moduleTask.Value;
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        await module.InvokeVoidAsync("exportAsPng", json, fileName);
    }

    public async Task ExportAsPdfAsync(DiagramSnapshot snapshot, string fileName)
    {
        var module = await _moduleTask.Value;
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        await module.InvokeVoidAsync("exportAsPdf", json, fileName);
    }

    public async Task ExportAsSvgAsync(DiagramSnapshot snapshot, string fileName)
    {
        var module = await _moduleTask.Value;
        var json = JsonSerializer.Serialize(snapshot, SerializerOptions);
        await module.InvokeVoidAsync("exportAsSvg", json, fileName);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
