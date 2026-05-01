# Fonts

The `FontFamily` property on `DiagramExportOptions` controls the font used for all text in the exported diagram -- headers, row labels, secondary text, section labels, and link labels.

## Default Font

The default font is `"Arial"`, which is available on virtually all systems and browsers.

```csharp
var options = new DiagramExportOptions
{
    FontFamily = "Arial" // default
};
```

## Browser Availability Requirement

The specified font must be **available in the browser** at the time of export. The library renders text in SVG using the `font-family` attribute, which relies on the browser's font resolution. For PNG/PDF rasterization, the browser uses its standard font matching. If the specified font is not available, the browser silently falls back to a default font (typically a generic sans-serif), which may produce unexpected results.

The library does **not** automatically load fonts. You are responsible for ensuring the font is available.

## Using Web Fonts

If you use a web font (such as Google Fonts or a self-hosted font), make sure it is fully loaded before calling any export method.

### Loading via CSS (@font-face)

Add the font to your application's CSS:

```css
@font-face {
    font-family: 'Inter';
    src: url('/fonts/Inter-Regular.woff2') format('woff2');
    font-weight: 400;
    font-style: normal;
    font-display: swap;
}

@font-face {
    font-family: 'Inter';
    src: url('/fonts/Inter-Bold.woff2') format('woff2');
    font-weight: 700;
    font-style: normal;
    font-display: swap;
}
```

### Loading via Google Fonts

Add the stylesheet link to your `index.html` or `_Host.cshtml`:

```html
<link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;700&display=swap"
      rel="stylesheet">
```

### Ensuring the Font Is Loaded

The browser may not have finished loading the font by the time your export runs. Use the [Font Loading API](https://developer.mozilla.org/en-US/docs/Web/API/CSS_Font_Loading_API) to wait for the font to be ready:

```csharp
// In your Blazor component, call this before exporting
await JS.InvokeVoidAsync("eval", "await document.fonts.ready");

// Or wait for a specific font
await JS.InvokeVoidAsync("eval",
    "await document.fonts.load('16px Inter')");

await Exporter.ExportAsPngAsync(diagram, new DiagramExportOptions
{
    FontFamily = "Inter, sans-serif"
});
```

## Font Stacks

You can specify a font stack (comma-separated list of font families) just as you would in CSS. The browser uses the first available font:

```csharp
var options = new DiagramExportOptions
{
    FontFamily = "Cascadia Code, Fira Code, Consolas, monospace"
};
```

## Common Font Choices

### System Fonts (no loading required)

These fonts are available on most systems without any additional setup:

| Font | Platform Availability |
|------|----------------------|
| `Arial` | Windows, macOS, Linux |
| `Segoe UI` | Windows |
| `Helvetica` | macOS |
| `Consolas` | Windows |
| `Courier New` | Windows, macOS, Linux |
| `Verdana` | Windows, macOS |

### Monospace Fonts (for code/data diagrams)

```csharp
// Good cross-platform monospace stack
FontFamily = "Cascadia Code, Fira Code, Consolas, Courier New, monospace"
```

### UI Fonts

```csharp
// Modern UI font stack
FontFamily = "Inter, Segoe UI, Roboto, Helvetica Neue, Arial, sans-serif"
```

## Troubleshooting

**Text appears in the wrong font:** The specified font is not available in the browser. Verify the font is loaded by checking `document.fonts.check('16px YourFont')` in the browser console.

**Text is missing or garbled:** Some Unicode characters or emoji may not be available in the specified font. The browser's font fallback mechanism should handle most cases, but icon rendering in `NodeRow.Icon` may be affected.

**Font looks different between screen and export:** The live Blazor.Diagrams rendering uses CSS/HTML text rendering, while the SVG export uses SVG text elements. For PNG/PDF, the SVG is rasterized via an offscreen canvas. There may be subtle differences in kerning, anti-aliasing, and sub-pixel rendering between the live view and the export. These differences are typically minor.
