using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Embeddings;

namespace EduAdvisory_Backend.Services.AI;

#pragma warning disable SKEXP0001
public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly IAiKernelService _kernelService;

    public OpenAiEmbeddingService(IAiKernelService kernelService)
    {
        _kernelService = kernelService;
    }

    public async Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        var kernel = _kernelService.CreateKernel();
        var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        var embedding = await embeddingService.GenerateEmbeddingAsync(
            text,
            cancellationToken: cancellationToken
        );

        return embedding.ToArray();
    }
}
#pragma warning restore SKEXP0001