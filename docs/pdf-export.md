# PDF Export

FactFoundry.BlazorDiagramExporter includes a built-in minimal PDF builder that can export your diagram as a PDF file without any third-party PDF library dependency. For advanced PDF features, you can use `RenderToPngBytesAsync` to get the raw PNG bytes and pass them to your preferred PDF library.

## Built-in PDF Export

The built-in PDF export is intentionally minimal. It:

1. Renders the diagram to PNG at the specified scale
2. Builds a minimal valid PDF document in JavaScript
3. Embeds the PNG as a full-page image
4. Triggers a browser file download

```csharp
await Exporter.ExportAsPdfAsync(diagram, new DiagramExportOptions
{
    Scale = 2.0,
    FileName = "my-diagram"
});
```

### What the built-in PDF builder does

- Creates a single-page PDF sized to fit the diagram
- Orients the page as landscape if the diagram is wider than it is tall
- Embeds the rendered PNG as an `XObject` image
- Produces a valid PDF that opens in any PDF viewer
- Requires **zero external dependencies** -- no jsPDF, no PDFSharp, no SkiaSharp

### What the built-in PDF builder does NOT do

- Multi-page PDFs
- Text layers (the diagram is a rasterized image -- text is not selectable)
- Vector graphics (the content is rasterized PNG, not vector paths)
- PDF metadata (title, author, keywords)
- PDF/A compliance
- Encryption or password protection
- Bookmarks or table of contents
- Annotations or form fields

## Using a Third-Party PDF Library Instead

For anything beyond a simple single-page image PDF, use `RenderToPngBytesAsync` to get the raw PNG bytes and pass them to your preferred PDF library.

### Why this design?

The library maintains a strict **zero external NuGet dependency** policy (beyond `Z.Blazor.Diagrams`). Bundling a full PDF library would:

- Add a significant dependency to every consumer's project
- Create version conflicts with consumers who already use a PDF library
- Bloat the package size for consumers who only need PNG export

Instead, the library gives you the PNG bytes and lets you use whatever PDF library you already have.

### Example with QuestPDF

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

byte[] pngBytes = await Exporter.RenderToPngBytesAsync(diagram, new DiagramExportOptions
{
    Scale = 2.0,
    BackgroundColor = "#ffffff"
});

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4.Landscape());
        page.Margin(1, Unit.Centimetre);
        page.Header().Text("Data Model Diagram").FontSize(16).Bold();
        page.Content().Image(pngBytes);
        page.Footer().AlignCenter().Text(text =>
        {
            text.Span("Generated on ");
            text.Span(DateTime.Now.ToString("yyyy-MM-dd"));
        });
    });
}).GeneratePdf("report.pdf");
```

### Example with iTextSharp / iText 7

```csharp
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;

byte[] pngBytes = await Exporter.RenderToPngBytesAsync(diagram, new DiagramExportOptions
{
    Scale = 2.0
});

using var writer = new PdfWriter("report.pdf");
using var pdf = new PdfDocument(writer);
using var document = new Document(pdf);

var imageData = ImageDataFactory.Create(pngBytes);
var image = new Image(imageData);
image.SetAutoScale(true);

document.Add(new Paragraph("Data Model Diagram").SetFontSize(16));
document.Add(image);
```

### Example with PDFSharpCore

```csharp
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

byte[] pngBytes = await Exporter.RenderToPngBytesAsync(diagram, new DiagramExportOptions
{
    Scale = 2.0
});

var document = new PdfDocument();
var page = document.AddPage();
page.Orientation = PdfSharpCore.PageOrientation.Landscape;

using var stream = new MemoryStream(pngBytes);
var xImage = XImage.FromStream(() => stream);

var gfx = XGraphics.FromPdfPage(page);
gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);

document.Save("report.pdf");
```

## Choosing a Scale Factor

When producing PDFs, a higher scale factor produces a sharper image. Recommendations:

| Use Case | Scale | Notes |
|----------|-------|-------|
| Screen viewing | `1.0` | Standard resolution, smallest file size |
| General PDF | `2.0` | Good quality on most screens and moderate zoom |
| Print (300 DPI) | `3.0` - `4.0` | Sharp at print resolution |

Keep in mind that higher scale factors produce larger PNG data, which increases the PDF file size.

## Limitations

- The built-in PDF export creates a rasterized image, not vector graphics. Text in the PDF is not selectable or searchable.
- PDF page size is determined by the diagram dimensions. Very large diagrams may produce very large pages.
- The built-in PDF builder runs in the browser via JS interop. It cannot be used in server-side-only scenarios without a browser context. Use `CreateSnapshotAsync` + your own PDF library for server-side PDF generation.
