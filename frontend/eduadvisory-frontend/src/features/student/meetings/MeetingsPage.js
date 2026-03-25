import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { Divider } from "primereact/divider";
import { Dialog } from "primereact/dialog";
import { Button } from "primereact/button";
import { InputTextarea } from "primereact/inputtextarea";
import { Message } from "primereact/message";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMeetingsApi } from "../../../services/students/studentMeetingsApi";
import { PageHero } from "../../../shared/components/PageHero";
import "./MeetingsPage.css";

export default function MeetingsPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [advisor, setAdvisor] = useState(null);
  const [upcoming, setUpcoming] = useState([]);
  const [history, setHistory] = useState([]);
  const [requests, setRequests] = useState([]);
  const [availability, setAvailability] = useState([]);

  const [requestDialogOpen, setRequestDialogOpen] = useState(false);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [reason, setReason] = useState("");
  const [savingRequest, setSavingRequest] = useState(false);
  const [actionMsg, setActionMsg] = useState("");
  const [actionError, setActionError] = useState("");

  const pendingRequestsCount = useMemo(
    () => requests.filter((r) => r.status === "PENDING").length,
    [requests]
  );

  const nextAvailableSlot = useMemo(() => {
    if (!availability?.length) return null;
    return availability[0];
  }, [availability]);

  const loadData = async () => {
    setLoading(true);
    setErr("");
    try {
      const [
        advisorRes,
        upcomingRes,
        historyRes,
        requestsRes,
        availabilityRes,
      ] = await Promise.all([
        studentMeetingsApi.getAdvisor(),
        studentMeetingsApi.getUpcoming(),
        studentMeetingsApi.getHistory(),
        studentMeetingsApi.getRequests(),
        studentMeetingsApi.getAdvisorAvailability(),
      ]);

      setAdvisor(advisorRes.data || null);
      setUpcoming(upcomingRes.data || []);
      setHistory(historyRes.data || []);
      setRequests(requestsRes.data || []);
      setAvailability(availabilityRes.data || []);
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to load meetings page.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenRequest = (slot) => {
    setSelectedSlot(slot);
    setReason("");
    setActionError("");
    setRequestDialogOpen(true);
  };

  const handleSubmitRequest = async () => {
    if (!selectedSlot) return;

    if (!reason.trim()) {
      setActionError("Please write a short reason for the meeting.");
      return;
    }

    setSavingRequest(true);
    setActionError("");

    try {
      await studentMeetingsApi.createRequest({
        availabilityId: selectedSlot.availabilityId,
        reason: reason.trim(),
      });

      setActionMsg("Meeting request submitted successfully.");
      setRequestDialogOpen(false);
      await loadData();
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
      await loadData();
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

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="meetings-page container-fluid p-3 p-md-4">
      <PageHero
        title="Meetings"
        badge={`${summary?.programCode ?? "-"} • Semester ${summary?.currentSemester ?? "-"}`}
        subtitle="Schedule and manage your advising meetings"
      />

      {actionMsg ? <Message severity="success" text={actionMsg} className="mb-4" /> : null}

      <div className="row g-4 mb-4">
        <CountCard title="Upcoming Meetings" value={upcoming.length} icon="pi pi-calendar-plus" />
        <CountCard title="Past Meetings" value={history.length} icon="pi pi-history" />
        <CountCard title="Pending Requests" value={pendingRequestsCount} icon="pi pi-send" />
      </div>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Upcoming Meetings</div>
            <div className="text-muted mt-1">
              Your confirmed upcoming sessions with your advisor
            </div>
          </div>
        </div>

        {upcoming?.length ? (
          <div className="d-flex flex-column gap-4">
            <div>
              <div className="meeting-section-label mb-3">Next Meeting</div>
              <MeetingBigCard meeting={upcoming[0]} />
            </div>

            {upcoming.length > 1 ? (
              <div>
                <div className="meeting-section-label mb-3">Other Upcoming Meetings</div>
                <div className="d-flex flex-column gap-3">
                  {upcoming.slice(1).map((meeting) => (
                    <UpcomingMeetingRow key={meeting.meetingId} meeting={meeting} />
                  ))}
                </div>
              </div>
            ) : null}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-calendar"
            title="No upcoming meetings"
            text="You do not have any confirmed meetings right now."
          />
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Pending Requests</div>
            <div className="text-muted mt-1">Requests waiting for advisor approval</div>
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
            <div className="fw-semibold fs-4">Request a Meeting</div>
            <div className="text-muted mt-1">Choose one of your advisor’s available slots</div>
          </div>

          {nextAvailableSlot ? (
            <div className="meeting-highlight-chip">
              <i className="pi pi-bolt" />
              Next available: {formatDateShort(nextAvailableSlot.startAt)}
            </div>
          ) : null}
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
                  value={availability.length ? "Slots Available" : "No Slots"}
                  severity={availability.length ? "success" : "warning"}
                  className="meeting-status-tag"
                />
              </div>

              <div className="meeting-advisor-summary mb-3">
                Book an advising session to discuss course planning, academic progress,
                registration, or any concerns related to your semester.
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
            </div>
          </div>

          <div className="meeting-request-main">
            {availability?.length ? (
              <>
                <div className="meeting-subsection-header mb-3">
                  <div>
                    <div className="meeting-subsection-title">Available Time Slots</div>
                    <div className="meeting-subsection-text">
                      Select the slot that works best for you and send a request.
                    </div>
                  </div>
                </div>

                <div className="row g-3">
                  {availability.map((slot) => (
                    <div className="col-12 col-xl-6" key={slot.availabilityId}>
                      <RequestSlotCard slot={slot} onRequest={() => handleOpenRequest(slot)} />
                    </div>
                  ))}
                </div>
              </>
            ) : (
              <EmptyState
                icon="pi pi-clock"
                title="No available slots"
                text="Your advisor has not published any available slots yet."
              />
            )}
          </div>
        </div>
      </Card>

      <Card className="meetings-card shadow-sm border-0">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Meeting History</div>
            <div className="text-muted mt-1">Past completed meetings and notes</div>
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
        {selectedSlot ? (
          <div>
            <div className="meeting-dialog-banner mb-3">
              <div className="meeting-dialog-banner-icon">
                <i className="pi pi-calendar-plus" />
              </div>

              <div>
                <div className="meeting-dialog-banner-title">Selected Time Slot</div>
                <div className="meeting-dialog-banner-text">
                  {formatDateLong(selectedSlot.startAt)} • {formatTime(selectedSlot.startAt)} -{" "}
                  {formatTime(selectedSlot.endAt)}
                </div>
              </div>
            </div>

            <div className="meeting-reminder-box mb-3">
              Tell your advisor what you want to discuss so they can prepare before the session.
            </div>

            <label className="meeting-advisor-label mb-2 d-block">Reason for meeting</label>
            <InputTextarea
              rows={5}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Example: I want help choosing courses for next semester and reviewing my academic progress."
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
                disabled={savingRequest || !reason.trim()}
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

function MeetingBigCard({ meeting }) {
  const hasLink = meeting.meetingLink && meeting.meetingLink.trim();

  return (
    <div className="meeting-feature-card">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="meeting-feature-title">{meeting.title}</div>

          <div className="meeting-feature-meta mt-3">
            <span className="meeting-feature-meta-item">
              <i className="pi pi-calendar" />
              {formatDateLong(meeting.startAt)}
            </span>

            <span className="meeting-feature-meta-item">
              <i className="pi pi-clock" />
              {formatTime(meeting.startAt)} - {formatTime(meeting.endAt)}
            </span>

            <span className="meeting-feature-meta-item">
              <i className="pi pi-user" />
              {meeting.advisorName}
            </span>

            {meeting.advisorEmail ? (
              <span className="meeting-feature-meta-item">
                <i className="pi pi-envelope" />
                {meeting.advisorEmail}
              </span>
            ) : null}
          </div>
        </div>

        <Tag value={meeting.status} severity="info" className="meeting-status-tag" />
      </div>

      <Divider className="meeting-divider" />

      <div className="meeting-access-panel">
        <div>
          <div className="meeting-access-label">Meeting Access</div>
          <div className="meeting-access-subtext">
            Use the generated Google Meet link to join your advising session.
          </div>
        </div>

        {hasLink ? (
          <a
            href={meeting.meetingLink}
            target="_blank"
            rel="noreferrer"
            className="meeting-primary-link"
          >
            <i className="pi pi-video me-2" />
            Join Google Meet
          </a>
        ) : (
          <div className="meeting-link-pending-box">Meeting link is not available yet.</div>
        )}
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

function RequestSlotCard({ slot, onRequest }) {
  return (
    <div className="meeting-slot-card">
      <div className="meeting-slot-card-top">
        <div>
          <div className="meeting-slot-title">Available Slot</div>
          <div className="meeting-slot-date">{formatDateLong(slot.startAt)}</div>
        </div>

        <Tag value="OPEN" severity="success" className="meeting-status-tag" />
      </div>

      <div className="meeting-slot-timebox">
        <div className="meeting-slot-time">
          <i className="pi pi-clock" />
          {formatTime(slot.startAt)} - {formatTime(slot.endAt)}
        </div>
        <div className="meeting-slot-duration">
          {getDurationLabel(slot.startAt, slot.endAt)}
        </div>
      </div>

      <div className="meeting-slot-footer">
        <div className="meeting-slot-note">
          Choose this slot if it fits your schedule.
        </div>
        <Button
          label="Request This Slot"
          icon="pi pi-send"
          className="p-button-sm"
          onClick={onRequest}
        />
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

        <Tag value={meeting.status} severity="success" className="meeting-status-tag" />
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

function getDurationLabel(startLike, endLike) {
  const start = new Date(startLike);
  const end = new Date(endLike);
  const diffMs = end - start;
  const totalMinutes = Math.max(0, Math.round(diffMs / 60000));

  if (totalMinutes < 60) return `${totalMinutes} min`;
  const hours = Math.floor(totalMinutes / 60);
  const mins = totalMinutes % 60;

  if (!mins) return `${hours} hr`;
  return `${hours} hr ${mins} min`;
}