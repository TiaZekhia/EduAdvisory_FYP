using EduAdvisory_Backend.Interfaces.Services.AI;
using EduAdvisory_Backend.Models;
using EduAdvisory_Backend.SemanticKernel.Plugins;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.Services.AI;

public class AiKernelService : IAiKernelService
{
    private readonly OpenAiOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public AiKernelService(
        IOptions<OpenAiOptions> openAiOptions,
        IServiceProvider serviceProvider)
    {
        _options = openAiOptions.Value;
        _serviceProvider = serviceProvider;
    }

    public Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();

        builder.Services.AddLogging();

        builder.AddOpenAIChatCompletion(
            modelId: _options.ChatModel,
            apiKey: _options.ApiKey
        );

        builder.AddOpenAITextEmbeddingGeneration(
            modelId: _options.EmbeddingModel,
            apiKey: _options.ApiKey
        );

        var kernel = builder.Build();

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentInfoPlugin>(_serviceProvider),
            "StudentInfo");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentProgressPlugin>(_serviceProvider),
            "StudentProgress");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentMeetingPlugin>(_serviceProvider),
            "StudentMeeting");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentAttendancePlugin>(_serviceProvider),
            "StudentAttendance");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentCourseHistoryPlugin>(_serviceProvider),
            "StudentCourseHistory");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentRiskPlugin>(_serviceProvider),
            "StudentRisk");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<AdvisorAnnouncementPlugin>(_serviceProvider),
            "AdvisorAnnouncements");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<StudentStudyPlanPlugin>(_serviceProvider),
            "StudentStudyPlan");

        kernel.Plugins.AddFromObject(
            ActivatorUtilities.CreateInstance<AcademicSupportPlugin>(_serviceProvider),
            "AcademicSupport");

        return kernel;
    }
}