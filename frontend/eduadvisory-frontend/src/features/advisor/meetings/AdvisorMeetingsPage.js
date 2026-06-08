import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { Dropdown } from "primereact/dropdown";
import { Calendar } from "primereact/calendar";
import { InputTextarea } from "primereact/inputtextarea";
import { Message } from "primereact/message";
import { Skeleton } from "primereact/skeleton";

import { advisorMeetingsApi } from "../../../services/advisors/advisorMeetingsApi";
import { PageHero } from "../../../shared/components/PageHero";
import "./AdvisorMeetingsPage.css";
import { sharedGoogleAuthApi } from "../../../shared/sharedGoogleAuthApi";

const dayOptions = [
  { label: "Sunday", value: 0 },
  { label: "Monday", value: 1 },
  { label: "Tuesday", value: 2 },
  { label: "Wednesday", value: 3 },
  { label: "Thursday", value: 4 },
  { label: "Friday", value: 5 },
  { label: "Saturday", value: 6 },
];

export default function AdvisorMeetingsPage() {
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [msg, setMsg] = useState("");

  const [weeklyAvailability, setWeeklyAvailability] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [upcomingMeetings, setUpcomingMeetings] = useState([]);
  const [historyMeetings, setHistoryMeetings] = useState([]);

  const [googleStatus, setGoogleStatus] = useState({
    connected: false,
    googleEmail: null,
  });

  // NEW: tracks whether a reconnect is specifically needed due to an auth error
  const [reconnectRequired, setReconnectRequired] = useState(false);

  const [availabilityDialogOpen, setAvailabilityDialogOpen] = useState(false);
  const [dayOfWeek, setDayOfWeek] = useState(1);
  const [startTime, setStartTime] = useState("");
  const [endTime, setEndTime] = useState("");
  const [savingAvailability, setSavingAvailability] = useState(false);

  const [responseDialogOpen, setResponseDialogOpen] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [decision, setDecision] = useState("ACCEPTED");
  const [rejectionReason, setRejectionReason] = useState("");
  const [responding, setResponding] = useState(false);
  const [respondErr, setRespondErr] = useState("");

  const rulesCount = useMemo(() => weeklyAvailability.length, [weeklyAvailability]);

  const loadData = async () => {
    setLoading(true);
    setErr("");

    try {
      const [rulesRes, pendingRes, upcomingRes, historyRes, googleRes] = await Promise.all([
        advisorMeetingsApi.getWeeklyAvailability(),
        advisorMeetingsApi.getPendingRequests(),
        advisorMeetingsApi.getUpcoming(),
        advisorMeetingsApi.getHistory(),
        sharedGoogleAuthApi.getStatus(),
      ]);

      setWeeklyAvailability(rulesRes.data || []);
      setPendingRequests(pendingRes.data || []);
      setUpcomingMeetings((upcomingRes.data || []).filter(m => new Date(m.endAt ?? m.startAt) > new Date()));      setHistoryMeetings(historyRes.data || []);
      setGoogleStatus(googleRes.data || { connected: false, googleEmail: null });
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data?.message ?? e?.response?.data ?? e?.message ?? "Failed to load advisor meetings.");
    } finally {
      setLoading(false);
    }
  };

  const loadGoogleStatus = async () => {
    try {
      const res = await sharedGoogleAuthApi.getStatus();
      setGoogleStatus(res.data || { connected: false, googleEmail: null });
    } catch (e) {
      console.error(e);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    if (params.get("google_connected") === "1") {
      setMsg("Google account connected successfully.");
      setReconnectRequired(false); // clear reconnect flag on successful connect
      loadGoogleStatus();

      params.delete("google_connected");
      const newUrl =
        window.location.pathname + (params.toString() ? `?${params.toString()}` : "");
      window.history.replaceState({}, "", newUrl);
    }
  }, []);

  const handleConnectGoogle = async () => {
    try {
      const res = await sharedGoogleAuthApi.getConnectUrl();
      const url = res?.data?.url;
      if (!url) {
        setErr("Failed to start Google connection.");
        return;
      }
      window.location.href = url;
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data?.message ?? e?.response?.data ?? e?.message ?? "Failed to start Google connection.");
    }
  };

  const handleDisconnectGoogle = async () => {
    try {
      await sharedGoogleAuthApi.disconnect();
      setMsg("Google account disconnected.");
      setReconnectRequired(false); // clear reconnect flag on disconnect
      await loadGoogleStatus();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data?.message ?? e?.response?.data ?? e?.message ?? "Failed to disconnect Google account.");
    }
  };

  const handleCreateAvailability = async () => {
    if (!startTime || !endTime) {
      setErr("Please select both start and end time.");
      return;
    }

    try {
      setSavingAvailability(true);
      setErr("");

      await advisorMeetingsApi.createWeeklyAvailability({
        dayOfWeek,
        startTime: startTime + ":00",
        endTime: endTime + ":00",
      });

      setMsg("Weekly availability added.");
      setAvailabilityDialogOpen(false);
      setDayOfWeek(1);
      setStartTime("");
      setEndTime("");
      await loadData();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data?.message ?? e?.response?.data ?? e?.message ?? "Failed to create weekly availability.");
    } finally {
      setSavingAvailability(false);
    }
  };

  const handleDeleteAvailability = async (ruleId) => {
    try {
      setErr("");
      await advisorMeetingsApi.deleteWeeklyAvailability(ruleId);
      setMsg("Weekly availability removed.");
      await loadData();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data?.message ?? e?.response?.data ?? e?.message ?? "Failed to delete weekly availability.");
    }
  };

  const openRespondDialog = (request) => {
    setSelectedRequest(request);
    setDecision("ACCEPTED");
    setRejectionReason("");
    setRespondErr("");
    setResponseDialogOpen(true);
  };

  const handleRespond = async () => {
    if (!selectedRequest) return;

    if (decision === "REJECTED" && !rejectionReason.trim()) {
      setRespondErr("Please provide a rejection reason.");
      return;
    }

    try {
      setResponding(true);
      setRespondErr("");

      await advisorMeetingsApi.respondToRequest(selectedRequest.requestId, {
        decision,
        rejectionReason: decision === "REJECTED" ? rejectionReason.trim() : null,
      });

      setMsg(
        decision === "ACCEPTED"
          ? "Request accepted and meeting link generated."
          : "Request rejected successfully."
      );

      setResponseDialogOpen(false);
      setReconnectRequired(false);
      await loadData();
    } catch (e) {
      console.error(e);
      const responseData = e?.response?.data;

      if (responseData?.reconnectRequired) {
        setReconnectRequired(true);
        setRespondErr(
          responseData.message ||
            "Google connection expired. Please close this dialog and reconnect your Google account."
        );
        await loadGoogleStatus();
      } else {
        const message =
          responseData?.message ??
          (typeof responseData === "string" ? responseData : null) ??
          e?.message ??
          "Failed to respond to request.";
        setRespondErr(message);
      }
    } finally {
      setResponding(false);
    }
  };

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="20rem" height="2rem" className="mb-3" />
        <Skeleton height="12rem" className="mb-4" />
        <Skeleton height="14rem" className="mb-4" />
        <Skeleton height="14rem" className="mb-4" />
      </div>
    );
  }

  return (
    <div className="container-fluid p-3 p-md-4">
      <PageHero
        title="Advisor Meetings"
        badge="Advisor Portal"
        subtitle="Manage your weekly availability, requests, and advising sessions"
      />

      {msg ? <Message severity="success" text={msg} className="mb-4" /> : null}
      {err ? <Message severity="error" text={String(err)} className="mb-4" /> : null}

     {(!googleStatus.connected || reconnectRequired) && (
  <Card className="meetings-card shadow-sm border-0 mb-4">
    <div className="meetings-card-header">
      <div>
        <div className="fw-semibold fs-4">Google Meet Integration</div>
        <div className="text-muted mt-1">
          Connect your Google account so meeting links can be generated when you approve requests.
        </div>
      </div>

      <Tag
        value={googleStatus.connected ? "Connected" : "Not Connected"}
        severity={googleStatus.connected ? "success" : "danger"}
        className="meeting-status-tag"
      />
    </div>

    <div className="d-flex align-items-center justify-content-between flex-wrap gap-3">
      <div className="meeting-advisor-summary">
        {googleStatus.connected
          ? "Your Google connection has expired and needs to be re-authorized."
          : "Google account is not connected or needs to be re-authorized."}
      </div>

      <div className="d-flex gap-2 flex-wrap">
        <Button
          label={reconnectRequired ? "Reconnect Google" : "Connect Google"}
          icon="pi pi-google"
          className={reconnectRequired ? "p-button-warning" : undefined}
          onClick={handleConnectGoogle}
        />
      </div>
    </div>
  </Card>
)}

      <div className="row g-4 mb-4">
        <CountCard title="Weekly Rules" value={rulesCount} icon="pi pi-calendar-plus" />
        <CountCard title="Pending Requests" value={pendingRequests.length} icon="pi pi-inbox" />
        <CountCard title="Upcoming Meetings" value={upcomingMeetings.length} icon="pi pi-users" />
      </div>

      <BlockedDatesSection weeklyRules={weeklyAvailability} />

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Weekly Availability</div>
            <div className="text-muted mt-1">
              Example: Monday 10:00 - 16:00, Tuesday 09:00 - 18:00
            </div>
          </div>

          <Button
            label="Add Availability"
            icon="pi pi-plus"
            onClick={() => setAvailabilityDialogOpen(true)}
          />
        </div>

        {weeklyAvailability.length ? (
          <div className="row g-3">
            {weeklyAvailability.map((rule) => (
              <div className="col-12 col-lg-6" key={rule.ruleId}>
                <AvailabilityRuleCard rule={rule} onDelete={handleDeleteAvailability} />
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-calendar"
            title="No weekly availability"
            text="Add weekly hours so students can request meetings."
          />
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Pending Requests</div>
            <div className="text-muted mt-1">Review and accept or reject student requests</div>
          </div>

          <Tag
            value={`${pendingRequests.length} Waiting`}
            severity={pendingRequests.length ? "warning" : "info"}
            className="meeting-status-tag"
          />
        </div>

        {pendingRequests.length ? (
          <div className="d-flex flex-column gap-3">
            {pendingRequests.map((request) => (
              <AdvisorRequestCard
                key={request.requestId}
                request={request}
                onRespond={() => openRespondDialog(request)}
              />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-inbox"
            title="No pending requests"
            text="New student meeting requests will appear here."
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

        {upcomingMeetings.length ? (
          <div className="d-flex flex-column gap-3">
            {upcomingMeetings.map((meeting) => (
              <MeetingCard key={meeting.meetingId} meeting={meeting} />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-users"
            title="No upcoming meetings"
            text="Approved meetings will appear here."
          />
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Meeting History</div>
            <div className="text-muted mt-1">Past meetings and completed sessions</div>
          </div>
        </div>

        {historyMeetings.length ? (
          <div className="d-flex flex-column gap-3">
            {historyMeetings.map((meeting) => (
              <MeetingCard key={meeting.meetingId} meeting={meeting} isPast />
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-history"
            title="No meeting history"
            text="Past meetings will appear here."
          />
        )}
      </Card>

      <Dialog
        header="Add Weekly Availability"
        visible={availabilityDialogOpen}
        style={{ width: "36rem", maxWidth: "95vw" }}
        onHide={() => setAvailabilityDialogOpen(false)}
      >
        <div className="meeting-dialog-banner mb-3">
          <div className="meeting-dialog-banner-icon">
            <i className="pi pi-calendar-plus" />
          </div>
          <div>
            <div className="meeting-dialog-banner-title">Add Weekly Hours</div>
            <div className="meeting-dialog-banner-text">
              Students will be able to request times inside this availability window.
            </div>
          </div>
        </div>

        <div className="mb-3">
          <label className="meeting-advisor-label d-block mb-2">Day</label>
          <Dropdown
            value={dayOfWeek}
            onChange={(e) => setDayOfWeek(e.value)}
            options={dayOptions}
            className="w-100"
          />
        </div>

        <div className="exc-time-row mb-3">
          <div className="exc-time-field">
            <label className="exc-time-label">From</label>
            <input
              type="time"
              className="exc-time-input"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
            />
          </div>
          <div className="exc-time-sep">→</div>
          <div className="exc-time-field">
            <label className="exc-time-label">To</label>
            <input
              type="time"
              className="exc-time-input"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
            />
          </div>
        </div>

        <div className="meeting-reminder-box">
          Students can request meetings in 15-minute increments, with durations of 15, 30, 45, or 60 minutes.
        </div>

        <div className="d-flex justify-content-end gap-2 mt-4">
          <Button
            label="Cancel"
            className="p-button-text"
            onClick={() => setAvailabilityDialogOpen(false)}
          />
          <Button
            label={savingAvailability ? "Saving..." : "Save"}
            icon="pi pi-check"
            onClick={handleCreateAvailability}
            disabled={savingAvailability || !startTime || !endTime || startTime >= endTime}
          />
        </div>
      </Dialog>

      <Dialog
        header="Respond to Meeting Request"
        visible={responseDialogOpen}
        style={{ width: "42rem", maxWidth: "95vw" }}
        onHide={() => setResponseDialogOpen(false)}
      >
        {selectedRequest ? (
          <div>
            <div className="meeting-dialog-banner mb-3">
              <div className="meeting-dialog-banner-icon">
                <i className="pi pi-user-edit" />
              </div>
              <div>
                <div className="meeting-dialog-banner-title">{selectedRequest.studentName}</div>
                <div className="meeting-dialog-banner-text">
                  {formatDateLong(selectedRequest.startAt)} • {formatTime(selectedRequest.startAt)} -{" "}
                  {formatTime(selectedRequest.endAt)}
                </div>
              </div>
            </div>

            <div className="meeting-request-reason-box mb-3">
              <div className="meeting-request-reason-header">
                <i className="pi pi-comment" />
                <span>Student Reason</span>
              </div>
              <div className="meeting-request-reason-text">
                {selectedRequest.reason || "No reason provided."}
              </div>
            </div>

            <div className="decision-selector mb-3">
              <button
                type="button"
                className={`decision-option ${decision === "ACCEPTED" ? "active-accept" : ""}`}
                onClick={() => setDecision("ACCEPTED")}
              >
                <i className="pi pi-check-circle" />
                <div>
                  <div className="decision-option-title">Approve</div>
                  <div className="decision-option-text">
                    Accept this request and generate a meeting link
                  </div>
                </div>
              </button>

              <button
                type="button"
                className={`decision-option ${decision === "REJECTED" ? "active-reject" : ""}`}
                onClick={() => setDecision("REJECTED")}
              >
                <i className="pi pi-times-circle" />
                <div>
                  <div className="decision-option-title">Reject</div>
                  <div className="decision-option-text">
                    Decline the request and send a reason to the student
                  </div>
                </div>
              </button>
            </div>

            {decision === "ACCEPTED" ? (
              <div className="meeting-reminder-box mb-3">
                A meeting link will be generated automatically when you approve this request.
              </div>
            ) : (
              <div className="mb-3">
                <label className="meeting-advisor-label d-block mb-2">Rejection Reason</label>
                <InputTextarea
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  rows={4}
                  className="w-100"
                  placeholder="Explain briefly why this request is being rejected."
                />
                <div className="meeting-text-counter mt-2">
                  {rejectionReason.trim().length} characters
                </div>
              </div>
            )}

            {respondErr && (
              <Message
                severity={respondErr.toLowerCase().includes("google") ? "warn" : "error"}
                text={respondErr}
                className="w-100 mt-3"
              />
            )}

            <div className="d-flex justify-content-end gap-2 mt-4">
              <Button
                label="Cancel"
                className="p-button-text"
                onClick={() => setResponseDialogOpen(false)}
              />
              <Button
                label={
                  responding
                    ? "Submitting..."
                    : decision === "ACCEPTED"
                    ? "Approve Request"
                    : "Reject Request"
                }
                icon="pi pi-check"
                severity={decision === "REJECTED" ? "danger" : undefined}
                onClick={handleRespond}
                disabled={responding || (decision === "REJECTED" && !rejectionReason.trim())}
              />
            </div>
          </div>
        ) : null}
      </Dialog>
    </div>
  );
}

/* ─── Blocked Dates Section ───────────────────────────────────── */
const EXC_DOW_LABELS = ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa"];
const EXC_MONTH_NAMES = [
  "January","February","March","April","May","June",
  "July","August","September","October","November","December",
];

function toDateKey(y, m, d) {
  return `${y}-${String(m + 1).padStart(2, "0")}-${String(d).padStart(2, "0")}`;
}

function fmtExcTime(t) {
  if (!t) return "";
  return t.slice(0, 5);
}

function fmtExcDate(dateStr) {
  return new Date(dateStr + "T00:00:00").toLocaleDateString(undefined, {
    weekday: "short", month: "short", day: "numeric",
  });
}

function BlockedDatesSection({ weeklyRules }) {
  const today = new Date();
  const todayKey = toDateKey(today.getFullYear(), today.getMonth(), today.getDate());

  const [viewYear, setViewYear] = useState(today.getFullYear());
  const [viewMonth, setViewMonth] = useState(today.getMonth());
  const [exceptions, setExceptions] = useState([]); // [{date, startTime, endTime, exceptionId}]
  const [loadingEx, setLoadingEx] = useState(true);
  const [exErr, setExErr] = useState("");

  // Dialog state
  const [blockDialog, setBlockDialog] = useState(false);
  const [blockDate, setBlockDate] = useState("");
  const [fullDay, setFullDay] = useState(true);
  const [blockStart, setBlockStart] = useState(""); // "HH:mm"
  const [blockEnd, setBlockEnd] = useState("");
  const [saving, setSaving] = useState(false);
  const [saveErr, setSaveErr] = useState("");

  const ruleDays = new Set(weeklyRules.map((r) => r.dayOfWeek));

  useEffect(() => {
    advisorMeetingsApi.getExceptions()
      .then((res) => setExceptions(res.data ?? []))
      .catch(() => setExErr("Failed to load blocked periods."))
      .finally(() => setLoadingEx(false));
  }, []);

  const openBlockDialog = (dateKey) => {
    setBlockDate(dateKey);
    setFullDay(true);
    setBlockStart("");
    setBlockEnd("");
    setSaveErr("");
    setBlockDialog(true);
  };

  const handleSaveBlock = async () => {
    setSaveErr("");
    setSaving(true);
    try {
      const payload = { date: blockDate };
      if (!fullDay) {
        if (!blockStart || !blockEnd) {
          setSaveErr("Please select both start and end time.");
          setSaving(false);
          return;
        }
        payload.startTime = blockStart + ":00";
        payload.endTime = blockEnd + ":00";
      }
      const res = await advisorMeetingsApi.addException(payload);
      // Reload exceptions list
      const updated = await advisorMeetingsApi.getExceptions();
      setExceptions(updated.data ?? []);
      setBlockDialog(false);
    } catch (e) {
      setSaveErr(e?.response?.data?.message ?? "Failed to block this period.");
    } finally {
      setSaving(false);
    }
  };

  const handleRemove = async (exceptionId) => {
    setExErr("");
    try {
      await advisorMeetingsApi.removeException(exceptionId);
      setExceptions((prev) => prev.filter((x) => x.exceptionId !== exceptionId));
    } catch (e) {
      setExErr(e?.response?.data?.message ?? "Failed to remove blocked period.");
    }
  };

  const firstDow = new Date(viewYear, viewMonth, 1).getDay();
  const daysInMonth = new Date(viewYear, viewMonth + 1, 0).getDate();
  const cells = [];
  for (let i = 0; i < firstDow; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  while (cells.length % 7 !== 0) cells.push(null);

  const prevMonth = () => {
    if (viewMonth === 0) { setViewYear((y) => y - 1); setViewMonth(11); }
    else setViewMonth((m) => m - 1);
  };
  const nextMonth = () => {
    if (viewMonth === 11) { setViewYear((y) => y + 1); setViewMonth(0); }
    else setViewMonth((m) => m + 1);
  };

  const upcomingExceptions = exceptions.slice(0, 10);

  return (
    <>
      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Blocked Periods</div>
            <div className="text-muted mt-1">
              Block a full day or specific hours — overrides your weekly rules
            </div>
          </div>
          {exceptions.length > 0 && (
            <Tag
              value={`${exceptions.length} blocked`}
              severity="danger"
              className="meeting-status-tag"
            />
          )}
        </div>

        {exErr && <Message severity="error" text={exErr} className="mb-3" />}

        <div className="exc-layout">
          <div className="exc-calendar">
            <div className="exc-nav">
              <button type="button" className="exc-nav-btn" onClick={prevMonth}>
                <i className="pi pi-chevron-left" />
              </button>
              <span className="exc-month-label">
                {EXC_MONTH_NAMES[viewMonth]} {viewYear}
              </span>
              <button type="button" className="exc-nav-btn" onClick={nextMonth}>
                <i className="pi pi-chevron-right" />
              </button>
            </div>

            <div className="exc-grid">
              {EXC_DOW_LABELS.map((d) => (
                <div key={d} className="exc-dow">{d}</div>
              ))}
              {cells.map((day, idx) => {
                if (!day) return <div key={`pad-${idx}`} className="exc-day exc-day--empty" />;

                const dateKey = toDateKey(viewYear, viewMonth, day);
                const dow = new Date(viewYear, viewMonth, day).getDay();
                const isPast = dateKey < todayKey;
                const isToday = dateKey === todayKey;
                const hasRule = ruleDays.has(dow);
                const dayExceptions = exceptions.filter((x) => x.date === dateKey);
                const isFullBlocked = dayExceptions.some((x) => !x.startTime);
                const isPartialBlocked = dayExceptions.length > 0 && !isFullBlocked;

                return (
                  <button
                    key={dateKey}
                    type="button"
                    className={[
                      "exc-day",
                      isPast ? "exc-day--past" : "",
                      isToday ? "exc-day--today" : "",
                      hasRule && !isPast ? "exc-day--has-rule" : "",
                      isFullBlocked ? "exc-day--blocked" : "",
                      isPartialBlocked ? "exc-day--partial" : "",
                    ].filter(Boolean).join(" ")}
                    onClick={() => !isPast && openBlockDialog(dateKey)}
                    disabled={isPast || loadingEx}
                    title={
                      isPast ? "" :
                      isFullBlocked ? "Full day blocked — click to add more" :
                      isPartialBlocked ? "Partially blocked — click to add more" :
                      hasRule ? "Click to block hours on this date" :
                      "Click to block this date"
                    }
                  >
                    {day}
                    {hasRule && !isFullBlocked && !isPast && !isPartialBlocked && (
                      <span className="exc-dot exc-dot--rule" />
                    )}
                    {isFullBlocked && <span className="exc-dot exc-dot--blocked" />}
                    {isPartialBlocked && <span className="exc-dot exc-dot--partial" />}
                  </button>
                );
              })}
            </div>

            <div className="exc-legend">
              <span className="exc-legend-item">
                <span className="exc-legend-dot exc-legend-dot--rule" /> Weekly rule
              </span>
              <span className="exc-legend-item">
                <span className="exc-legend-dot exc-legend-dot--partial" /> Partial block
              </span>
              <span className="exc-legend-item">
                <span className="exc-legend-dot exc-legend-dot--blocked" /> Full day
              </span>
            </div>
          </div>

          <div className="exc-sidebar">
            {upcomingExceptions.length > 0 ? (
              <>
                <div className="exc-sidebar-label">Upcoming blocked periods</div>
                <div className="exc-blocked-chips">
                  {upcomingExceptions.map((ex) => (
                    <span key={ex.exceptionId} className={`exc-blocked-chip${ex.startTime ? " exc-blocked-chip--partial" : ""}`}>
                      <span className="exc-chip-info">
                        <span className="exc-chip-date">{fmtExcDate(ex.date)}</span>
                        {ex.startTime && (
                          <span className="exc-chip-time">
                            {fmtExcTime(ex.startTime)} – {fmtExcTime(ex.endTime)}
                          </span>
                        )}
                        {!ex.startTime && (
                          <span className="exc-chip-time">Full day</span>
                        )}
                      </span>
                      <button
                        type="button"
                        className="exc-chip-remove"
                        onClick={() => handleRemove(ex.exceptionId)}
                        title="Remove this block"
                      >
                        <i className="pi pi-times" />
                      </button>
                    </span>
                  ))}
                  {exceptions.length > 10 && (
                    <span className="exc-blocked-chip exc-blocked-chip--more">
                      +{exceptions.length - 10} more
                    </span>
                  )}
                </div>
              </>
            ) : (
              <div className="exc-sidebar-empty">
                <i className="pi pi-check-circle exc-sidebar-empty-icon" />
                <div className="exc-sidebar-empty-text">No upcoming blocks</div>
                <div className="exc-sidebar-empty-hint">
                  Click any date on the calendar to block it
                </div>
              </div>
            )}
          </div>
        </div>
      </Card>

      {/* Block dialog */}
      <Dialog
        header={`Block Period — ${blockDate ? fmtExcDate(blockDate) : ""}`}
        visible={blockDialog}
        style={{ width: "34rem", maxWidth: "95vw" }}
        onHide={() => setBlockDialog(false)}
      >
        <div className="exc-dialog-type mb-3">
          <button
            type="button"
            className={`exc-type-btn${fullDay ? " exc-type-btn--active" : ""}`}
            onClick={() => setFullDay(true)}
          >
            <i className="pi pi-ban" />
            <div>
              <div className="exc-type-title">Full Day</div>
              <div className="exc-type-sub">No slots available for the entire day</div>
            </div>
          </button>
          <button
            type="button"
            className={`exc-type-btn${!fullDay ? " exc-type-btn--active" : ""}`}
            onClick={() => setFullDay(false)}
          >
            <i className="pi pi-clock" />
            <div>
              <div className="exc-type-title">Specific Hours</div>
              <div className="exc-type-sub">Block only a time range on this day</div>
            </div>
          </button>
        </div>

        {!fullDay && (
          <div className="exc-time-row">
            <div className="exc-time-field">
              <label className="exc-time-label">From</label>
              <input
                type="time"
                className="exc-time-input"
                value={blockStart}
                onChange={(e) => setBlockStart(e.target.value)}
              />
            </div>
            <div className="exc-time-sep">→</div>
            <div className="exc-time-field">
              <label className="exc-time-label">To</label>
              <input
                type="time"
                className="exc-time-input"
                value={blockEnd}
                onChange={(e) => setBlockEnd(e.target.value)}
              />
            </div>
          </div>
        )}

        {saveErr && <Message severity="error" text={saveErr} className="w-100 mb-3" />}

        <div className="d-flex justify-content-end gap-2 mt-3">
          <Button label="Cancel" className="p-button-text" onClick={() => setBlockDialog(false)} />
          <Button
            label={saving ? "Saving…" : fullDay ? "Block Full Day" : "Block Hours"}
            icon="pi pi-ban"
            severity="danger"
            onClick={handleSaveBlock}
            disabled={saving}
          />
        </div>
      </Dialog>
    </>
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

const DOW_ABBR = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

function AvailabilityRuleCard({ rule, onDelete }) {
  return (
    <div className="aval-card">
      <div className="aval-card-top">
        <div className="aval-day-badge">
          <span className="aval-day-abbr">{DOW_ABBR[rule.dayOfWeek]}</span>
        </div>
        <div className="aval-info">
          <div className="aval-day-name">{dayName(rule.dayOfWeek)}</div>
          <div className="aval-time">
            <i className="pi pi-clock" />
            {formatRuleTime(rule.startTime)} – {formatRuleTime(rule.endTime)}
          </div>
        </div>
        <Tag value="ACTIVE" severity="success" className="meeting-status-tag ms-auto" />
      </div>
      <div className="aval-footer">
        <span className="aval-note">15 – 60 min slots available in this window</span>
        <Button
          label="Remove"
          icon="pi pi-trash"
          className="p-button-sm p-button-outlined p-button-danger"
          onClick={() => onDelete(rule.ruleId)}
        />
      </div>
    </div>
  );
}

function AdvisorRequestCard({ request, onRespond }) {
  return (
    <div className="advisor-request-card">
      <div className="advisor-request-card-top">
        <div>
          <div className="advisor-request-name">{request.studentName}</div>
          <div className="meeting-history-meta mt-2">
            <span className="meeting-history-meta-item">
              <i className="pi pi-calendar" />
              {formatDateLong(request.startAt)}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-clock" />
              {formatTime(request.startAt)} - {formatTime(request.endAt)}
            </span>
          </div>
        </div>

        <Tag value={request.status} severity="warning" className="meeting-status-tag" />
      </div>

      <div className="advisor-request-reason">
        <div className="meeting-notes-label mb-2">
          <i className="pi pi-comment" />
          <span>Reason for meeting</span>
        </div>
        <div className="meeting-notes-box">{request.reason || "No reason provided."}</div>
      </div>

      <div className="advisor-request-actions">
        <div className="advisor-request-hint">
          Review the request details, then approve or reject it.
        </div>

        <Button
          label="Review Request"
          icon="pi pi-arrow-right"
          onClick={onRespond}
        />
      </div>
    </div>
  );
}

function MeetingCard({ meeting, isPast = false }) {
  const hasLink = meeting.meetingLink?.trim();
  const accent = meetingAccentColor(meeting.title);
  const ini = nameInitials(meeting.studentName);

  return (
    <div className={`mcard${isPast ? " mcard--past" : ""}`} style={{ "--accent": accent }}>
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
            {isPast ? (
              <span className="mcard-completed-chip">
                <i className="pi pi-check-circle" /> Completed
              </span>
            ) : (
              <Tag value="UPCOMING" severity="info" className="meeting-status-tag" />
            )}
          </div>
          <div className="mcard-meta">
            <span className="mcard-person">
              <span className="mcard-avatar" style={{ background: accent, opacity: isPast ? 0.7 : 1 }}>{ini}</span>
              {meeting.studentName}
            </span>
            <span className="mcard-date-chip">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
          </div>
          {!isPast && hasLink && (
            <a href={meeting.meetingLink} target="_blank" rel="noreferrer" className="mcard-join-btn">
              <i className="pi pi-video" /> Join Meeting
            </a>
          )}
          {!isPast && !hasLink && (
            <span className="mcard-link-pending">
              <i className="pi pi-clock" /> Meeting link will be available soon
            </span>
          )}
        </div>
      </div>
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

function dayName(day) {
  return dayOptions.find((x) => x.value === day)?.label ?? "Unknown";
}

function toTimeString(date) {
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");
  return `${hours}:${minutes}:00`;
}

function formatRuleTime(value) {
  return String(value).slice(0, 5);
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