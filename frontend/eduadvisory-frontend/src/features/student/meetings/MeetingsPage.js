import { useEffect, useMemo, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { Divider } from "primereact/divider";

import { useStudentSummary } from "../context/StudentSummaryProvider"; // adjust if different
import { studentMeetingsApi } from "../../../services/students/studentMeetingsApi"; // adjust if different

export default function MeetingsPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [counts, setCounts] = useState({ upcomingMeetings: 0, pastMeetings: 0, totalMeetings: 0 });
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
        setCounts(summaryRes.data || { upcomingMeetings: 0, pastMeetings: 0, totalMeetings: 0 });
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

  const nextMeeting = useMemo(() => (upcoming?.length ? upcoming[0] : null), [upcoming]);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="14rem" height="2rem" className="mb-2" />
        <Skeleton width="26rem" className="mb-4" />

        <div className="row g-3 mb-4">
          <div className="col-12 col-md-4"><Skeleton height="6rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="6rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="6rem" /></div>
        </div>

        <Skeleton height="14rem" className="mb-4" />
        <Skeleton height="18rem" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="container-fluid p-4">
      {/* Header */}
      <div className="mb-3">
        <h2 className="m-0">Meetings</h2>
        <div className="text-muted">
          {summary?.programCode ?? "-"} • Semester {summary?.currentSemester ?? "-"}
        </div>
      </div>

      <h3 className="mt-3">Meetings with Advisor</h3>
      <div className="text-muted mb-4">View your scheduled and past meetings</div>

      {/* Counters */}
      <div className="row g-3 mb-4">
        <CountCard title="Upcoming Meetings" value={counts.upcomingMeetings} />
        <CountCard title="Past Meetings" value={counts.pastMeetings} />
        <CountCard title="Total Meetings" value={counts.totalMeetings} />
      </div>

      {/* Upcoming meeting */}
      <Card className="shadow-sm border-0 mb-4">
        <div className="fw-semibold">Upcoming Meetings</div>
        <div className="text-muted mb-3">Your scheduled meetings with your advisor</div>

        {nextMeeting ? (
          <MeetingBigCard meeting={nextMeeting} />
        ) : (
          <div className="text-muted">No upcoming meetings.</div>
        )}
      </Card>

      {/* Meeting history */}
      <Card className="shadow-sm border-0 mb-4">
        <div className="fw-semibold">Meeting History</div>
        <div className="text-muted mb-3">Past meetings and advisor notes</div>

        {past?.length ? (
          <div className="d-flex flex-column gap-3">
            {past.map((m) => (
              <MeetingHistoryRow key={m.meetingId} meeting={m} />
            ))}
          </div>
        ) : (
          <div className="text-muted">No past meetings.</div>
        )}
      </Card>

      {/* Schedule section */}
      <Card className="shadow-sm border-0">
        <div className="fw-semibold">Need to Schedule a Meeting?</div>
        <div className="text-muted mb-3">Contact your advisor to arrange an appointment</div>

        <div className="border rounded-3 p-3 bg-white">
          <div className="d-flex align-items-center justify-content-between flex-wrap gap-2">
            <div>
              <div className="fw-semibold">Your Academic Advisor</div>
              <div className="text-muted">{advisor?.name ?? "Not assigned"}</div>
            </div>
            <Tag value={advisor?.availability ?? "Unknown"} severity="success" />
          </div>

          <div className="row g-3 mt-2">
            <div className="col-12 col-md-6">
              <div className="text-muted small mb-1">Email</div>
              <div className="border rounded-3 p-3 bg-white">{advisor?.email ?? "-"}</div>
            </div>
            <div className="col-12 col-md-6">
              <div className="text-muted small mb-1">Office Hours</div>
              <div className="border rounded-3 p-3 bg-white">{advisor?.officeHours ?? "Mon, Wed, Fri: 2-4 PM"}</div>
            </div>
          </div>

          <div className="text-muted small mt-3">
            You can request a meeting through the university&apos;s advising portal or by sending an email to your advisor.
          </div>
        </div>
      </Card>
    </div>
  );
}

function CountCard({ title, value }) {
  return (
    <div className="col-12 col-md-4">
      <div className="border rounded-3 p-4 bg-white h-100">
        <div className="fw-semibold">{title}</div>
        <div className="fs-1 fw-semibold mt-2">{value}</div>
      </div>
    </div>
  );
}

function MeetingBigCard({ meeting }) {
  const when = formatDateLong(meeting.meetingDate);
  const time = formatTime(meeting.meetingDate);

  return (
    <div className="border rounded-3 p-4 bg-white">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-2">
        <div>
          <div className="fw-semibold fs-5">{meeting.title}</div>

          <div className="mt-2 d-flex flex-column gap-2 text-muted">
            <div className="d-flex align-items-center gap-2">
              <i className="pi pi-calendar" />
              <span>{when}</span>
            </div>

            <div className="d-flex align-items-center gap-2">
              <i className="pi pi-clock" />
              <span>{time} (30 minutes)</span>
            </div>

            <div className="d-flex align-items-center gap-2">
              <i className="pi pi-user" />
              <span>Your Academic Advisor</span>
            </div>

            <div className="d-flex align-items-center gap-2">
              <i className="pi pi-envelope" />
              <span>{meeting.advisorEmail}</span>
            </div>
          </div>
        </div>

        <Tag value={meeting.status} severity="info" />
      </div>

      <Divider />

      <div className="p-3 rounded-3" style={{ background: "#f3f4f6" }}>
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
    <div className="border rounded-3 p-3 bg-white">
      <div className="d-flex align-items-start justify-content-between flex-wrap gap-2">
        <div>
          <div className="fw-semibold">{meeting.title}</div>

          <div className="text-muted small mt-1 d-flex flex-wrap gap-3">
            <span className="d-flex align-items-center gap-2">
              <i className="pi pi-calendar" />
              {date}
            </span>
            <span className="d-flex align-items-center gap-2">
              <i className="pi pi-clock" />
              {time}
            </span>
          </div>

          <div className="mt-3">
            <div className="text-muted small d-flex align-items-center gap-2">
              <i className="pi pi-file" />
              <span className="fw-semibold">Advisor Notes:</span>
            </div>
            <div className="mt-2 p-3 rounded-3" style={{ background: "#f3f4f6" }}>
              {meeting.notes?.length ? meeting.notes : "No notes."}
            </div>
          </div>
        </div>

        <Tag value={meeting.status} severity="success" />
      </div>
    </div>
  );
}

function formatDateLong(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, { weekday: "long", year: "numeric", month: "long", day: "numeric" });
}

function formatDateShort(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

function formatTime(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" });
}