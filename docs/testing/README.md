# EduAdvisory — Chapter VI Testing Campaign

## Folder Structure

```
docs/testing/
├── README.md                          ← This file
├── Test1_Auth/
│   ├── screenshots/                   ← Captured screenshots go here
│   └── notes.txt                      ← Observations during test run
├── Test2_AI_Assistant/
│   ├── screenshots/
│   └── notes.txt
├── Test3_Risk_Automation/
│   ├── screenshots/
│   └── notes.txt
├── Test4_Meeting/
│   ├── screenshots/
│   └── notes.txt
├── Test5_Course_Planning/
│   ├── screenshots/
│   └── notes.txt
├── sql/
│   ├── 00_identify_accounts.sql       ← Run first to find IDs
│   ├── 01_backup.sql                  ← Run before any test
│   ├── 02_restore.sql                 ← Run to revert test data
│   ├── 03_setup_test_data.sql         ← Prepares HIGH-risk + meeting data
│   ├── verify_test1_auth.sql
│   ├── verify_test2_ai.sql
│   ├── verify_test3_risk.sql
│   ├── verify_test4_meeting.sql
│   └── verify_test5_course_plan.sql
└── postman/
    └── EduAdvisory_Tests.postman_collection.json
```

## Quick Start

1. Set environment variables before starting the backend:
   - `OPENAI_API_KEY=<your-key>`

2. Run SQL scripts in order:
   ```
   00_identify_accounts.sql  → note your STUDENT_ID and ADVISOR_ID
   01_backup.sql             → create snapshot
   03_setup_test_data.sql    → inject HIGH-risk data and meeting rules
   ```

3. Import `postman/EduAdvisory_Tests.postman_collection.json` into Postman.

4. Execute tests in order: T1 → T2 → T3 → T4 → T5

5. After each test run the matching `verify_testN_*.sql`

6. After all tests: run `02_restore.sql` to clean up

## Test Accounts (Keycloak)

| Account | Username | Role | Keycloak Realm |
|---------|----------|------|----------------|
| Student | `tia.student` | STUDENT | EduAdvisory |
| Advisor | `ahmad.advisor` | ADVISOR | EduAdvisory |
| Admin | `admin` | ADMIN | EduAdvisory |
| Automation | `n8n_bot` | AutomationAdmin | EduAdvisory |

## URLs

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| Backend API | http://localhost:5267/api |
| Swagger | http://localhost:5267/swagger |
| Keycloak | http://localhost:8080 |
| n8n | http://localhost:5678 |
| DB | localhost:5432 / eduadvisory_db |
