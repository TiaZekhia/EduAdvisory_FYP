using System.Text;
using System.Text.RegularExpressions;
using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;

namespace EduAdvisory_Backend.Services.AI;

public class SemanticChunkingService : ISemanticChunkingService
{
    private const int MaxChunkWords = 450;
    private const int MinChunkWords = 60;
    private const int OverlapWords = 50;

    private static readonly Regex PageNumberRegex = new(
        @"^\d+\s*/\s*\d+$",
        RegexOptions.Compiled);

    public List<DocumentChunkCandidate> CreateChunks(
        List<ExtractedPdfPage> pages,
        string documentType)
    {
        var cleanedPages = CleanPages(pages);
        var sections = BuildSemanticSections(cleanedPages);

        var chunks = new List<DocumentChunkCandidate>();
        var chunkIndex = 0;

        foreach (var section in sections)
        {
            var sectionChunks = SplitSectionIntoChunks(section);

            foreach (var chunk in sectionChunks)
            {
                chunk.ChunkIndex = chunkIndex++;
                chunks.Add(chunk);
            }
        }

        return chunks
            .Where(c => !string.IsNullOrWhiteSpace(c.ChunkText))
            .ToList();
    }

    private static List<ExtractedPdfPage> CleanPages(List<ExtractedPdfPage> pages)
    {
        var result = new List<ExtractedPdfPage>();

        foreach (var page in pages)
        {
            if (LooksLikeTableOfContents(page.Text))
            {
                continue;
            }

            var lines = page.Text
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeLine)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => !IsNoiseLine(l))
                .ToList();

            if (!lines.Any())
            {
                continue;
            }

            result.Add(new ExtractedPdfPage
            {
                PageNumber = page.PageNumber,
                Text = string.Join("\n", lines)
            });
        }

        return result;
    }

    private static List<SemanticDocumentSection> BuildSemanticSections(
        List<ExtractedPdfPage> pages)
    {
        return pages
            .Where(p => !string.IsNullOrWhiteSpace(p.Text))
            .Where(p => CountWords(p.Text) >= 20)
            .Select(p => new SemanticDocumentSection
            {
                SectionTitle = DetectSectionTitleFromPage(p.Text, p.PageNumber),
                PageNumber = p.PageNumber,
                Text = p.Text.Trim()
            })
            .ToList();
    }

    private static string DetectSectionTitleFromPage(string text, int pageNumber)
    {
        var normalized = text.ToLowerInvariant();

        // Course syllabus-specific detection first
        if (normalized.Contains("assessment measures") ||
            normalized.Contains("passing grade") ||
            normalized.Contains("prerequisite course") ||
            normalized.Contains("corequisite course") ||
            normalized.Contains("credits"))
        {
            return "Course Overview and Assessment";
        }

        if (normalized.Contains("course references") ||
            normalized.Contains("references") ||
            normalized.Contains("textbook") ||
            normalized.Contains("bibliography"))
        {
            return "Course References";
        }

        var chapterTitle = DetectChapterTitle(text);

        if (!string.IsNullOrWhiteSpace(chapterTitle))
        {
            return chapterTitle;
        }

        // Program guide / study guide detection
        var rules = new List<(string Keyword, string Title)>
    {
        ("graduation requirements", "Graduation requirements"),

        ("academic suspension", "Academic suspension"),
        ("academic probation", "Academic probation"),
        ("good academic status", "Good academic status"),
        ("academic status", "Academic Status"),

        ("teaching language", "Teaching Language"),

        ("grade point average or gpa", "Grade point average or GPA"),
        ("grade point average", "Grade point average or GPA"),

        ("assessment methods for learning", "Assessment methods for learning"),
        ("assessment methods", "Assessment methods for learning"),

        ("main ranking system", "Grading"),
        ("grading", "Grading"),

        ("conditions of admission", "Conditions of admission"),
        ("admission procedures", "Admission procedures"),
        ("eligibility for the program", "Eligibility for the program"),

        ("student outcomes", "Student Outcomes"),
        ("program learning outcomes", "Program Learning Outcomes"),
        ("program educational objectives", "Program Educational Objectives"),

        ("suggestion for courses distribution per semester", "Courses distribution per semester"),

        ("semester 10", "Semester 10"),
        ("semester 9", "Semester 9"),
        ("semester 8", "Semester 8"),
        ("semester 7", "Semester 7"),
        ("semester 6", "Semester 6"),
        ("semester 5", "Semester 5"),
        ("semester 4", "Semester 4"),
        ("semester 3", "Semester 3"),
        ("semester 2", "Semester 2"),
        ("semester 1", "Semester 1"),

        ("projects and professional training", "Projects and Professional Training"),
        ("major requirements", "Major Requirements"),
        ("faculty requirements", "Faculty Requirements"),
        ("general education requirements", "General Education Requirements"),
        ("program of study", "Program of Study"),

        ("teaching methods", "Teaching methods"),
        ("international partners", "International partners and student mobility"),
        ("student mobility", "International partners and student mobility"),
        ("mission of the faculty", "The mission of the Faculty"),
        ("vision of the faculty", "The vision of the Faculty"),
        ("mission of the antonine university", "The mission of the Antonine University"),
        ("general provisions", "General provisions"),
        ("official withdrawal", "Official withdrawal"),
        ("unofficial withdrawal", "Class participation and unofficial withdrawal")
    };

        foreach (var rule in rules)
        {
            if (normalized.Contains(rule.Keyword))
            {
                return rule.Title;
            }
        }

        return $"Page {pageNumber}";
    }
    private static string? DetectChapterTitle(string text)
    {
        var lines = text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(NormalizeLine)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();

        var chapterRegex = new Regex(
            @"chapter\s+\d+\s*:\s*.+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (var line in lines)
        {
            var match = chapterRegex.Match(line);

            if (match.Success)
            {
                return ToTitleCase(match.Value);
            }
        }

        // Handles pages where the chapter title is split or appears with leading spaces
        var normalized = NormalizeLine(text);

        var inlineMatch = chapterRegex.Match(normalized);

        if (inlineMatch.Success)
        {
            return ToTitleCase(inlineMatch.Value);
        }

        return null;
    }
    private static string ToTitleCase(string value)
    {
        value = NormalizeLine(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static bool LooksLikeTableOfContents(string text)
    {
        var normalized = text.ToLowerInvariant();

        if (normalized.Contains("table of content") ||
            normalized.Contains("table of contents"))
        {
            return true;
        }

        var tocSignals = new[]
        {
            "introduction",
            "graduation requirements",
            "academic status",
            "teaching methods",
            "assessment methods",
            "general provisions",
            "official withdrawal"
        };

        var signalCount = tocSignals.Count(signal => normalized.Contains(signal));
        var hasManyDots = text.Count(c => c == '.') > 20;

        return signalCount >= 5 && hasManyDots;
    }

    private static bool IsNoiseLine(string line)
    {
        if (PageNumberRegex.IsMatch(line))
        {
            return true;
        }

        var noiseLines = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "STUDY GUIDE Faculty of Engineering",
            "STUDY GUIDE",
            "Faculty of Engineering",
            "Department of",
            "Computer and Communications Engineering",
            "2023-2024"
        };

        if (noiseLines.Contains(line))
        {
            return true;
        }

        return line.Length <= 2;
    }

    private static List<DocumentChunkCandidate> SplitSectionIntoChunks(
        SemanticDocumentSection section)
    {
        var paragraphs = section.Text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        if (!paragraphs.Any())
        {
            return new List<DocumentChunkCandidate>();
        }

        var chunks = new List<DocumentChunkCandidate>();
        var buffer = new List<string>();
        var wordCount = 0;

        foreach (var paragraph in paragraphs)
        {
            var paragraphWords = CountWords(paragraph);

            if (paragraphWords > MaxChunkWords)
            {
                FlushBuffer();
                chunks.AddRange(SplitLargeParagraph(section, paragraph));
                continue;
            }

            if (wordCount + paragraphWords > MaxChunkWords)
            {
                FlushBuffer();

                var overlap = GetOverlapText(chunks.LastOrDefault()?.ChunkText);

                if (!string.IsNullOrWhiteSpace(overlap))
                {
                    buffer.Add(overlap);
                    wordCount = CountWords(overlap);
                }
            }

            buffer.Add(paragraph);
            wordCount += paragraphWords;
        }

        FlushBuffer();

        return chunks;

        void FlushBuffer()
        {
            if (!buffer.Any())
            {
                return;
            }

            var text = string.Join("\n", buffer).Trim();
            var count = CountWords(text);

            if (count >= MinChunkWords || !chunks.Any())
            {
                chunks.Add(new DocumentChunkCandidate
                {
                    SectionTitle = section.SectionTitle,
                    PageNumber = section.PageNumber,
                    ChunkText = text,
                    TokenCount = EstimateTokens(text)
                });
            }

            buffer.Clear();
            wordCount = 0;
        }
    }

    private static List<DocumentChunkCandidate> SplitLargeParagraph(
        SemanticDocumentSection section,
        string paragraph)
    {
        var words = paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = new List<DocumentChunkCandidate>();

        var start = 0;

        while (start < words.Length)
        {
            var take = Math.Min(MaxChunkWords, words.Length - start);
            var chunkWords = words.Skip(start).Take(take).ToArray();

            var text = string.Join(" ", chunkWords);

            result.Add(new DocumentChunkCandidate
            {
                SectionTitle = section.SectionTitle,
                PageNumber = section.PageNumber,
                ChunkText = text,
                TokenCount = EstimateTokens(text)
            });

            if (start + take >= words.Length)
            {
                break;
            }

            start += MaxChunkWords - OverlapWords;
        }

        return result;
    }

    private static string? GetOverlapText(string? previousChunkText)
    {
        if (string.IsNullOrWhiteSpace(previousChunkText))
        {
            return null;
        }

        var words = previousChunkText.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length <= OverlapWords)
        {
            return previousChunkText;
        }

        return string.Join(" ", words.Skip(words.Length - OverlapWords));
    }

    private static string NormalizeLine(string line)
    {
        return Regex.Replace(line.Trim(), @"\s+", " ");
    }

    private static int CountWords(string text)
    {
        return text.Split(
            new[] { ' ', '\n', '\t' },
            StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static int EstimateTokens(string text)
    {
        return (int)Math.Ceiling(CountWords(text) * 1.3);
    }
}