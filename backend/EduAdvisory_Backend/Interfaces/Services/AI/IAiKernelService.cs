using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.Interfaces.Services.AI;

public interface IAiKernelService
{
    Kernel CreateKernel();
}