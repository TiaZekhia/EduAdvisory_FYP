import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { Dialog } from "primereact/dialog";
import { Button } from "primereact/button";
import { Calendar } from "primereact/calendar";
import { Dropdown } from "primereact/dropdown";
import { InputTextarea } from "primereact/inputtextarea";
import { Message } from "primereact/message";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMeetingsApi } from "../../../services/students/studentMeetingsApi";
import { PageHero } from "../../../shared/components/PageHero";
import "./MeetingsPage.css";

const durationOptions = [
  { label: "15 min", value: 15 },
  { label: "30 min", value: 30 },
  { label: "45 min", value: 45 },
  { label: "1 hour", value: 60 },
];

export default function MeetingsPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [actionMsg, setActionMsg] = useState("");

  const [advisor, setAdvisor] = useState(null);
  const [upcoming, setUpcoming] = useState([]);
  const [history, setHistory] = useState([]);
  const [requests, setRequests] = useState([]);

  const [calendarDate, setCalendarDate] = useState(new Date());
  const [calendarStartTimes, setCalendarStartTimes] = useState([]);
  const [calendarLoading, setCalendarLoading] = useState(false);

  const [requestDialogOpen, setRequestDialogOpen] = useState(false);
  const [selectedStartTime, setSelectedStartTime] = useState(null);
  const [selectedDuration, setSelectedDuration] = useState(null);
  const [reason, setReason] = useState("");
  const [savingRequest, setSavingRequest] = useState(false);
  const [actionError, setActionError] = useState("");

  const pendingRequestsCount = useMemo(
    () => requests.filter((r) => r.status === "PENDING").length,
    [requests]
  );

  const availableCount = useMemo(() => calendarStartTimes.length, [calendarStartTimes]);

  const loadBaseData = async () => {
    setLoading(true);
    setErr("");

    try {
      const [advisorRes, upcomingRes, historyRes, requestsRes] = await Promise.all([
        studentMeetingsApi.getAdvisor(),
        studentMeetingsApi.getUpcoming(),
        studentMeetingsApi.getHistory(),
        studentMeetingsApi.getRequests(),
      ]);

      setAdvisor(advisorRes.data || null);
      setUpcoming((upcomingRes.data || []).filter(m => new Date(m.endAt ?? m.startAt) > new Date()));      setHistory(historyRes.data || []);
      setRequests(requestsRes.data || []);
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to load meetings page.");
    } finally {
      setLoading(false);
    }
  };

  const loadCalendar = async (date) => {
    try {
      setCalendarLoading(true);
      const yyyyMmDd = toDateOnly(date);
      const res = await studentMeetingsApi.getAdvisorCalendar(yyyyMmDd);
      setCalendarStartTimes(res.data || []);
    } catch (e) {
      console.error(e);
      setCalendarStartTimes([]);
      setErr(e?.response?.data ?? e?.message ?? "Failed to load advisor calendar.");
    } finally {
      setCalendarLoading(false);
    }
  };

  useEffect(() => {
    loadBaseData();
  }, []);

  useEffect(() => {
    if (calendarDate) loadCalendar(calendarDate);
  }, [calendarDate]);

  const handleOpenRequest = (startTimeItem) => {
    setSelectedStartTime(startTimeItem);
    setSelectedDuration(startTimeItem.allowedDurations?.[0] ?? null);
    setReason("");
    setActionError("");
    setRequestDialogOpen(true);
  };

  const handleSubmitRequest = async () => {
    if (!selectedStartTime || !selectedDuration) return;

    if (!reason.trim()) {
      setActionError("Please write a short reason for the meeting.");
      return;
    }

    try {
      setSavingRequest(true);
      setActionError("");

      await studentMeetingsApi.createRequest({
        startAt: selectedStartTime.startAt,
        durationMinutes: selectedDuration,
        reason: reason.trim(),
      });

      setActionMsg("Meeting request submitted successfully.");
      setRequestDialogOpen(false);
      await Promise.all([loadBaseData(), loadCalendar(calendarDate)]);
    } catch (e) {
      console.error(e);
      setActionError(e?.response?.data ?? e?.message ?? "Failed to submit request.");
    } finally {
      setSavingRequest(false);
    }
  };

  const handleCancelRequest = async (requestId) => {
    try {
      setErr("");
      setActionMsg("");
      await studentMeetingsApi.cancelRequest(requestId);
      setActionMsg("Meeting request canceled successfully.");
      await Promise.all([loadBaseData(), loadCalendar(calendarDate)]);
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to cancel request.");
    }
  };

  if (loading) {
    return (
      <div className="meetings-page container-fluid p-4 p-lg-5">
        <Skeleton width="18rem" height="2rem" className="mb-3" />
        <Skeleton height="10rem" className="mb-4" />
        <Skeleton height="14rem" className="mb-4" />
        <Skeleton height="16rem" className="mb-4" />
        <Skeleton height="16rem" />
      </div>
    );
  }

  if (err && !advisor && !upcoming.length && !history.length && !requests.length) {
    return <div className="p-4 text-danger">{String(err)}</div>;
  }

  return (
    <div className="meetings-page container-fluid p-3 p-md-4">
      <PageHero
        title="Meetings"
        badge={`${summary?.programCode ?? "-"} • Semester ${summary?.currentSemester ?? "-"}`}
        subtitle="Request and manage advising meetings"
      />

      {actionMsg ? <Message severity="success" text={actionMsg} className="mb-4" /> : null}
      {err ? <Message severity="error" text={String(err)} className="mb-4" /> : null}

      <div className="row g-4 mb-4">
        <CountCard title="Upcoming Meetings" value={upcoming.length} icon="pi pi-calendar-plus" />
        <CountCard title="Past Meetings" value={history.length} icon="pi pi-history" />
        <CountCard title="Pending Requests" value={pendingRequestsCount} icon="pi pi-send" />
      </div>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Request a Meeting</div>
            <div className="text-muted mt-1">
              Choose a date, then request a time with your advisor
            </div>
          </div>

          <Tag
            value={`${availableCount} start times`}
            severity={availableCount ? "success" : "warning"}
            className="meeting-status-tag"
          />
        </div>

        <div className="meeting-request-layout">
          <div className="meeting-request-sidebar">
            <div className="meeting-advisor-box h-100">
              <div className="d-flex align-items-center justify-content-between flex-wrap gap-3 mb-3">
                <div>
                  <div className="meeting-advisor-label">Your Academic Advisor</div>
                  <div className="meeting-advisor-name">{advisor?.name ?? "Not assigned"}</div>
                </div>

                <Tag
                  value={availableCount ? "Available" : "No Availability"}
                  severity={availableCount ? "success" : "warning"}
                  className="meeting-status-tag"
                />
              </div>

              <div className="meeting-advisor-summary mb-3">
                Select a date and request a meeting time. Your advisor will review the request and
                approve or reject it.
              </div>

              <div className="row g-3">
                <div className="col-12">
                  <div className="meeting-info-tile">
                    <div className="meeting-info-label">
                      <i className="pi pi-envelope" />
                      Email
                    </div>
                    <div className="meeting-info-value">{advisor?.email ?? "-"}</div>
                  </div>
                </div>

                <div className="col-12">
                  <div className="meeting-info-tile">
                    <div className="meeting-info-label">
                      <i className="pi pi-building" />
                      Office
                    </div>
                    <div className="meeting-info-value">{advisor?.office ?? "-"}</div>
                  </div>
                </div>
              </div>

              <div className="meeting-reminder-box mt-3">
                Allowed durations: 15, 30, 45, or 60 minutes.
              </div>
            </div>
          </div>

          <div className="meeting-request-main">
            <div className="mb-4">
              <label className="meeting-advisor-label d-block mb-2">Choose Date</label>
              <Calendar
                value={calendarDate}
                onChange={(e) => setCalendarDate(e.value)}
                dateFormat="yy-mm-dd"
                className="w-100"
                showIcon
              />
            </div>

            <div className="meeting-subsection-header mb-3">
              <div>
                <div className="meeting-subsection-title">Available Start Times</div>
                <div className="meeting-subsection-text">{formatDateLong(calendarDate)}</div>
              </div>
            </div>

            {calendarLoading ? (
              <Skeleton height="14rem" />
            ) : calendarStartTimes.length ? (
              <TimelineSlotPicker slots={calendarStartTimes} onSelect={handleOpenRequest} />
            ) : (
              <EmptyState
                icon="pi pi-calendar"
                title="No available times"
                text="There is no advisor availability for the selected date."
              />
            )}
          </div>
        </div>
      </Card>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">My Requests</div>
            <div className="text-muted mt-1">Requests you submitted to your advisor</div>
          </div>

          <Tag
            value={`${pendingRequestsCount} Pending`}
            severity={pendingRequestsCount ? "warning" : "info"}
            className="meeting-status-tag"
          />
        </div>

        {requests?.length ? (
          <div className="d-flex flex-column gap-3">
            {requests.map((r) => (
              <RequestRow key={r.requestId} request={r} onCancel={handleCancelRequest} />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-inbox"
            title="No meeting requests"
            text="Your submitted meeting requests will appear here."
          />
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Upcoming Meetings</div>
            <div className="text-muted mt-1">Approved upcoming meetings</div>
          </div>
        </div>

        {upcoming?.length ? (
          <div className="d-flex flex-column gap-3">
            {upcoming.map((meeting) => (
              <UpcomingMeetingRow key={meeting.meetingId} meeting={meeting} />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-calendar"
            title="No upcoming meetings"
            text="Approved meetings will appear here."
          />
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Meeting History</div>
            <div className="text-muted mt-1">Past meetings and notes</div>
          </div>
        </div>

        {history?.length ? (
          <div className="d-flex flex-column gap-3">
            {history.map((m) => (
              <MeetingHistoryRow key={m.meetingId} meeting={m} />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-history"
            title="No past meetings"
            text="Your completed meetings will appear here."
          />
        )}
      </Card>

      <Dialog
        header="Request Meeting"
        visible={requestDialogOpen}
        style={{ width: "38rem", maxWidth: "95vw" }}
        onHide={() => setRequestDialogOpen(false)}
      >
        {selectedStartTime ? (
          <div>
            <div className="meeting-dialog-banner mb-3">
              <div className="meeting-dialog-banner-icon">
                <i className="pi pi-calendar-plus" />
              </div>

              <div>
                <div className="meeting-dialog-banner-title">Selected Start Time</div>
                <div className="meeting-dialog-banner-text">
                  {formatDateLong(selectedStartTime.startAt)} • {formatTime(selectedStartTime.startAt)}
                </div>
              </div>
            </div>

            <div className="mb-3">
              <label className="meeting-advisor-label d-block mb-2">Duration</label>
              <Dropdown
                value={selectedDuration}
                onChange={(e) => setSelectedDuration(e.value)}
                options={durationOptions.filter((x) =>
                  selectedStartTime.allowedDurations.includes(x.value)
                )}
                className="w-100"
              />
            </div>

            <div className="meeting-reminder-box mb-3">
              Tell your advisor what you want to discuss so they can prepare before reviewing your request.
            </div>

            <label className="meeting-advisor-label mb-2 d-block">Reason for meeting</label>
            <InputTextarea
              rows={5}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Example: I want help choosing courses for next semester and reviewing my progress."
              className="w-100"
            />

            <div className="meeting-text-counter mt-2">{reason.trim().length} characters</div>

            {actionError ? <div className="text-danger mt-3">{actionError}</div> : null}

            <div className="d-flex justify-content-end gap-2 mt-4">
              <Button
                label="Cancel"
                className="p-button-text"
                onClick={() => setRequestDialogOpen(false)}
              />
              <Button
                label={savingRequest ? "Submitting..." : "Submit Request"}
                icon="pi pi-check"
                onClick={handleSubmitRequest}
                disabled={savingRequest || !reason.trim() || !selectedDuration}
              />
            </div>
          </div>
        ) : null}
      </Dialog>
    </div>
  );
}

function CountCard({ title, value, icon }) {
  return (
    <div className="col-12 col-md-4">
      <div className="meeting-stat-card h-100">
        <div className="meeting-stat-card-top">
          <div className="meeting-stat-icon">
            <i className={icon} />
          </div>
          <div className="meeting-stat-title">{title}</div>
        </div>
        <div className="meeting-stat-value">{value}</div>
      </div>
    </div>
  );
}

function EmptyState({ icon, title, text }) {
  return (
    <div className="meetings-empty-state">
      <div className="meetings-empty-icon">
        <i className={icon} />
      </div>
      <div>
        <div className="meetings-empty-title">{title}</div>
        <div className="meetings-empty-text">{text}</div>
      </div>
    </div>
  );
}

function UpcomingMeetingRow({ meeting }) {
  const hasLink = meeting.meetingLink?.trim();
  const accent = meetingAccentColor(meeting.title);
  const ini = nameInitials(meeting.advisorName);

  return (
    <div className="mcard" style={{ "--accent": accent }}>
      <div className="mcard-accent" />
      <div className="mcard-body">
        <div className="mcard-timecol">
          <span className="mcard-time-start">{formatTime(meeting.startAt)}</span>
          <span className="mcard-time-sep">–</span>
          <span className="mcard-time-end">{formatTime(meeting.endAt)}</span>
        </div>
        <div className="mcard-content">
          <div className="d-flex align-items-start justify-content-between gap-2 flex-wrap">
            <div className="mcard-title">{meeting.title}</div>
            <Tag value={meeting.status} severity="info" className="meeting-status-tag" />
          </div>
          <div className="mcard-meta">
            <span className="mcard-person">
              <span className="mcard-avatar" style={{ background: accent }}>{ini}</span>
              {meeting.advisorName}
            </span>
            <span className="mcard-date-chip">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
          </div>
          {hasLink ? (
            <a href={meeting.meetingLink} target="_blank" rel="noreferrer" className="mcard-join-btn">
              <i className="pi pi-video" /> Join Meeting
            </a>
          ) : (
            <span className="mcard-link-pending">
              <i className="pi pi-clock" /> Meeting link pending approval
            </span>
          )}
        </div>
      </div>
    </div>
  );
}

function MeetingHistoryRow({ meeting }) {
  const accent = meetingAccentColor(meeting.title);
  const ini = nameInitials(meeting.advisorName);

  return (
    <div className="mcard mcard--past" style={{ "--accent": accent }}>
      <div className="mcard-accent" />
      <div className="mcard-body">
        <div className="mcard-timecol">
          <span className="mcard-time-start">{formatTime(meeting.startAt)}</span>
          <span className="mcard-time-sep">–</span>
          <span className="mcard-time-end">{formatTime(meeting.endAt)}</span>
        </div>
        <div className="mcard-content">
          <div className="d-flex align-items-start justify-content-between gap-2 flex-wrap">
            <div className="mcard-title">{meeting.title}</div>
            <span className="mcard-completed-chip">
              <i className="pi pi-check-circle" /> Completed
            </span>
          </div>
          <div className="mcard-meta">
            <span className="mcard-person">
              <span className="mcard-avatar" style={{ background: accent, opacity: 0.7 }}>{ini}</span>
              {meeting.advisorName}
            </span>
            <span className="mcard-date-chip">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
          </div>
          {meeting.notes?.length ? (
            <div className="mcard-notes">
              <div className="mcard-notes-label">
                <i className="pi pi-file-edit" /> Advisor Notes
              </div>
              <div className="mcard-notes-text">{meeting.notes}</div>
            </div>
          ) : (
            <div className="mcard-no-notes">No advisor notes recorded.</div>
          )}
        </div>
      </div>
    </div>
  );
}

function RequestRow({ request, onCancel }) {
  const severity =
    request.status === "PENDING" ? "warning"
    : request.status === "REJECTED" ? "danger"
    : request.status === "ACCEPTED" ? "success"
    : "info";

  const accent =
    request.status === "PENDING" ? "#f59e0b"
    : request.status === "REJECTED" ? "#dc2626"
    : request.status === "ACCEPTED" ? "#16a34a"
    : "#6b7280";

  const statusIcon =
    request.status === "PENDING" ? "pi pi-clock"
    : request.status === "REJECTED" ? "pi pi-times-circle"
    : "pi pi-check-circle";

  return (
    <div className="mcard" style={{ "--accent": accent }}>
      <div className="mcard-accent" />
      <div className="mcard-body">
        <div className="mcard-timecol">
          <span className="mcard-time-start">{formatTime(request.startAt)}</span>
          <span className="mcard-time-sep">–</span>
          <span className="mcard-time-end">{formatTime(request.endAt)}</span>
        </div>
        <div className="mcard-content">
          <div className="d-flex align-items-start justify-content-between gap-2 flex-wrap">
            <div className="mcard-title">Meeting Request</div>
            <Tag value={request.status} severity={severity} className="meeting-status-tag" />
          </div>
          <div className="mcard-meta">
            <span className="mcard-person">
              <i className={statusIcon} style={{ color: accent, fontSize: 14 }} />
              {request.advisorName}
            </span>
            <span className="mcard-date-chip">
              <i className="pi pi-calendar" />
              {formatDateShort(request.startAt)}
            </span>
          </div>
          {request.reason && (
            <div className="mcard-notes">
              <div className="mcard-notes-label">
                <i className="pi pi-comment" /> Your Reason
              </div>
              <div className="mcard-notes-text">{request.reason}</div>
            </div>
          )}
          {request.rejectionReason && (
            <div className="mcard-notes mt-2" style={{ borderColor: "#fecaca", background: "#fff1f2" }}>
              <div className="mcard-notes-label" style={{ color: "#b91c1c" }}>
                <i className="pi pi-exclamation-circle" style={{ color: "#b91c1c" }} /> Advisor Response
              </div>
              <div className="mcard-notes-text" style={{ color: "#9f1239" }}>{request.rejectionReason}</div>
            </div>
          )}
          {request.status === "PENDING" && (
            <div className="mt-3">
              <Button
                label="Cancel Request"
                icon="pi pi-times"
                className="p-button-sm p-button-outlined p-button-danger"
                onClick={() => onCancel(request.requestId)}
              />
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

/* ─── Timeline Slot Picker ────────────────────────────────────── */
function TimelineSlotPicker({ slots, onSelect }) {
  const groups = [
    {
      key: "morning",
      label: "Morning",
      icon: "pi pi-sun",
      items: slots.filter((s) => new Date(s.startAt).getHours() < 12),
    },
    {
      key: "afternoon",
      label: "Afternoon",
      icon: "pi pi-cloud-sun",
      items: slots.filter((s) => {
        const h = new Date(s.startAt).getHours();
        return h >= 12 && h < 17;
      }),
    },
    {
      key: "evening",
      label: "Evening",
      icon: "pi pi-moon",
      items: slots.filter((s) => new Date(s.startAt).getHours() >= 17),
    },
  ].filter((g) => g.items.length > 0);

  return (
    <div className="tl-container">
      {groups.map((group, gi) => (
        <div key={group.key}>
          <div className={`tl-period-header${gi > 0 ? " tl-period-header--border" : ""}`}>
            <i className={group.icon} />
            <span>{group.label}</span>
            <span className="tl-period-count">{group.items.length} slot{group.items.length !== 1 ? "s" : ""}</span>
          </div>
          {group.items.map((slot, si) => {
            const maxDur = Math.max(...slot.allowedDurations);
            return (
              <button
                key={`${slot.startAt}-${si}`}
                type="button"
                className="tl-slot-row"
                onClick={() => onSelect(slot)}
              >
                <div className="tl-slot-time">{formatTime(slot.startAt)}</div>
                <div className="tl-slot-bar">
                  <div
                    className="tl-slot-bar-fill"
                    style={{ width: `${(maxDur / 60) * 100}%` }}
                  />
                </div>
                <div className="tl-slot-durations">
                  {slot.allowedDurations.map((d) => (
                    <span key={d} className="tl-dur-pill">
                      {d < 60 ? `${d}m` : "1h"}
                    </span>
                  ))}
                </div>
                <i className="pi pi-chevron-right tl-slot-arrow" />
              </button>
            );
          })}
        </div>
      ))}
    </div>
  );
}

function meetingAccentColor(title) {
  const t = (title ?? "").toLowerCase();
  if (t.includes("progress")) return "#2563eb";
  if (t.includes("course")) return "#7c3aed";
  if (t.includes("probation") || t.includes("risk")) return "#dc2626";
  return "#16a34a";
}

function nameInitials(name) {
  return (name ?? "?").split(" ").map((w) => w[0]).join("").slice(0, 2).toUpperCase();
}

function toDateOnly(date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function formatDateLong(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

function formatDateShort(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function formatTime(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleTimeString(undefined, {
    hour: "2-digit",
    minute: "2-digit",
  });
}