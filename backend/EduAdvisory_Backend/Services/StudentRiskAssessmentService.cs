using EduAdvisory_Backend.DTOs.Course;
using EduAdvisory_Backend.DTOs.Student;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;

namespace EduAdvisory_Backend.Services
{
    public class StudentRiskAssessmentService : IStudentRiskAssessmentService
    {
        private readonly IStudentRepository _studentRepo;

        public StudentRiskAssessmentService(IStudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
        }

        public StudentRiskAssessmentDto AssessStudent(int studentId)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null)
                throw new Exception("Student not found.");

            var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

            var currentEnrollment = _studentRepo.GetCurrentEnrollmentWithCourse(studentId);
            var currentPerformance = _studentRepo.GetCurrentCoursesPerformance(studentId);
            var passedCourses = _studentRepo.GetPassedCourses(studentId).ToHashSet();
            var studyMap = _studentRepo.GetStudyGuideRecommendedSemester(student.ProgramCode ?? "");

            var currentCourseCodes = currentEnrollment
                .Select(c => c.CourseCode)
                .Distinct()
                .ToList();

            var gradingSchemaMap = _studentRepo.GetCourseGradingSchema(currentCourseCodes);

            var globalAlerts = new List<StudentAlertDto>();
            var mainReasons = new List<string>();

            // ===================================================
            // 1) GLOBAL ALERTS (3c)
            // Only:
            // - Low GPA
            // - Credit delay
            // - Missing current semester courses
            // ===================================================

            var currentGpa = student.CurrentGpa;
            var gpaRisk = GpaRiskFromValue(currentGpa);

            if (currentGpa.HasValue)
            {
                if (currentGpa.Value < 60m)
                {
                    globalAlerts.Add(new StudentAlertDto
                    {
                        Severity = "HIGH",
                        Title = "Low cumulative GPA",
                        Message = $"Current GPA is {currentGpa.Value:F1}. Immediate academic support is recommended.",
                        CreatedAt = now
                    });

                    mainReasons.Add("Low cumulative GPA.");
                }
                else if (currentGpa.Value < 70m)
                {
                    globalAlerts.Add(new StudentAlertDto
                    {
                        Severity = "MEDIUM",
                        Title = "GPA needs improvement",
                        Message = $"Current GPA is {currentGpa.Value:F1}. Performance should be monitored closely.",
                        CreatedAt = now
                    });

                    mainReasons.Add("Moderate GPA risk.");
                }
            }

            var currentSemester = student.CurrentSemester ?? 0;

            var currentSemesterExpectedCourses = studyMap
                .Where(kvp => kvp.Value == currentSemester)
                .Select(kvp => kvp.Key)
                .Distinct()
                .ToList();

            var currentSemesterMissingCourses = currentSemesterExpectedCourses
                .Where(code => !passedCourses.Contains(code) && !currentCourseCodes.Contains(code))
                .ToList();

            var currentSemesterMissingCount = currentSemesterMissingCourses.Count;

            var currentSemesterMissingRisk = currentSemesterExpectedCourses.Count == 0
                ? 0
                : Clamp01((double)currentSemesterMissingCount / currentSemesterExpectedCourses.Count);

            if (currentSemesterMissingCount > 0)
            {
                globalAlerts.Add(new StudentAlertDto
                {
                    Severity = currentSemesterMissingCount >= 2 ? "HIGH" : "MEDIUM",
                    Title = "Missing current semester course",
                    Message = $"Student is missing {currentSemesterMissingCount} course(s) recommended for the current semester.",
                    CreatedAt = now
                });

                mainReasons.Add("Student did not enroll in one or more courses expected this semester.");
            }

            var requiredByNow = studyMap
                .Where(kvp => kvp.Value <= currentSemester)
                .Select(kvp => kvp.Key)
                .Distinct()
                .ToList();

            var requiredByNowMeta = _studentRepo.GetCoursesMeta(requiredByNow);

            var expectedCreditsByNow = requiredByNow.Sum(code =>
                requiredByNowMeta.TryGetValue(code, out var meta) ? meta.credits : 0);

            var earnedCredits = passedCourses.Sum(code =>
                requiredByNowMeta.TryGetValue(code, out var meta) ? meta.credits : 0);

            var delayedCredits = expectedCreditsByNow - earnedCredits;
            if (delayedCredits < 0) delayedCredits = 0;

            var creditDelayRisk = expectedCreditsByNow == 0
                ? 0
                : Clamp01((double)delayedCredits / expectedCreditsByNow);

            if (delayedCredits >= 12)
            {
                globalAlerts.Add(new StudentAlertDto
                {
                    Severity = "HIGH",
                    Title = "Credit completion delay",
                    Message = $"Student is behind by {delayedCredits} credits compared to the study plan.",
                    CreatedAt = now
                });

                mainReasons.Add("Significant delay in earned credits.");
            }
            else if (delayedCredits >= 6)
            {
                globalAlerts.Add(new StudentAlertDto
                {
                    Severity = "MEDIUM",
                    Title = "Credit completion delay",
                    Message = $"Student is behind by {delayedCredits} credits compared to the study plan.",
                    CreatedAt = now
                });

                mainReasons.Add("Moderate delay in earned credits.");
            }

            // ===================================================
            // 2) COURSE-LEVEL ASSESSMENT (3d)
            // ===================================================

            var courseAssessments = new List<CourseRiskAssessmentDto>();

            foreach (var course in currentPerformance)
            {
                var riskFactors = new List<CourseRiskFactorDto>();
                var componentRisks = new List<CourseComponentRiskDto>();

                // Attendance
                double absenceRisk = 0;
                if (course.MaxAbsences.HasValue && course.MaxAbsences.Value > 0)
                {
                    var ratio = (double)(course.AbsencesCount ?? 0) / course.MaxAbsences.Value;
                    absenceRisk = AbsenceRiskFromRatio(ratio);

                    if (ratio >= 0.8)
                    {
                        riskFactors.Add(new CourseRiskFactorDto
                        {
                            Severity = "HIGH",
                            Title = "Critical Absences",
                            Detail = $"{course.AbsencesCount}/{course.MaxAbsences} absences"
                        });
                    }
                    else if (ratio >= 0.5)
                    {
                        riskFactors.Add(new CourseRiskFactorDto
                        {
                            Severity = "MEDIUM",
                            Title = "High Absences",
                            Detail = $"{course.AbsencesCount}/{course.MaxAbsences} absences"
                        });
                    }
                    else if (ratio >= 0.3)
                    {
                        riskFactors.Add(new CourseRiskFactorDto
                        {
                            Severity = "LOW",
                            Title = "Moderate Absences",
                            Detail = $"{course.AbsencesCount}/{course.MaxAbsences} absences"
                        });
                    }
                }

                // Components + grading schema
                var schema = gradingSchemaMap.ContainsKey(course.CourseCode)
                    ? gradingSchemaMap[course.CourseCode]
                    : new List<CourseGradingSchemaDto>();

                var components = course.Components ?? new List<CourseComponentGradeDto>();

                decimal weightedComponentRiskSum = 0m;
                int totalKnownWeight = schema.Sum(x => x.WeightPercentage);

                foreach (var component in components)
                {
                    var schemaItem = schema.FirstOrDefault(x =>
                        string.Equals(x.ComponentName, component.ComponentName, StringComparison.OrdinalIgnoreCase));

                    var weight = schemaItem?.WeightPercentage ?? 0;
                    var componentRisk = ComponentRiskFromGrade(component.Grade);
                    var componentRiskScore = ToPercent(componentRisk);

                    componentRisks.Add(new CourseComponentRiskDto
                    {
                        ComponentName = component.ComponentName,
                        Grade = component.Grade,
                        WeightPercentage = weight,
                        RiskScore = componentRiskScore
                    });

                    if (component.Grade.HasValue && componentRisk > 0)
                    {
                        riskFactors.Add(new CourseRiskFactorDto
                        {
                            Severity = componentRisk >= 0.8 ? "HIGH" : componentRisk >= 0.5 ? "MEDIUM" : "LOW",
                            Title = ComponentTitle(component.ComponentName, componentRisk),
                            Detail = weight > 0
                                ? $"{component.ComponentName} grade: {component.Grade}/100 • Weight {weight}%"
                                : $"{component.ComponentName} grade: {component.Grade}/100"
                        });
                    }

                    if (weight > 0)
                    {
                        weightedComponentRiskSum += (decimal)componentRisk * weight;
                    }
                }

                double weightedComponentRisk = totalKnownWeight > 0
                    ? (double)(weightedComponentRiskSum / totalKnownWeight)
                    : ComponentsAverageRisk(componentRisks);

                // Final course score
                var courseRisk = 0.35 * absenceRisk + 0.65 * weightedComponentRisk;
                var courseRiskScore = ToPercent(courseRisk);
                var courseRiskLevel = RiskLevelFromScore(courseRiskScore);

                if (courseRiskScore >= 70)
                    mainReasons.Add($"{course.CourseCode} is at high risk this semester.");

                courseAssessments.Add(new CourseRiskAssessmentDto
                {
                    CourseCode = course.CourseCode,
                    CourseName = course.CourseName,
                    Credits = course.Credits,
                    RiskLevel = courseRiskLevel,
                    RiskScore = courseRiskScore,
                    AbsencesCount = course.AbsencesCount,
                    MaxAbsences = course.MaxAbsences,
                    RiskFactors = riskFactors
                        .OrderByDescending(x => SeverityRank(x.Severity))
                        .ToList(),
                    Components = componentRisks
                        .OrderByDescending(x => x.WeightPercentage)
                        .ThenBy(x => x.ComponentName)
                        .ToList(),
                    Recommendation = CourseRecommendationFromRisk(courseRiskLevel)
                });
            }

            courseAssessments = courseAssessments
                .OrderByDescending(x => SeverityRank(x.RiskLevel))
                .ThenByDescending(x => x.RiskScore)
                .ThenBy(x => x.CourseCode)
                .ToList();

            var courseSummary = new CourseRiskSummaryDto
            {
                Count = courseAssessments.Count,
                High = courseAssessments.Count(x => x.RiskLevel == "HIGH"),
                Medium = courseAssessments.Count(x => x.RiskLevel == "MEDIUM"),
                Low = courseAssessments.Count(x => x.RiskLevel == "LOW")
            };

            // ===================================================
            // 3) OVERALL STUDENT RISK SCORE
            // Focus:
            // - current course portfolio
            // - GPA
            // - academic status
            // - credit delay
            // - missing current semester courses
            // ===================================================

            var coursePortfolioRisk = courseAssessments.Any()
                ? courseAssessments.Average(x => x.RiskScore) / 100.0
                : 0.0;

            var academicStatus = (student.AcademicStatus ?? "NORMAL").ToUpperInvariant();
            var academicStatusRisk = AcademicStatusRiskFromValue(academicStatus);

            var weightedRisk =
                  0.55 * coursePortfolioRisk
                + 0.15 * gpaRisk
                + 0.10 * academicStatusRisk
                + 0.10 * creditDelayRisk
                + 0.10 * currentSemesterMissingRisk;

            var riskScore = (int)Math.Round(weightedRisk * 100);
            var riskLevel = RiskLevelFromScore(riskScore);

            if (!mainReasons.Any())
                mainReasons.Add("No major risk indicators detected.");

            return new StudentRiskAssessmentDto
            {
                StudentId = student.StudentId,
                StudentName = $"{student.FirstName} {student.LastName}".Trim(),
                ProgramCode = student.ProgramCode ?? "",
                CurrentSemester = currentSemester,
                RiskLevel = riskLevel,
                RiskScore = riskScore,
                Factors = new StudentRiskFactorsDto
                {
                    CoursePortfolioRiskScore = ToPercent(coursePortfolioRisk),
                    GpaRiskScore = ToPercent(gpaRisk),
                    AcademicStatusRiskScore = ToPercent(academicStatusRisk),
                    CreditDelayRiskScore = ToPercent(creditDelayRisk),
                    CurrentSemesterMissingRiskScore = ToPercent(currentSemesterMissingRisk),

                    CurrentGpa = currentGpa,
                    AcademicStatus = academicStatus,
                    ExpectedCreditsByNow = expectedCreditsByNow,
                    EarnedCredits = earnedCredits,
                    DelayedCredits = delayedCredits,
                    CurrentSemesterMissingCoursesCount = currentSemesterMissingCount
                },
                MainReasons = mainReasons
                    .Distinct()
                    .Take(6)
                    .ToList(),
                RecommendedAction = StudentRecommendationFromRisk(riskLevel),
                GlobalAlertsSummary = new StudentAlertsResponseDto
                {
                    Count = globalAlerts.Count,
                    High = globalAlerts.Count(x => x.Severity == "HIGH"),
                    Medium = globalAlerts.Count(x => x.Severity == "MEDIUM"),
                    Low = globalAlerts.Count(x => x.Severity == "LOW"),
                    Alerts = globalAlerts
                        .OrderByDescending(x => SeverityRank(x.Severity))
                        .ThenByDescending(x => x.CreatedAt)
                        .ToList()
                },
                CourseSummary = courseSummary,
                CourseAssessments = courseAssessments
            };
        }

        // Existing student alerts page can keep using this old-style endpoint if you want.
        // Here it returns the "global alerts" only for the risk assessment engine.
        public StudentAlertsResponseDto GetStudentAlerts(int studentId)
        {
            return AssessStudent(studentId).GlobalAlertsSummary;
        }

        public StudentAlertsCountDto GetStudentAlertsCount(int studentId)
        {
            var alerts = GetStudentAlerts(studentId);

            return new StudentAlertsCountDto
            {
                Count = alerts.Count,
                High = alerts.High,
                Medium = alerts.Medium,
                Low = alerts.Low
            };
        }

        private static int ToPercent(double value)
        {
            return (int)Math.Round(Clamp01(value) * 100);
        }

        private static double Clamp01(double value)
        {
            if (value < 0) return 0;
            if (value > 1) return 1;
            return value;
        }

        private static int SeverityRank(string severity)
        {
            return severity switch
            {
                "HIGH" => 3,
                "MEDIUM" => 2,
                _ => 1
            };
        }

        private static double AbsenceRiskFromRatio(double ratio)
        {
            if (ratio >= 0.8) return 1.0;
            if (ratio >= 0.5) return 0.6;
            if (ratio >= 0.3) return 0.25;
            return 0.0;
        }

        private static double ComponentRiskFromGrade(decimal? grade)
        {
            if (!grade.HasValue) return 0.0;
            if (grade.Value < 60m) return 1.0;
            if (grade.Value < 70m) return 0.6;
            if (grade.Value < 80m) return 0.25;
            return 0.0;
        }

        private static double ComponentsAverageRisk(List<CourseComponentRiskDto> components)
        {
            if (components == null || components.Count == 0) return 0.0;
            return components.Average(x => x.RiskScore) / 100.0;
        }

        private static double GpaRiskFromValue(decimal? gpa)
        {
            if (!gpa.HasValue) return 0.0;
            if (gpa.Value < 60m) return 1.0;
            if (gpa.Value < 70m) return 0.6;
            if (gpa.Value < 80m) return 0.25;
            return 0.0;
        }

        private static double AcademicStatusRiskFromValue(string status)
        {
            return status switch
            {
                "PROBATION" => 1.0,
                "WARNING" => 0.6,
                _ => 0.0
            };
        }

        private static string RiskLevelFromScore(int score)
        {
            if (score >= 70) return "HIGH";
            if (score >= 40) return "MEDIUM";
            return "LOW";
        }

        private static string StudentRecommendationFromRisk(string riskLevel)
        {
            return riskLevel switch
            {
                "HIGH" => "Immediate advisor follow-up is recommended. Review current course performance, GPA, and academic progression.",
                "MEDIUM" => "Monitor this student closely and intervene on weak courses or academic standing where needed.",
                _ => "Student is currently stable. Continue routine monitoring."
            };
        }

        private static string CourseRecommendationFromRisk(string riskLevel)
        {
            return riskLevel switch
            {
                "HIGH" => "Schedule immediate meeting with the student. Consider tutoring, extra support, and close follow-up.",
                "MEDIUM" => "Monitor this course closely and encourage corrective action before the final assessment period.",
                _ => "Student is performing adequately in this course. Continue monitoring."
            };
        }

        private static string ComponentTitle(string componentName, double risk)
        {
            var name = (componentName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(name))
                return risk >= 0.8 ? "Weak Component Performance" :
                       risk >= 0.5 ? "Average Component Performance" :
                       "Slight Component Concern";

            if (risk >= 0.8) return $"Weak {name} Performance";
            if (risk >= 0.5) return $"Average {name} Performance";
            return $"{name} Needs Attention";
        }
    }
}