# EduAdvisory — Screenshot Checklist for Chapter VI

Save screenshots into the matching test folder:
- `Test1_Auth/screenshots/`
- `Test2_AI_Assistant/screenshots/`
- `Test3_Risk_Automation/screenshots/`
- `Test4_Meeting/screenshots/`
- `Test5_Course_Planning/screenshots/`

---

## TEST 1 — Authentication & RBAC

### MANDATORY (must appear in report)

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC1-A | `jwt_decoded_student.png` | jwt.io showing decoded student JWT with `realm_access.roles: ["STUDENT"]` |
| SC1-B | `403_student_hits_admin.png` | Postman: `GET /api/admin/users` with student token → **403** response |
| SC1-C | `401_no_token.png` | Postman: `GET /api/profile/me` with no token → **401** response |
| SC1-D | `200_automation_admin.png` | Postman: `POST /api/automation/risk-interventions/run` with AutomationAdmin → **200** |

### OPTIONAL

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC1-E | `keycloak_roles.png` | Keycloak admin → Realm roles list |
| SC1-F | `student_dashboard.png` | Student dashboard in browser (`/student/dashboard`) |
| SC1-G | `advisor_dashboard.png` | Advisor dashboard in browser (`/advisor/dashboard`) |

---

## TEST 2 — AI Assistant

### MANDATORY (must appear in report)

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC2-A | `rag_response_with_sources.png` | AI chat page: RAG answer + **Sources panel open** showing doc title, page #, similarity score |
| SC2-B | `plugin_response.png` | AI chat page: Plugin answer showing student's real GPA and course count |
| SC2-C | `not_supported_response.png` | AI chat page: Out-of-scope question response with `responseSource: not_supported` |

### OPTIONAL

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC2-D | `streaming_mid_response.png` | Mid-stream screenshot showing tokens appearing token-by-token |
| SC2-E | `document_processed_admin.png` | Admin knowledge base page showing document `status=Processed` |
| SC2-F | `retrieval_logs_db.png` | DB query: `ai_retrieval_logs` showing 3 rows (rag / plugin / not_supported) |

---

## TEST 3 — Risk Assessment + n8n Automation

### MANDATORY (must appear in report)

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC3-A | `automation_run_response.png` | Postman: automation run → **200** JSON with `riskLevel:"HIGH"`, `actionType:"HIGH_RISK_MEETING_RECOMMENDATION"`, `status:"COMPLETED"` |
| SC3-B | `high_risk_alert_message.png` | Advisor's messages page: `[HIGH RISK ALERT]` message visible in conversation |
| SC3-C | `cooldown_skipped.png` | Postman: 2nd automation run → `status:"SKIPPED_DUPLICATE"` in response |

### OPTIONAL

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC3-D | `intervention_log_db.png` | DB: `risk_intervention_log` — 2 rows (COMPLETED → SKIPPED_DUPLICATE) |
| SC3-E | `n8n_execution_history.png` | n8n: execution history showing successful HTTP POST call |
| SC3-F | `student_risk_data_db.png` | DB: `student_risk` table row with `risk_level=HIGH`, `risk_score >= 70` |

---

## TEST 4 — Meeting Scheduling + Google Meet

### MANDATORY (must appear in report)

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC4-A | `meeting_request_pending.png` | Student meetings page: new request visible with `status=PENDING` |
| SC4-B | `meet_link_confirmed.png` | Advisor upcoming meetings page: confirmed meeting with **Google Meet link** visible (`meet.google.com/...`) |
| SC4-C | `meet_link_student_view.png` | Student upcoming meetings page: same meeting + same Meet link visible |

### OPTIONAL

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC4-D | `meeting_request_form.png` | Student meeting request form before submitting |
| SC4-E | `advisor_pending_requests.png` | Advisor's incoming requests list showing the student's request |
| SC4-F | `google_meet_opened.png` | Meet link clicked — opens in browser (shows `meet.google.com` in URL bar) |

---

## TEST 5 — AI Course Planning

### MANDATORY (must appear in report)

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC5-A | `course_plans_generated.png` | Course planning page: **2–3 plan cards** visible (Balanced, FastBlockers, StudyGuideStrict) |
| SC5-B | `ai_insights_best_plan.png` | AI Insights panel: `score`, `explanation`, `pros`, `cons` for the Best Plan |
| SC5-C | `pdf_exported.png` | Downloaded PDF open: shows semester-grouped course listing |

### OPTIONAL

| ID | Filename | What to Capture |
|----|----------|----------------|
| SC5-D | `plan_scores_comparison.png` | All 3 plans' scores visible for comparison |
| SC5-E | `prereq_evidence.png` | A course's prerequisite appears in an earlier semester (annotated in the plan) |
| SC5-F | `generated_plan_db.png` | DB: `generated_study_plan` rows for this student |

---

## TOTAL SCREENSHOT COUNT

| Category | Mandatory | Optional | Total |
|----------|-----------|----------|-------|
| Test 1 Auth | 4 | 3 | 7 |
| Test 2 AI | 3 | 3 | 6 |
| Test 3 Risk | 3 | 3 | 6 |
| Test 4 Meeting | 3 | 3 | 6 |
| Test 5 Plans | 3 | 3 | 6 |
| **TOTAL** | **16** | **15** | **31** |

**Minimum for Chapter VI: 16 mandatory screenshots.**
