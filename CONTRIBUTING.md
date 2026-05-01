# Contributing to FactFoundry.BlazorDiagramExporter

Thank you for your interest in contributing. This document explains how to set up the project locally, run tests, and submit changes.

## Getting Started

### Fork and Clone

1. Fork the repository on GitHub: [FactFoundry/blazor-diagram-exporter](https://github.com/FactFoundry/blazor-diagram-exporter)
2. Clone your fork:

```bash
git clone https://github.com/YOUR_USERNAME/blazor-diagram-exporter.git
cd blazor-diagram-exporter
```

### Prerequisites

- .NET 10.0 SDK (the solution targets `net8.0`, `net9.0`, and `net10.0`)
- A modern web browser (for running the sample app)

### Build

```bash
dotnet build
```

This builds all projects in the solution: the library, the test project, and the sample app.

### Run Tests

```bash
dotnet test
```

Tests target `DiagramSnapshot` directly and do not require a browser or JS interop. They run entirely in .NET.

### Run the Sample App

```bash
dotnet run --project samples/FactFoundry.BlazorDiagramExporter.Sample/
```

This starts a Blazor WebAssembly application that demonstrates the exporter with default and custom renderers.

## Code Style

- Follow the project's `.editorconfig` settings
- 4-space indentation for C#, 2-space for JSON/YAML/JS
- LF line endings
- Follow existing patterns and conventions in the codebase
- All warnings are treated as errors (`TreatWarningsAsErrors` is enabled in `Directory.Build.props`)

## Submitting Changes

1. Create a feature branch from `main`:

```bash
git checkout -b feature/your-feature-name
```

2. Make your changes and commit with clear, descriptive messages.

3. Ensure all tests pass:

```bash
dotnet test
```

4. Push your branch and open a pull request against `main` on the upstream repository.

### Pull Request Guidelines

- Keep PRs focused on a single change
- Include tests for new functionality
- Update documentation in `/docs` if your change affects the public API
- Ensure the sample app still builds

## Important Design Constraints

The following constraints are deliberate and must be maintained. Changes to these require discussion in an issue before implementation:

### DiagramSnapshot Is Part of the Public API

`DiagramSnapshot` and its constituent types (`SnapshotNode`, `SnapshotLink`, `SnapshotPoint`) are first-class public API. They must remain serializable plain objects. Breaking changes to these types require a major version bump and prior discussion.

### Zero External NuGet Dependencies

The library must have **zero** runtime NuGet dependencies beyond `Z.Blazor.Diagrams` and the Blazor framework itself. Do not add dependencies on SkiaSharp, jsPDF, or any third-party rendering library. SVG is built entirely in JavaScript; PNG/PDF rasterization uses the browser's built-in Canvas API. The JS module is loaded from `wwwroot` -- no CDN fetches at runtime.

### Orthogonal Routing Is the Consumer's Responsibility

The library does not compute orthogonal waypoints. This is a deliberate design decision because `Blazor.Diagrams` does not persist router-computed waypoints on the link model. Do not attempt to read waypoints from the DOM.

### Sync and Async Renderer Callbacks

Both synchronous and asynchronous renderer delegates must continue to be supported. When both are set, the async version takes precedence. Do not remove support for either variant.

### Built-in PDF Builder Is Intentionally Minimal

The built-in PDF export creates a single-page PDF with an embedded PNG image. Do not expand it into a full-featured PDF library. Consumers who need advanced PDF features should use `RenderToPngBytesAsync` and their preferred PDF library.

## Reporting Issues

- Use the [bug report template](https://github.com/FactFoundry/blazor-diagram-exporter/issues/new?template=bug_report.md) for bugs
- Use the [feature request template](https://github.com/FactFoundry/blazor-diagram-exporter/issues/new?template=feature_request.md) for feature ideas
- Check the [future enhancements](docs/future-enhancements.md) page to see if the feature is already planned

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
