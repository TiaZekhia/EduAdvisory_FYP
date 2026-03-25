import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Button } from "primereact/button";
import { Dialog } from "primereact/dialog";
import { Calendar } from "primereact/calendar";
import { InputTextarea } from "primereact/inputtextarea";
import { Message } from "primereact/message";
import { Skeleton } from "primereact/skeleton";

import { advisorMeetingsApi } from "../../../services/advisors/advisorMeetingsApi";
import { PageHero } from "../../../shared/components/PageHero";
import "./AdvisorMeetingsPage.css";

export default function AdvisorMeetingsPage() {
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [msg, setMsg] = useState("");

  const [availability, setAvailability] = useState([]);
  const [pendingRequests, setPendingRequests] = useState([]);
  const [upcomingMeetings, setUpcomingMeetings] = useState([]);
  const [historyMeetings, setHistoryMeetings] = useState([]);

  const [slotDialogOpen, setSlotDialogOpen] = useState(false);
  const [newStartAt, setNewStartAt] = useState(null);
  const [newEndAt, setNewEndAt] = useState(null);
  const [savingSlot, setSavingSlot] = useState(false);

  const [responseDialogOpen, setResponseDialogOpen] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [decision, setDecision] = useState("ACCEPTED");
  const [rejectionReason, setRejectionReason] = useState("");
  const [responding, setResponding] = useState(false);

  const activeSlotsCount = useMemo(
    () => availability.filter((x) => !x.isBooked && x.isActive).length,
    [availability]
  );

  const loadData = async () => {
    setLoading(true);
    setErr("");

    try {
      const [availabilityRes, pendingRes, upcomingRes, historyRes] = await Promise.all([
        advisorMeetingsApi.getAvailability(),
        advisorMeetingsApi.getPendingRequests(),
        advisorMeetingsApi.getUpcoming(),
        advisorMeetingsApi.getHistory(),
      ]);

      setAvailability(availabilityRes.data || []);
      setPendingRequests(pendingRes.data || []);
      setUpcomingMeetings(upcomingRes.data || []);
      setHistoryMeetings(historyRes.data || []);
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to load advisor meetings.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleCreateAvailability = async () => {
    if (!newStartAt || !newEndAt) {
      setErr("Please select both start and end time.");
      return;
    }

    if (new Date(newEndAt) <= new Date(newStartAt)) {
      setErr("End time must be after start time.");
      return;
    }

    try {
      setSavingSlot(true);
      setErr("");
      await advisorMeetingsApi.createAvailability({
        startAt: newStartAt,
        endAt: newEndAt,
      });

      setMsg("Availability slot created.");
      setSlotDialogOpen(false);
      setNewStartAt(null);
      setNewEndAt(null);
      await loadData();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to create availability.");
    } finally {
      setSavingSlot(false);
    }
  };

  const handleDeleteAvailability = async (id) => {
    try {
      setErr("");
      await advisorMeetingsApi.deleteAvailability(id);
      setMsg("Availability slot removed.");
      await loadData();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to delete slot.");
    }
  };

  const openRespondDialog = (request) => {
    setSelectedRequest(request);
    setDecision("ACCEPTED");
    setRejectionReason("");
    setResponseDialogOpen(true);
  };

  const handleRespond = async () => {
    if (!selectedRequest) return;

    if (decision === "REJECTED" && !rejectionReason.trim()) {
      setErr("Please provide a rejection reason.");
      return;
    }

    try {
      setResponding(true);
      setErr("");

      await advisorMeetingsApi.respondToRequest(selectedRequest.requestId, {
        decision,
        rejectionReason: decision === "REJECTED" ? rejectionReason.trim() : null,
      });

      setMsg(
        decision === "ACCEPTED"
          ? "Request accepted and Google Meet link generated."
          : "Request rejected successfully."
      );

      setResponseDialogOpen(false);
      await loadData();
    } catch (e) {
      console.error(e);
      setErr(e?.response?.data ?? e?.message ?? "Failed to respond to request.");
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

  if (err && !loading) {
    return (
      <div className="container-fluid p-3 p-md-4">
        <PageHero
          title="Advisor Meetings"
          badge="Advisor Portal"
          subtitle="Manage your availability, pending meeting requests, and scheduled advising sessions"
        />
        {msg ? <Message severity="success" text={msg} className="mb-4" /> : null}
        <div className="p-4 text-danger">{String(err)}</div>
      </div>
    );
  }

  return (
    <div className="container-fluid p-3 p-md-4">
      <PageHero
        title="Advisor Meetings"
        badge="Advisor Portal"
        subtitle="Manage your availability, pending meeting requests, and scheduled advising sessions"
      />

      {msg ? <Message severity="success" text={msg} className="mb-4" /> : null}

      <div className="row g-4 mb-4">
        <CountCard title="Available Slots" value={activeSlotsCount} icon="pi pi-calendar-plus" />
        <CountCard title="Pending Requests" value={pendingRequests.length} icon="pi pi-inbox" />
        <CountCard title="Upcoming Meetings" value={upcomingMeetings.length} icon="pi pi-users" />
      </div>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">My Availability</div>
            <div className="text-muted mt-1">Add and manage available advising slots</div>
          </div>

          <Button
            label="Add Slot"
            icon="pi pi-plus"
            onClick={() => setSlotDialogOpen(true)}
          />
        </div>

        {availability.length ? (
          <div className="row g-3">
            {availability.map((slot) => (
              <div className="col-12 col-lg-6" key={slot.availabilityId}>
                <AvailabilitySlotCard slot={slot} onDelete={handleDeleteAvailability} />
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            icon="pi pi-calendar"
            title="No availability slots"
            text="Add a slot so students can request meetings."
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
            <div className="text-muted mt-1">Confirmed meetings with students</div>
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
            text="Accepted meetings will appear here."
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
              <MeetingCard key={meeting.meetingId} meeting={meeting} />
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
        header="Add Availability Slot"
        visible={slotDialogOpen}
        style={{ width: "36rem", maxWidth: "95vw" }}
        onHide={() => setSlotDialogOpen(false)}
      >
        <div className="meeting-dialog-banner mb-3">
          <div className="meeting-dialog-banner-icon">
            <i className="pi pi-calendar-plus" />
          </div>
          <div>
            <div className="meeting-dialog-banner-title">Publish a New Time Slot</div>
            <div className="meeting-dialog-banner-text">
              Students will be able to request meetings for this time once published.
            </div>
          </div>
        </div>

        <div className="mb-3">
          <label className="meeting-advisor-label d-block mb-2">Start</label>
          <Calendar
            value={newStartAt}
            onChange={(e) => setNewStartAt(e.value)}
            showTime
            hourFormat="12"
            className="w-100"
          />
        </div>

        <div className="mb-3">
          <label className="meeting-advisor-label d-block mb-2">End</label>
          <Calendar
            value={newEndAt}
            onChange={(e) => setNewEndAt(e.value)}
            showTime
            hourFormat="12"
            className="w-100"
          />
        </div>

        <div className="meeting-reminder-box">
          Tip: keeping slots between 20 and 30 minutes usually works well for advising sessions.
        </div>

        <div className="d-flex justify-content-end gap-2 mt-4">
          <Button
            label="Cancel"
            className="p-button-text"
            onClick={() => setSlotDialogOpen(false)}
          />
          <Button
            label={savingSlot ? "Saving..." : "Save Slot"}
            icon="pi pi-check"
            onClick={handleCreateAvailability}
            disabled={savingSlot || !newStartAt || !newEndAt}
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
                  <div className="decision-option-title">Accept</div>
                  <div className="decision-option-text">
                    Confirm this meeting and generate a Google Meet link
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
                A Google Meet link will be generated automatically when you accept this request.
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

            <div className="d-flex justify-content-end gap-2 mt-4">
              <Button
                label="Cancel"
                className="p-button-text"
                onClick={() => setResponseDialogOpen(false)}
              />
              <Button
                label={responding ? "Submitting..." : decision === "ACCEPTED" ? "Accept Request" : "Reject Request"}
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

function AvailabilitySlotCard({ slot, onDelete }) {
  return (
    <div className={`meeting-slot-card ${slot.isBooked ? "slot-booked" : ""}`}>
      <div className="meeting-slot-card-top">
        <div>
          <div className="meeting-slot-title">Availability Slot</div>
          <div className="meeting-slot-date">{formatDateLong(slot.startAt)}</div>
        </div>

        <Tag
          value={slot.isBooked ? "BOOKED" : "AVAILABLE"}
          severity={slot.isBooked ? "warning" : "success"}
          className="meeting-status-tag"
        />
      </div>

      <div className="meeting-slot-timebox">
        <div className="meeting-slot-time">
          <i className="pi pi-clock" />
          {formatTime(slot.startAt)} - {formatTime(slot.endAt)}
        </div>
        <div className="meeting-slot-duration">{getDurationLabel(slot.startAt, slot.endAt)}</div>
      </div>

      <div className="meeting-slot-footer">
        <div className="meeting-slot-note">
          {slot.isBooked ? "This slot already has a student request or booking." : "This slot is open for students to request."}
        </div>

        {!slot.isBooked ? (
          <Button
            label="Delete"
            icon="pi pi-trash"
            className="p-button-sm p-button-outlined p-button-danger"
            onClick={() => onDelete(slot.availabilityId)}
          />
        ) : null}
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
          Review the request details, then accept or reject it.
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

function MeetingCard({ meeting }) {
  return (
    <div className="meeting-history-row">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="meeting-history-title">{meeting.title}</div>
          <div className="meeting-history-meta mt-2">
            <span className="meeting-history-meta-item">
              <i className="pi pi-user" />
              {meeting.studentName}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-calendar" />
              {formatDateShort(meeting.startAt)}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-clock" />
              {formatTime(meeting.startAt)} - {formatTime(meeting.endAt)}
            </span>
          </div>

          <div className="meeting-reminder-box mt-3">
            {meeting.meetingLink ? (
              <a href={meeting.meetingLink} target="_blank" rel="noreferrer">
                Open Meeting Link
              </a>
            ) : (
              <span className="text-muted">Meeting link not available.</span>
            )}
          </div>
        </div>

        <Tag value={meeting.status} severity="info" className="meeting-status-tag" />
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