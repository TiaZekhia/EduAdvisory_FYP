-- ============================================================
-- EduAdvisory — Verify Test 2: AI Assistant
-- Run after completing Test 2 execution.
-- ============================================================

SELECT '=== TEST 2: AI ASSISTANT VERIFICATION ===' AS section;

-- 1. Verify document is processed with embedded chunks
SELECT
    d.document_id,
    d.title,
    d.course_code,
    d.document_type,
    d.scope,
    d.status,
    d.processed_at,
    COUNT(c.chunk_id)                                          AS total_chunks,
    COUNT(c.chunk_id) FILTER (WHERE c.embedding IS NOT NULL)   AS embedded_chunks,
    ROUND(AVG(c.token_count)::numeric, 1)                      AS avg_tokens_per_chunk,
    MIN(c.page_number)                                         AS first_page,
    MAX(c.page_number)                                         AS last_page
FROM ai_documents d
LEFT JOIN ai_document_chunks c ON c.document_id = d.document_id
GROUP BY d.document_id, d.title, d.course_code, d.document_type,
         d.scope, d.status, d.processed_at
ORDER BY d.created_at DESC;
-- Expected: at least 1 row with status='Processed', embedded_chunks > 0

-- 2. Verify AI chat sessions were created for the test student
SELECT
    s.session_id,
    s.student_id,
    s.started_at,
    s.last_activity_at,
    COUNT(m.message_id)                                           AS total_messages,
    COUNT(m.message_id) FILTER (WHERE m.role = 'user')            AS user_turns,
    COUNT(m.message_id) FILTER (WHERE m.role = 'assistant')       AS assistant_turns
FROM ai_chat_sessions s
LEFT JOIN ai_chat_messages m ON m.session_id = s.session_id
WHERE s.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY s.session_id, s.student_id, s.started_at, s.last_activity_at
ORDER BY s.started_at DESC
LIMIT 5;

-- 3. Verify retrieval audit log — check all 3 response types
SELECT
    retrieval_log_id,
    LEFT(question, 70)               AS question_preview,
    response_source,
    ROUND(top_similarity_score::numeric, 4) AS similarity_score,
    plugin_used,
    CASE
        WHEN retrieved_chunk_ids IS NULL OR retrieved_chunk_ids = '' THEN 'NONE'
        ELSE retrieved_chunk_ids
    END                              AS chunk_ids,
    created_at
FROM ai_retrieval_logs
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY created_at DESC
LIMIT 10;
-- Expected rows:
--   response_source='rag':           similarity >= 0.75, chunk_ids not empty
--   response_source='plugin':        similarity < 0.75 OR null, plugin_used not null
--   response_source='not_supported': similarity < 0.75, chunk_ids empty

-- 4. Summary by response_source
SELECT
    response_source,
    COUNT(*)                              AS occurrences,
    ROUND(AVG(top_similarity_score)::numeric, 4) AS avg_similarity
FROM ai_retrieval_logs
WHERE student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
GROUP BY response_source
ORDER BY response_source;

-- 5. Verify chat message content was saved
SELECT
    m.message_id,
    m.role,
    LEFT(m.message, 80) AS message_preview,
    m.created_at
FROM ai_chat_messages m
JOIN ai_chat_sessions s ON s.session_id = m.session_id
WHERE s.student_id = (SELECT linked_student_id FROM users WHERE username = 'tia.student')
ORDER BY m.created_at DESC
LIMIT 10;
