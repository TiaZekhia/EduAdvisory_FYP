import { useMemo, useState } from "react";
import "./UpcomingMeetingsCalendar.css";

const DOW_LABELS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
const MONTH_NAMES = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

function toKey(date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;
}

function fmtTime(date) {
  return date.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: true });
}

function fmtDayLong(date) {
  return date.toLocaleDateString(undefined, { weekday: "long", month: "long", day: "numeric" });
}

function fmtDayShort(date) {
  return date.toLocaleDateString(undefined, { weekday: "short", month: "short", day: "numeric" });
}

/* ─── Public component ─────────────────────────────────────────────────────── */

export default function UpcomingMeetingsCalendar({
  meetings = [],
  nameField = "studentName",
  title = "Upcoming Meetings",
  subtitle = null,
}) {
  const today = new Date();
  const todayKey = toKey(today);

  const [viewYear, setViewYear] = useState(today.getFullYear());
  const [viewMonth, setViewMonth] = useState(today.getMonth());
  const [selectedKey, setSelectedKey] = useState(todayKey);

  /* Build date→meetings map */
  const byDay = useMemo(() => {
    const map = {};
    meetings.forEach((m) => {
      const k = toKey(new Date(m.startAt));
      if (!map[k]) map[k] = [];
      map[k].push(m);
    });
    return map;
  }, [meetings]);

  /* Calendar grid cells */
  const firstDow = new Date(viewYear, viewMonth, 1).getDay();
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
  const cells = [];
  for (let i = 0; i < firstDow; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  while (cells.length % 7 !== 0) cells.push(null);

  function prevMonth() {
    if (viewMonth === 0) { setViewYear((y) => y - 1); setViewMonth(11); }
    else setViewMonth((m) => m - 1);
  }
  function nextMonth() {
    if (viewMonth === 11) { setViewYear((y) => y + 1); setViewMonth(0); }
    else setViewMonth((m) => m + 1);
  }

  /* Selected day meetings sorted by time */
  const selectedMeetings = (byDay[selectedKey] ?? [])
    .slice()
    .sort((a, b) => new Date(a.startAt) - new Date(b.startAt));

  /* Next upcoming meetings (for "what's next" when selected day is empty) */
  const nextUpcoming = useMemo(
    () =>
      [...meetings]
        .filter((m) => new Date(m.startAt) >= today)
        .sort((a, b) => new Date(a.startAt) - new Date(b.startAt))
        .slice(0, 3),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [meetings]
  );

  const totalUpcoming = meetings.filter((m) => new Date(m.startAt) >= today).length;

  return (
    <div className="ucal-wrapper">
      {/* Card header */}
      <div className="ucal-card-header">
        <div>
          <div className="ucal-card-title">{title}</div>
          <div className="ucal-card-subtitle">
            {subtitle ?? `${totalUpcoming} upcoming meeting${totalUpcoming !== 1 ? "s" : ""}`}
          </div>
        </div>
        {totalUpcoming > 0 && (
          <div className="ucal-count-badge">
            <i className="pi pi-calendar" />
            {totalUpcoming}
          </div>
        )}
      </div>

      {/* Body: calendar + agenda side by side */}
      <div className="ucal-body">

        {/* ── Mini calendar ───────────────────────────── */}
        <div className="ucal-mini-cal">
          <div className="ucal-nav">
            <button type="button" className="ucal-nav-btn" onClick={prevMonth}>
              <i className="pi pi-chevron-left" />
            </button>
            <span className="ucal-month-label">
              {MONTH_NAMES[viewMonth]} {viewYear}
            </span>
            <button type="button" className="ucal-nav-btn" onClick={nextMonth}>
              <i className="pi pi-chevron-right" />
            </button>
          </div>

          <div className="ucal-grid">
            {DOW_LABELS.map((d) => (
              <div key={d} className="ucal-dow">{d}</div>
            ))}

            {cells.map((day, idx) => {
              if (!day) return <div key={`pad-${idx}`} className="ucal-day ucal-day--empty" />;

              const key = `${viewYear}-${String(viewMonth + 1).padStart(2, "0")}-${String(day).padStart(2, "0")}`;
              const isToday = key === todayKey;
              const isSelected = key === selectedKey;
              const hasMeeting = !!byDay[key];

              return (
                <button
                  key={key}
                  type="button"
                  className={[
                    "ucal-day",
                    isToday ? "ucal-day--today" : "",
                    isSelected ? "ucal-day--selected" : "",
                    hasMeeting ? "ucal-day--has-meeting" : "",
                  ]
                    .filter(Boolean)
                    .join(" ")}
                  onClick={() => setSelectedKey(key)}
                >
                  {day}
                  {hasMeeting && <span className="ucal-dot" />}
                </button>
              );
            })}
          </div>

          {/* Legend */}
          <div className="ucal-legend">
            <span className="ucal-legend-item">
              <span className="ucal-legend-dot ucal-legend-dot--today" />
              Today
            </span>
            <span className="ucal-legend-item">
              <span className="ucal-legend-dot ucal-legend-dot--meeting" />
              Meeting
            </span>
          </div>
        </div>

        {/* ── Agenda panel ─────────────────────────────── */}
        <div className="ucal-agenda">
          {selectedMeetings.length > 0 ? (
            <>
              <div className="ucal-agenda-date-label">
                <i className="pi pi-calendar ucal-agenda-date-icon" />
                {fmtDayLong(new Date(selectedKey + "T00:00:00"))}
                <span className="ucal-agenda-count-pill">
                  {selectedMeetings.length}
                </span>
              </div>

              <div className="ucal-agenda-list">
                {selectedMeetings.map((m) => (
                  <AgendaItem key={m.meetingId} meeting={m} nameField={nameField} />
                ))}
              </div>
            </>
          ) : (
            <div className="ucal-agenda-empty-state">
              <div className="ucal-agenda-date-label ucal-agenda-date-label--muted">
                <i className="pi pi-calendar ucal-agenda-date-icon" />
                {fmtDayLong(new Date(selectedKey + "T00:00:00"))}
              </div>

              <div className="ucal-no-meetings">
                <i className="pi pi-calendar-times ucal-no-meetings-icon" />
                <span>No meetings on this day</span>
              </div>

              {nextUpcoming.length > 0 && (
                <div className="ucal-next-section">
                  <div className="ucal-next-label">Next upcoming</div>
                  {nextUpcoming.map((m) => (
                    <AgendaItem
                      key={m.meetingId}
                      meeting={m}
                      nameField={nameField}
                      showDate
                      compact
                    />
                  ))}
                </div>
              )}

              {meetings.length === 0 && (
                <div className="ucal-all-empty">
                  <i className="pi pi-check-circle ucal-all-empty-icon" />
                  <div className="ucal-all-empty-text">No upcoming meetings scheduled</div>
                </div>
              )}
            </div>
          )}
        </div>

      </div>
    </div>
  );
}

/* ─── Agenda item ──────────────────────────────────────────────────────────── */

function AgendaItem({ meeting, nameField, compact = false, showDate = false }) {
  const start = new Date(meeting.startAt);
  const end = new Date(meeting.endAt);
  const hasLink = meeting.meetingLink?.trim();

  /* Color stripe based on meeting type keyword */
  const type = (meeting.title ?? "").toLowerCase();
  const stripeClass =
    type.includes("progress") ? "ucal-item--blue"
    : type.includes("course") ? "ucal-item--purple"
    : type.includes("probation") || type.includes("risk") ? "ucal-item--red"
    : "ucal-item--green";

  return (
    <div className={`ucal-agenda-item ${stripeClass} ${compact ? "ucal-agenda-item--compact" : ""}`}>
      <div className="ucal-item-time">
        <span className="ucal-item-time-start">{fmtTime(start)}</span>
        <span className="ucal-item-time-sep">–</span>
        <span className="ucal-item-time-end">{fmtTime(end)}</span>
      </div>

      <div className="ucal-item-body">
        {showDate && (
          <div className="ucal-item-date-chip">
            <i className="pi pi-calendar" />
            {fmtDayShort(start)}
          </div>
        )}
        <div className="ucal-item-title">{meeting.title}</div>
        {meeting[nameField] && (
          <div className="ucal-item-person">
            <i className="pi pi-user" />
            {meeting[nameField]}
          </div>
        )}
        {hasLink && (
          <a
            href={meeting.meetingLink}
            target="_blank"
            rel="noreferrer"
            className="ucal-join-btn"
          >
            <i className="pi pi-video" />
            Join meeting
          </a>
        )}
      </div>
    </div>
  );
}
