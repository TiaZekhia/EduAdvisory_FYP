import jsPDF from "jspdf";

export function decodeHtmlDeep(input, maxPasses = 3) {
  if (input == null) return "";
  let text = typeof input === "string" ? input : String(input);

  for (let i = 0; i < maxPasses; i++) {
    const txt = document.createElement("textarea");
    txt.innerHTML = text;
    const decoded = txt.value;
    if (decoded === text) break;
    text = decoded;
  }

  return text
    .replace(/\u00A0/g, " ")
    .replace(/&nbsp;/g, " ")
    .replace(/[ \t]+/g, " ")
    .replace(/\s+\n/g, "\n")
    .trim();
}

function cleanString(s) {
  return decodeHtmlDeep(s);
}

function cleanStringArray(arr) {
  if (!Array.isArray(arr)) return [];
  return arr.map(cleanString).filter(Boolean);
}

export function exportCoursePlanPdf({
  selected,
  selectedPlanIndex,
  summary,
  insights,
  selectedInsight,
  selectedRank,
  aiLoading,
}) {
  if (!selected) throw new Error("No plan selected to export.");

  const doc = new jsPDF("p", "mm", "a4");

  const pageW = doc.internal.pageSize.getWidth();
  const pageH = doc.internal.pageSize.getHeight();

  const marginX = 14;
  const marginTop = 14;
  const marginBottom = 14;

  const contentW = pageW - marginX * 2;
  let y = marginTop;

  const lineGap = 5;

  const ensureSpace = (needed = 10) => {
    if (y + needed > pageH - marginBottom) {
      doc.addPage();
      y = marginTop;
    }
  };

  const addTitle = (text) => {
    ensureSpace(10);
    doc.setFont("helvetica", "bold");
    doc.setFontSize(16);
    doc.text(String(text), marginX, y);
    y += 8;
  };

  const addH2 = (text) => {
    ensureSpace(8);
    doc.setFont("helvetica", "bold");
    doc.setFontSize(12);
    doc.text(String(text), marginX, y);
    y += 6;
  };

  const addMuted = (text) => {
    ensureSpace(6);
    doc.setFont("helvetica", "normal");
    doc.setFontSize(10);
    doc.setTextColor(90);
    const lines = doc.splitTextToSize(cleanString(text), contentW);
    doc.text(lines, marginX, y);
    y += lines.length * lineGap;
    doc.setTextColor(0);
  };

  const addParagraph = (text) => {
    ensureSpace(6);
    doc.setFont("helvetica", "normal");
    doc.setFontSize(11);
    const lines = doc.splitTextToSize(cleanString(text), contentW);
    doc.text(lines, marginX, y);
    y += lines.length * lineGap;
  };

  const addKeyValueRow = (pairs) => {
    doc.setFont("helvetica", "normal");
    doc.setFontSize(10);
    const colGap = 6;
    const colW = (contentW - colGap) / 2;

    for (let i = 0; i < pairs.length; i += 2) {
      ensureSpace(8);

      const left = pairs[i];
      const right = pairs[i + 1];

      doc.setFont("helvetica", "bold");
      doc.text(`${left.k}: ${cleanString(left.v ?? "-")}`, marginX, y);

      if (right) {
        doc.text(
          `${right.k}: ${cleanString(right.v ?? "-")}`,
          marginX + colW + colGap,
          y
        );
      }

      y += 6;
    }

    y += 2;
    doc.setFont("helvetica", "normal");
  };

  const addBullets = (items) => {
    const list = cleanStringArray(items);
    if (!list.length) {
      addMuted("None");
      return;
    }

    doc.setFont("helvetica", "normal");
    doc.setFontSize(11);

    const bulletIndent = 4;
    const textIndent = 8;

    list.forEach((it) => {
      const lines = doc.splitTextToSize(it, contentW - textIndent);
      ensureSpace(6 + lines.length * lineGap);

      doc.text("•", marginX + bulletIndent, y);
      doc.text(lines, marginX + textIndent, y);
      y += lines.length * lineGap;
    });

    y += 2;
  };

  const addDivider = () => {
    ensureSpace(6);
    doc.setDrawColor(220);
    doc.line(marginX, y, pageW - marginX, y);
    y += 6;
    doc.setDrawColor(0);
  };

  // ---- Content ----
  const planNumber = (selectedPlanIndex ?? 0) + 1;
  const program = summary?.programCode || "Program";
  const semester = summary?.currentSemester ?? "-";
  const exportedOn = new Date().toLocaleString();

  addTitle(`Course Plan (Plan ${planNumber})`);
  addMuted(`${program} • Semester ${semester}`);
  addMuted(`Exported on: ${exportedOn}`);
  addDivider();

  addH2("Overview");
  addKeyValueRow([
    { k: "Strategy", v: selected.strategy },
    { k: "Est. Graduation", v: selected.metrics?.estimatedGraduationTerm },
    { k: "Semesters Planned", v: selected.semesters?.length ?? 0 },
    { k: "Courses Remaining", v: selected.metrics?.coursesRemaining ?? 0 },
    { k: "Credits Remaining", v: selected.metrics?.creditsRemaining ?? 0 },
    { k: "Plan ID", v: selected.planId ?? "-" },
  ]);
  addDivider();

  addH2("AI Recommendation");
  if (aiLoading) {
    addMuted("AI insights were still generating when this PDF was exported.");
  } else if (insights?.bestPlanSummary) {
    addParagraph(insights.bestPlanSummary);
    if (typeof insights.bestPlanIndex === "number") {
      addMuted(`Recommended Plan: ${insights.bestPlanIndex + 1}`);
    }
  } else {
    addMuted("No AI recommendation available.");
  }
  addDivider();

  addH2("Plan Explanation");
  if (aiLoading) {
    addMuted("AI explanation was still generating when this PDF was exported.");
  } else if (selectedInsight) {
    addMuted(
      `Score: ${selectedInsight.score}/100` +
        (selectedRank ? ` • Rank: #${selectedRank}` : "")
    );

    addParagraph(selectedInsight.explanation || "No explanation provided.");

    addH2("Pros");
    addBullets(selectedInsight.pros);

    addH2("Cons");
    addBullets(selectedInsight.cons);

    addH2("Warnings");
    addBullets(
      selectedInsight.warnings?.length ? selectedInsight.warnings : ["None"]
    );
  } else {
    addMuted("No AI explanation available for this plan.");
  }
  addDivider();

  addH2("Semesters");
  (selected.semesters ?? []).forEach((sem, idx) => {
    ensureSpace(10);

    doc.setFont("helvetica", "bold");
    doc.setFontSize(12);
    doc.text(
      cleanString(
        `Semester ${sem.plannedSemester}: ${sem.termLabel} (${sem.coursesCount} course(s), ${sem.totalCredits} credits)`
      ),
      marginX,
      y
    );
    y += 6;

    (sem.courses ?? []).forEach((c) => {
      const header = `${c.courseCode} — ${c.courseName}`;
      const meta = `Credits: ${c.credits} • ${
        c.prereqsSatisfiedBeforeThisSemester ? "Eligible" : "Blocked"
      }${c.isRetake ? " • Retake" : ""}`;
      const prereq = `Prerequisites: ${
        c.prerequisites?.length ? c.prerequisites.join(", ") : "None"
      }`;

      doc.setFont("helvetica", "bold");
      doc.setFontSize(11);
      const h1 = doc.splitTextToSize(cleanString(header), contentW);

      doc.setFont("helvetica", "normal");
      doc.setFontSize(10);
      const h2 = doc.splitTextToSize(cleanString(meta), contentW);
      const h3 = doc.splitTextToSize(cleanString(prereq), contentW);

      const needed = (h1.length + h2.length + h3.length) * lineGap + 4;
      ensureSpace(needed);

      doc.setFont("helvetica", "bold");
      doc.setFontSize(11);
      doc.text(h1, marginX, y);
      y += h1.length * lineGap;

      doc.setFont("helvetica", "normal");
      doc.setFontSize(10);
      doc.setTextColor(80);
      doc.text(h2, marginX, y);
      y += h2.length * lineGap;

      doc.text(h3, marginX, y);
      y += h3.length * lineGap;

      doc.setTextColor(0);
      y += 2;
    });

    if (idx !== (selected.semesters?.length ?? 0) - 1) addDivider();
  });

  const filename = `${program}_Plan_${planNumber}.pdf`;
  doc.save(filename);
}