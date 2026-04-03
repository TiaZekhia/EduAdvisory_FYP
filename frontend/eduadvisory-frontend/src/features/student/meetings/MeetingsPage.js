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
              <Skeleton height="10rem" />
            ) : calendarStartTimes.length ? (
              <div className="row g-3">
                {calendarStartTimes.map((item, index) => (
                  <div className="col-12 col-md-6 col-xl-4" key={`${item.startAt}-${index}`}>
                    <button
                      type="button"
                      className="calendar-interval-card"
                      onClick={() => handleOpenRequest(item)}
                    >
                      <div className="calendar-interval-time">{formatTime(item.startAt)}</div>
                      <div className="calendar-interval-status">
                        Allowed: {item.allowedDurations.map((x) => `${x}m`).join(", ")}
                      </div>
                    </button>
                  </div>
                ))}
              </div>
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
  const hasLink = meeting.meetingLink && meeting.meetingLink.trim();

  return (
    <div className="upcoming-meeting-row">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="upcoming-meeting-title">{meeting.title}</div>

          <div className="meeting-history-meta mt-2">
            <span className="meeting-history-meta-item">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-clock" />
              {formatTime(meeting.startAt)} - {formatTime(meeting.endAt)}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-user" />
              {meeting.advisorName}
            </span>
          </div>
        </div>

        <div className="d-flex flex-column align-items-end gap-2">
          <Tag value={meeting.status} severity="info" className="meeting-status-tag" />
          {hasLink ? (
            <a
              href={meeting.meetingLink}
              target="_blank"
              rel="noreferrer"
              className="meeting-link-btn"
            >
              <i className="pi pi-video me-2" />
              Join
            </a>
          ) : (
            <span className="meeting-link-pending">Link pending</span>
          )}
        </div>
      </div>
    </div>
  );
}

function MeetingHistoryRow({ meeting }) {
  return (
    <div className="meeting-history-row">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="meeting-history-title">{meeting.title}</div>

          <div className="meeting-history-meta mt-2">
            <span className="meeting-history-meta-item">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-clock" />
              {formatTime(meeting.startAt)} - {formatTime(meeting.endAt)}
            </span>
          </div>

          <div className="mt-3">
            <div className="meeting-notes-label">
              <i className="pi pi-file" />
              <span>Advisor Notes</span>
            </div>
            <div className="meeting-notes-box mt-2">
              {meeting.notes?.length ? meeting.notes : "No notes."}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function RequestRow({ request, onCancel }) {
  const severity =
    request.status === "PENDING"
      ? "warning"
      : request.status === "REJECTED"
      ? "danger"
      : request.status === "ACCEPTED"
      ? "success"
      : "info";

  return (
    <div className={`meeting-request-row request-status-${String(request.status).toLowerCase()}`}>
      <div className="meeting-request-left">
        <div className="meeting-request-row-title">Meeting Request</div>

        <div className="meeting-history-meta mt-2">
          <span className="meeting-history-meta-item">
            <i className="pi pi-user" />
            {request.advisorName}
          </span>
          <span className="meeting-history-meta-item">
            <i className="pi pi-calendar" />
            {formatDateShort(request.startAt)}
          </span>
          <span className="meeting-history-meta-item">
            <i className="pi pi-clock" />
            {formatTime(request.startAt)} - {formatTime(request.endAt)}
          </span>
        </div>

        {request.reason ? (
          <div className="meeting-request-reason-box mt-3">
            <div className="meeting-request-reason-header">
              <i className="pi pi-comment" />
              <span>Your Reason</span>
            </div>
            <div className="meeting-request-reason-text">{request.reason}</div>
          </div>
        ) : null}

        {request.rejectionReason ? (
          <div className="meeting-request-reject-box mt-3">
            <div className="meeting-request-reason-header">
              <i className="pi pi-exclamation-circle" />
              <span>Advisor Response</span>
            </div>
            <div className="meeting-request-reason-text">{request.rejectionReason}</div>
          </div>
        ) : null}
      </div>

      <div className="meeting-request-right">
        <Tag value={request.status} severity={severity} className="meeting-status-tag" />

        {request.status === "PENDING" ? (
          <div className="meeting-request-actions">
            <Button
              label="Cancel"
              icon="pi pi-times"
              className="p-button-sm p-button-outlined p-button-danger"
              onClick={() => onCancel(request.requestId)}
            />
          </div>
        ) : null}
      </div>
    </div>
  );
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