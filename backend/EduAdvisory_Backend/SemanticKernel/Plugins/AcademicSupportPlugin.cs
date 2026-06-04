using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace EduAdvisory_Backend.SemanticKernel.Plugins;

public class AcademicSupportPlugin
{
    [KernelFunction]
    [Description("Provides general academic support guidance when the student's question is not answered by RAG documents or live student data.")]
    public string GetGeneralAcademicSupportAdvice(
        [Description("The student's question.")]
        string question)
    {
        return """
General academic support advice:
- Review your course study guide and syllabus first.
- Break the topic into smaller subtopics.
- Prioritize upcoming assessments and weak areas.
- Contact your advisor or instructor if the issue is related to policy, deadlines, grades, or personal academic risk.
- Use office hours or academic support services if available.
""";
    }
}