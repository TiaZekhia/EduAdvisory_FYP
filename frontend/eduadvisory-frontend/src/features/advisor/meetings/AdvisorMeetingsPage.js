import { useEffect, useState } from "react";
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
import "../../student/meetings/MeetingsPage.css";

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

  const [responseDialogOpen, setResponseDialogOpen] = useState(false);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [decision, setDecision] = useState("ACCEPTED");
  const [rejectionReason, setRejectionReason] = useState("");

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

    try {
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

    try {
      setErr("");

      await advisorMeetingsApi.respondToRequest(selectedRequest.requestId, {
        decision,
        rejectionReason: decision === "REJECTED" ? rejectionReason : null,
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

  if (err) {
    return (
      <div className="container-fluid p-3 p-md-4">
        <PageHero
          title="Advisor Meetings"
          badge="Advisor Portal"
          subtitle="Manage your availability, pending meeting requests, and scheduled advising sessions"
        />
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
        <CountCard
          title="Available Slots"
          value={availability.filter((x) => !x.isBooked && x.isActive).length}
          icon="pi pi-calendar-plus"
        />
        <CountCard
          title="Pending Requests"
          value={pendingRequests.length}
          icon="pi pi-inbox"
        />
        <CountCard
          title="Upcoming Meetings"
          value={upcomingMeetings.length}
          icon="pi pi-users"
        />
      </div>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="d-flex justify-content-between align-items-center flex-wrap gap-3 mb-4">
          <div>
            <div className="fw-semibold fs-4">My Availability</div>
            <div className="text-muted mt-1">
              Add and manage available advising slots
            </div>
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
                <div className="meeting-history-row">
                  <div className="d-flex justify-content-between gap-3 flex-wrap">
                    <div>
                      <div className="meeting-history-title">Availability Slot</div>
                      <div className="meeting-history-meta mt-2">
                        <span className="meeting-history-meta-item">
                          <i className="pi pi-calendar" />
                          {formatDateShort(slot.startAt)}
                        </span>
                        <span className="meeting-history-meta-item">
                          <i className="pi pi-clock" />
                          {formatTime(slot.startAt)} - {formatTime(slot.endAt)}
                        </span>
                      </div>
                    </div>

                    <div className="d-flex flex-column gap-2 align-items-end">
                      <Tag
                        value={slot.isBooked ? "BOOKED" : "AVAILABLE"}
                        severity={slot.isBooked ? "warning" : "success"}
                        className="meeting-status-tag"
                      />
                      {!slot.isBooked ? (
                        <Button
                          label="Delete"
                          icon="pi pi-trash"
                          className="p-button-sm p-button-outlined p-button-danger"
                          onClick={() => handleDeleteAvailability(slot.availabilityId)}
                        />
                      ) : null}
                    </div>
                  </div>
                </div>
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
        <div className="fw-semibold fs-4 mb-1">Pending Requests</div>
        <div className="text-muted mb-4">
          Review and accept or reject student requests
        </div>

        {pendingRequests.length ? (
          <div className="d-flex flex-column gap-3">
            {pendingRequests.map((request) => (
              <div className="meeting-history-row" key={request.requestId}>
                <div className="d-flex justify-content-between flex-wrap gap-3">
                  <div className="flex-grow-1">
                    <div className="meeting-history-title">{request.studentName}</div>
                    <div className="meeting-history-meta mt-2">
                      <span className="meeting-history-meta-item">
                        <i className="pi pi-calendar" />
                        {formatDateShort(request.startAt)}
                      </span>
                      <span className="meeting-history-meta-item">
                        <i className="pi pi-clock" />
                        {formatTime(request.startAt)} - {formatTime(request.endAt)}
                      </span>
                    </div>

                    <div className="meeting-notes-box mt-3">
                      {request.reason || "No reason provided."}
                    </div>
                  </div>

                  <div className="d-flex flex-column gap-2">
                    <Tag
                      value={request.status}
                      severity="warning"
                      className="meeting-status-tag"
                    />
                    <Button
                      label="Respond"
                      icon="pi pi-check"
                      className="p-button-sm"
                      onClick={() => openRespondDialog(request)}
                    />
                  </div>
                </div>
              </div>
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
        <div className="fw-semibold fs-4 mb-1">Upcoming Meetings</div>
        <div className="text-muted mb-4">Confirmed meetings with students</div>

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
        <div className="fw-semibold fs-4 mb-1">Meeting History</div>
        <div className="text-muted mb-4">Past meetings and completed sessions</div>

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
        style={{ width: "35rem", maxWidth: "95vw" }}
        onHide={() => setSlotDialogOpen(false)}
      >
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

        <div className="d-flex justify-content-end gap-2 mt-4">
          <Button
            label="Cancel"
            className="p-button-text"
            onClick={() => setSlotDialogOpen(false)}
          />
          <Button label="Save Slot" icon="pi pi-check" onClick={handleCreateAvailability} />
        </div>
      </Dialog>

      <Dialog
        header="Respond to Meeting Request"
        visible={responseDialogOpen}
        style={{ width: "38rem", maxWidth: "95vw" }}
        onHide={() => setResponseDialogOpen(false)}
      >
        {selectedRequest ? (
          <div>
            <div className="meeting-notes-box mb-3">
              <div>
                <strong>Student:</strong> {selectedRequest.studentName}
              </div>
              <div>
                <strong>Date:</strong> {formatDateShort(selectedRequest.startAt)}
              </div>
              <div>
                <strong>Time:</strong> {formatTime(selectedRequest.startAt)} -{" "}
                {formatTime(selectedRequest.endAt)}
              </div>
            </div>

            <div className="d-flex gap-2 mb-3">
              <Button
                label="Accept"
                className={decision === "ACCEPTED" ? "" : "p-button-outlined"}
                onClick={() => setDecision("ACCEPTED")}
              />
              <Button
                label="Reject"
                severity="danger"
                className={decision === "REJECTED" ? "" : "p-button-outlined"}
                onClick={() => setDecision("REJECTED")}
              />
            </div>

            {decision === "ACCEPTED" ? (
              <div className="meeting-reminder-box mb-3">
                A Google Meet link will be generated automatically when you accept this request.
              </div>
            ) : (
              <div className="mb-3">
                <label className="meeting-advisor-label d-block mb-2">
                  Rejection Reason
                </label>
                <InputTextarea
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  rows={4}
                  className="w-100"
                />
              </div>
            )}

            <div className="d-flex justify-content-end gap-2 mt-4">
              <Button
                label="Cancel"
                className="p-button-text"
                onClick={() => setResponseDialogOpen(false)}
              />
              <Button label="Submit" icon="pi pi-check" onClick={handleRespond} />
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