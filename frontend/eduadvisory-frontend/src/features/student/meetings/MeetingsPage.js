import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { Divider } from "primereact/divider";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMeetingsApi } from "../../../services/students/studentMeetingsApi";
import { PageHero } from "../../../shared/components/PageHero";
import "./MeetingsPage.css";

export default function MeetingsPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [counts, setCounts] = useState({
    upcomingMeetings: 0,
    pastMeetings: 0,
    totalMeetings: 0,
  });
  const [upcoming, setUpcoming] = useState([]);
  const [past, setPast] = useState([]);
  const [advisor, setAdvisor] = useState(null);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      studentMeetingsApi.getSummary(),
      studentMeetingsApi.getUpcoming(3),
      studentMeetingsApi.getPast(10),
      studentMeetingsApi.getAdvisor(),
    ])
      .then(([summaryRes, upcomingRes, pastRes, advisorRes]) => {
        setCounts(
          summaryRes.data || {
            upcomingMeetings: 0,
            pastMeetings: 0,
            totalMeetings: 0,
          }
        );
        setUpcoming(upcomingRes.data || []);
        setPast(pastRes.data || []);
        setAdvisor(advisorRes.data || null);
      })
      .catch((e) => {
        console.error(e);
        setErr(e?.response?.data ?? e?.message ?? "Failed to load meetings");
      })
      .finally(() => setLoading(false));
  }, []);

  const nextMeeting = useMemo(
    () => (upcoming?.length ? upcoming[0] : null),
    [upcoming]
  );

  if (loading) {
    return (
      <div className="meetings-page container-fluid p-4 p-lg-5">
        <div className="mb-4">
          <Skeleton width="14rem" height="2.2rem" className="mb-2" />
          <Skeleton width="24rem" height="1.2rem" />
        </div>

        <div className="row g-4 mb-4">
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
          <div className="col-12 col-md-4">
            <Skeleton height="9rem" borderRadius="20px" />
          </div>
        </div>

        <Skeleton height="20rem" className="mb-4" borderRadius="24px" />
        <Skeleton height="22rem" className="mb-4" borderRadius="24px" />
        <Skeleton height="14rem" borderRadius="24px" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="meetings-page container-fluid p-3 p-md-4">
      <PageHero
        title="Meetings"
        badge={`${summary?.programCode ?? "-"} • Semester ${
          summary?.currentSemester ?? "-"
        }`}
        subtitle="View your scheduled and past meetings with your academic advisor"
      />

      <div className="row g-4 mb-4">
        <CountCard
          title="Upcoming Meetings"
          value={counts.upcomingMeetings}
          icon="pi pi-calendar-plus"
        />
        <CountCard
          title="Past Meetings"
          value={counts.pastMeetings}
          icon="pi pi-history"
        />
        <CountCard
          title="Total Meetings"
          value={counts.totalMeetings}
          icon="pi pi-users"
        />
      </div>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Upcoming Meeting</div>
            <div className="text-muted mt-1">
              Your next scheduled session with your advisor
            </div>
          </div>
        </div>

        {nextMeeting ? (
          <MeetingBigCard meeting={nextMeeting} />
        ) : (
          <div className="meetings-empty-state">
            <div className="meetings-empty-icon">
              <i className="pi pi-calendar" />
            </div>
            <div>
              <div className="meetings-empty-title">No upcoming meetings</div>
              <div className="meetings-empty-text">
                You do not have any scheduled meetings right now.
              </div>
            </div>
          </div>
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0 mb-4">
        <div className="meetings-card-header">
          <div>
            <div className="fw-semibold fs-4">Meeting History</div>
            <div className="text-muted mt-1">
              Past meetings and advisor notes
            </div>
          </div>
        </div>

        {past?.length ? (
          <div className="d-flex flex-column gap-3">
            {past.map((m) => (
              <MeetingHistoryRow key={m.meetingId} meeting={m} />
            ))}
          </div>
        ) : (
          <div className="meetings-empty-state">
            <div className="meetings-empty-icon">
              <i className="pi pi-clock" />
            </div>
            <div>
              <div className="meetings-empty-title">No past meetings</div>
              <div className="meetings-empty-text">
                Your completed meetings will appear here.
              </div>
            </div>
          </div>
        )}
      </Card>

      <Card className="meetings-card shadow-sm border-0">
        <div className="fw-semibold fs-4">Need to Schedule a Meeting?</div>
        <div className="text-muted mb-3">
          Contact your advisor to arrange an appointment
        </div>

        <div className="meeting-advisor-box">
          <div className="d-flex align-items-center justify-content-between flex-wrap gap-3 mb-3">
            <div>
              <div className="meeting-advisor-label">Your Academic Advisor</div>
              <div className="meeting-advisor-name">
                {advisor?.name ?? "Not assigned"}
              </div>
            </div>

            <Tag
              value={advisor?.availability ?? "Unknown"}
              severity="success"
              className="meeting-status-tag"
            />
          </div>

          <div className="row g-3">
            <div className="col-12 col-md-6">
              <div className="meeting-info-tile">
                <div className="meeting-info-label">
                  <i className="pi pi-envelope" />
                  Email
                </div>
                <div className="meeting-info-value">{advisor?.email ?? "-"}</div>
              </div>
            </div>

            <div className="col-12 col-md-6">
              <div className="meeting-info-tile">
                <div className="meeting-info-label">
                  <i className="pi pi-clock" />
                  Office Hours
                </div>
                <div className="meeting-info-value">
                  {advisor?.officeHours ?? "Mon, Wed, Fri: 2-4 PM"}
                </div>
              </div>
            </div>
          </div>

          <div className="meeting-advisor-note mt-3">
            You can request a meeting through the university&apos;s advising
            portal or by sending an email to your advisor.
          </div>
        </div>
      </Card>
    </div>
  );
}

function CountCard({ title, value, icon }) {
  return (
    <div className="col-12 col-md-4">
      <div className="meeting-stat-card h-100">
        <div className="meeting-stat-card-top">
          <div className="meeting-stat-icon">
            {icon ? <i className={icon} /> : null}
          </div>
          <div className="meeting-stat-title">{title}</div>
        </div>
        <div className="meeting-stat-value">{value}</div>
      </div>
    </div>
  );
}

function MeetingBigCard({ meeting }) {
  const when = formatDateLong(meeting.meetingDate);
  const time = formatTime(meeting.meetingDate);

  return (
    <div className="meeting-feature-card">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="meeting-feature-title">{meeting.title}</div>

          <div className="meeting-feature-meta mt-3">
            <span className="meeting-feature-meta-item">
              <i className="pi pi-calendar" />
              {when}
            </span>

            <span className="meeting-feature-meta-item">
              <i className="pi pi-clock" />
              {time} (30 minutes)
            </span>

            <span className="meeting-feature-meta-item">
              <i className="pi pi-user" />
              Your Academic Advisor
            </span>

            <span className="meeting-feature-meta-item">
              <i className="pi pi-envelope" />
              {meeting.advisorEmail}
            </span>
          </div>
        </div>

        <Tag
          value={meeting.status}
          severity="info"
          className="meeting-status-tag"
        />
      </div>

      <Divider className="meeting-divider" />

      <div className="meeting-reminder-box">
        <span className="fw-semibold">Reminder:</span>{" "}
        {meeting.notes?.length
          ? meeting.notes
          : "Please prepare any questions or concerns you'd like to discuss during this meeting. Bring your current course schedule and any documents related to your academic progress."}
      </div>
    </div>
  );
}

function MeetingHistoryRow({ meeting }) {
  const date = formatDateShort(meeting.meetingDate);
  const time = formatTime(meeting.meetingDate);

  return (
    <div className="meeting-history-row">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-3">
        <div className="flex-grow-1">
          <div className="meeting-history-title">{meeting.title}</div>

          <div className="meeting-history-meta mt-2">
            <span className="meeting-history-meta-item">
              <i className="pi pi-calendar" />
              {date}
            </span>
            <span className="meeting-history-meta-item">
              <i className="pi pi-clock" />
              {time}
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

        <Tag
          value={meeting.status}
          severity="success"
          className="meeting-status-tag"
        />
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