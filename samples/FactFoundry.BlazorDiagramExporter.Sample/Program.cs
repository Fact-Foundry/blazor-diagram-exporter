using FactFoundry.BlazorDiagramExporter;
using FactFoundry.BlazorDiagramExporter.Sample;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorDiagramExporter();

await builder.Build().RunAsync();
