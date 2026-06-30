-- Mark all existing chat messages as read on fresh Docker startup
-- so advisors and students don't see stale unread counts from the backup.
UPDATE public.chat_message SET is_read = true WHERE is_read = false;
