using EduAdvisory_Backend.DTOs.CoursePlan;
using EduAdvisory_Backend.Interfaces.Repositories;
using EduAdvisory_Backend.Interfaces.Services;

namespace EduAdvisory_Backend.Services
{
    public class CoursePlanService : ICoursePlanService
    {
        private readonly IStudentRepository _studentRepo;

        public CoursePlanService(IStudentRepository studentRepo)
        {
            _studentRepo = studentRepo;
        }

        private static bool IsFall(int semesterNumber) => semesterNumber % 2 == 1;

        private static bool IsOfferedInSemester(int plannedSemester, int recommendedSemester)
            => (plannedSemester % 2) == (recommendedSemester % 2);

        public List<CoursePlanDto> GeneratePlansForStudent(int studentId, int plansCount = 3)
        {
            var student = _studentRepo.GetById(studentId);
            if (student == null) return new();

            var program = student.ProgramCode ?? "";
            var currentSemester = student.CurrentSemester ?? 0;

            var creditLimit = (student.AcademicStatus == "PROBATION") ? 16 : 18;

            var passed = _studentRepo.GetPassedCourses(studentId).ToHashSet();
            var failedNotRetaken = _studentRepo.GetFailedNotRetakenCourses(studentId).ToHashSet();

            // All program courses (study guide) + include failed courses (must retake)
            var studyMap = _studentRepo.GetStudyGuideRecommendedSemester(program);
            var allProgramCourses = studyMap.Keys.ToHashSet();

            // Remaining = all program courses not passed, PLUS failed-not-retaken (already included but keep rule explicit)
            var remaining = allProgramCourses.Where(c => !passed.Contains(c)).ToHashSet();
            foreach (var fc in failedNotRetaken) remaining.Add(fc);

            var allCodes = remaining.ToList();
            var courseMeta = _studentRepo.GetCoursesMeta(allCodes);
            var prereqMap = _studentRepo.GetPrerequisitesMap(allCodes);

            // Different “strategies” (variations)
            var strategies = new List<(string name, Func<string, (int a, int b, int c, int d)> key)>
            {
                // Balanced: retakes first, then lowest recommended semester, then higher credits
                ("Balanced",
                    code =>
                    {
                        var rec = studyMap.TryGetValue(code, out var r) ? r : 99;
                        var credits = courseMeta.TryGetValue(code, out var m) ? m.credits : 0;
                        var isRetake = failedNotRetaken.Contains(code) ? 0 : 1; // 0 first
                        return (isRetake, rec, -credits, code.GetHashCode());
                    }),

                // Fast blockers: prioritize prerequisites of many others (simple proxy: appear as prereq often)
                ("FastBlockers",
                    code =>
                    {
                        var rec = studyMap.TryGetValue(code, out var r) ? r : 99;
                        var credits = courseMeta.TryGetValue(code, out var m) ? m.credits : 0;
                        var isRetake = failedNotRetaken.Contains(code) ? 0 : 1;
                        var isPrereqForMany = 0;
                        // compute later via map; use placeholder here
                        return (isRetake, isPrereqForMany, rec, -credits);
                    }),

                // Study-guide strict: follow recommended semester strongly
                ("StudyGuideStrict",
                    code =>
                    {
                        var rec = studyMap.TryGetValue(code, out var r) ? r : 99;
                        var isRetake = failedNotRetaken.Contains(code) ? 0 : 1;
                        return (isRetake, rec, code.Length, code.GetHashCode());
                    })
            };

            // Precompute "prereq frequency" for FastBlockers
            var prereqFrequency = new Dictionary<string, int>();
            foreach (var kv in prereqMap)
            {
                foreach (var p in kv.Value)
                {
                    if (!prereqFrequency.ContainsKey(p)) prereqFrequency[p] = 0;
                    prereqFrequency[p]++;
                }
            }

            // Patch FastBlockers strategy to use frequency
            strategies[1] = ("FastBlockers",
                code =>
                {
                    var rec = studyMap.TryGetValue(code, out var r) ? r : 99;
                    var credits = courseMeta.TryGetValue(code, out var m) ? m.credits : 0;
                    var isRetake = failedNotRetaken.Contains(code) ? 0 : 1;
                    var freq = prereqFrequency.TryGetValue(code, out var f) ? -f : 0; // negative = higher first
                    return (isRetake, freq, rec, -credits);
                }
            );

            var chosen = strategies.Take(Math.Max(1, Math.Min(plansCount, strategies.Count))).ToList();
            var plans = new List<CoursePlanDto>();

            foreach (var (strategyName, sortKey) in chosen)
            {
                var plan = BuildSinglePlan(
                    studentId,
                    program,
                    currentSemester,
                    creditLimit,
                    passed,
                    failedNotRetaken,
                    remaining,
                    studyMap,
                    prereqMap,
                    courseMeta,
                    strategyName,
                    sortKey
                );

                if (plan.Semesters.Any())
                    plans.Add(plan);
            }

            return plans;
        }

        private CoursePlanDto BuildSinglePlan(
            int studentId,
            string program,
            int currentSemester,
            int creditLimit,
            HashSet<string> passed,
            HashSet<string> failedNotRetaken,
            HashSet<string> remaining,
            Dictionary<string, int> studyMap,
            Dictionary<string, List<string>> prereqMap,
            Dictionary<string, (string name, int credits)> courseMeta,
            string strategy,
            Func<string, (int a, int b, int c, int d)> sortKey
        )
        {
            var scheduled = new HashSet<string>();
            var semesters = new List<PlannedSemesterDto>();

            // Start planning from next semester (future plan)
            var plannedSemester = currentSemester + 1;

            // Safety cap to avoid infinite loops in messy data
            var maxSemestersToTry = 20;

            for (int i = 0; i < maxSemestersToTry && remaining.Except(scheduled).Any(); i++, plannedSemester++)
            {
                var termLabel = IsFall(plannedSemester) ? "Fall" : "Spring";
                var cap = creditLimit;

                var available = remaining
                    .Except(scheduled)
                    .Where(code =>
                    {
                        // must exist in study guide to know offering
                        var rec = studyMap.TryGetValue(code, out var r) ? r : 0;
                        if (rec == 0) return false;

                        // offering constraint
                        if (!IsOfferedInSemester(plannedSemester, rec)) return false;

                        // prereq constraint (must be passed or scheduled in earlier semesters)
                        var prereqs = prereqMap.TryGetValue(code, out var p) ? p : new List<string>();
                        return prereqs.All(pr => passed.Contains(pr) || scheduled.Contains(pr));
                    })
                    .ToList();

                // If nothing can be taken this semester, continue (maybe next term offering helps)
                if (!available.Any())
                    continue;

                // Sort by strategy
                available = available
                    .OrderBy(sortKey)
                    .ToList();

                var picked = new List<string>();
                var usedCredits = 0;

                foreach (var code in available)
                {
                    if (!courseMeta.TryGetValue(code, out var meta)) continue;
                    if (usedCredits + meta.credits > cap) continue;

                    picked.Add(code);
                    usedCredits += meta.credits;
                }

                if (!picked.Any())
                    continue;

                foreach (var code in picked) scheduled.Add(code);

                var semesterDto = new PlannedSemesterDto
                {
                    PlannedSemester = plannedSemester,
                    TermLabel = termLabel,
                    TotalCredits = usedCredits,
                    CoursesCount = picked.Count,
                    Courses = picked.Select(code =>
                    {
                        var rec = studyMap.TryGetValue(code, out var r) ? r : 0;
                        var prereqs = prereqMap.TryGetValue(code, out var p) ? p : new List<string>();
                        var prereqOk = prereqs.All(pr => passed.Contains(pr) || scheduled.Contains(pr)); // scheduled includes picked too, but prereq should be earlier; still ok because prereq would not be in picked due to filter
                        var meta = courseMeta[code];

                        return new PlannedCourseDto
                        {
                            CourseCode = code,
                            CourseName = meta.name,
                            Credits = meta.credits,
                            IsRetake = failedNotRetaken.Contains(code),
                            RecommendedSemester = rec,
                            Prerequisites = prereqs,
                            PrereqsSatisfiedBeforeThisSemester = prereqOk
                        };
                    }).ToList()
                };

                semesters.Add(semesterDto);
            }

            // PRE-PLAN remaining numbers (before generating any plan)
            var prePlanCoursesRemaining = remaining.Count;

            var prePlanCreditsRemaining = remaining.Sum(code =>
                courseMeta.TryGetValue(code, out var m) ? m.credits : 0
            );

            // Metrics based on the produced plan + pre-plan remaining stats
            var metrics = new CoursePlanMetricsDto
            {
                // SemestersRemaining here means "Semesters planned in this generated plan"
                SemestersRemaining = semesters.Count,

                // These now represent remaining BEFORE applying the plan
                CoursesRemaining = prePlanCoursesRemaining,
                CreditsRemaining = prePlanCreditsRemaining,

                EstimatedGraduationTerm = semesters.Any()
                    ? $"Semester {semesters.Last().PlannedSemester} ({semesters.Last().TermLabel})"
                    : "N/A"
            };

            return new CoursePlanDto
            {
                PlanId = Guid.NewGuid().ToString("N"),
                Strategy = strategy,
                Semesters = semesters,
                Metrics = metrics
            };

        }

    }
}