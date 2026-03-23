import { useEffect, useState } from "react";
import { Card } from "primereact/card";
import { Tag } from "primereact/tag";
import { Skeleton } from "primereact/skeleton";
import { advisorApi } from "../../../services/advisors/advisorApi";
import { useAdvisorSummary } from "../context/AdvisorSummaryProvider";

export default function AdvisorMeetingsPage() {
  const { summary } = useAdvisorSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [upcoming, setUpcoming] = useState([]);
  const [past, setPast] = useState([]);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      advisorApi.getUpcomingMeetings(10),
      advisorApi.getPastMeetings(10),
    ])
      .then(([upcomingRes, pastRes]) => {
        setUpcoming(upcomingRes.data || []);
        setPast(pastRes.data || []);
      })
      .catch((e) => {
        console.error(e);
        setErr(e?.response?.data ?? e?.message ?? "Failed to load meetings");
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="12rem" height="2rem" className="mb-2" />
        <Skeleton width="26rem" className="mb-4" />
        <Skeleton height="14rem" className="mb-4" />
        <Skeleton height="18rem" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Meetings</h2>
        <div className="text-muted">{summary?.name ?? "Advisor"} • Meeting activity</div>
      </div>

      <h3 className="mt-3">Upcoming Meetings</h3>
      <div className="text-muted mb-4">Your scheduled advising meetings</div>

      <Card className="shadow-sm border-0 mb-4">
        {upcoming.length === 0 ? (
          <div className="text-muted">No upcoming meetings.</div>
        ) : (
          <div className="d-flex flex-column gap-3">
            {upcoming.map((m) => (
              <MeetingRow key={m.meetingId} meeting={m} severity="info" />
            ))}
          </div>
        )}
      </Card>

      <h3 className="mt-3">Meeting History</h3>
      <div className="text-muted mb-4">Completed meetings with your students</div>

      <Card className="shadow-sm border-0">
        {past.length === 0 ? (
          <div className="text-muted">No past meetings.</div>
        ) : (
          <div className="d-flex flex-column gap-3">
            {past.map((m) => (
              <MeetingRow key={m.meetingId} meeting={m} severity="success" />
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}

function MeetingRow({ meeting, severity }) {
  return (
    <div className="border rounded-3 p-3 bg-white">
      <div className="d-flex justify-content-between align-items-start flex-wrap gap-2">
        <div>
          <div className="fw-semibold">{meeting.title}</div>
          <div className="text-muted small mt-1">Student: {meeting.studentName}</div>
          <div className="text-muted small">{new Date(meeting.meetingDate).toLocaleString()}</div>
          {meeting.notes ? <div className="text-muted small mt-2">{meeting.notes}</div> : null}
        </div>
        <Tag value={meeting.status} severity={severity} />
      </div>
    </div>
  );
}