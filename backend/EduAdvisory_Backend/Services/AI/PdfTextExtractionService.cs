using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using UglyToad.PdfPig;

namespace EduAdvisory_Backend.Services.AI;

public class PdfTextExtractionService : IPdfTextExtractionService
{
    private readonly ILogger<PdfTextExtractionService> _logger;

    public PdfTextExtractionService(ILogger<PdfTextExtractionService> logger)
    {
        _logger = logger;
    }

    public Task<List<ExtractedPdfPage>> ExtractPagesAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("PDF file was not found.", filePath);
        }

        var pages = new List<ExtractedPdfPage>();

        using var document = PdfDocument.Open(filePath);

        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var text = page.Text ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(text))
            {
                pages.Add(new ExtractedPdfPage
                {
                    PageNumber = page.Number,
                    Text = NormalizeText(text)
                });
            }
        }

        _logger.LogInformation(
            "Extracted {PageCount} pages from PDF {FilePath}",
            pages.Count,
            filePath);

        return Task.FromResult(pages);
    }

    private static string NormalizeText(string text)
    {
        return text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }
}