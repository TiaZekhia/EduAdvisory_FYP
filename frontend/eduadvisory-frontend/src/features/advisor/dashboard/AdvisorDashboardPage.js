import { useEffect, useState } from "react";
import { Card } from "primereact/card";
import { Skeleton } from "primereact/skeleton";
import { advisorApi } from "../../../services/advisors/advisorApi";
import { useAdvisorSummary } from "../context/AdvisorSummaryProvider";

export default function AdvisorDashboardPage() {
  const { summary } = useAdvisorSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [stats, setStats] = useState(null);
  const [upcomingMeetings, setUpcomingMeetings] = useState([]);
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      advisorApi.getDashboardSummary(),
      advisorApi.getUpcomingMeetings(5),
      advisorApi.getMessages(5),
    ])
      .then(([statsRes, meetingsRes, messagesRes]) => {
        setStats(statsRes.data);
        setUpcomingMeetings(meetingsRes.data || []);
        setMessages(messagesRes.data || []);
      })
      .catch((e) => {
        console.error(e);
        setErr(e?.response?.data ?? e?.message ?? "Failed to load advisor dashboard");
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="14rem" height="2rem" className="mb-2" />
        <Skeleton width="26rem" className="mb-4" />
        <div className="row g-3">
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="7rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="7rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="7rem" /></div>
          <div className="col-12 col-md-6 col-lg-3"><Skeleton height="7rem" /></div>
        </div>
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Dashboard</h2>
        <div className="text-muted">Overview of your advising activity</div>
      </div>

      <h3 className="mt-3">Welcome back, {summary?.name ?? "Advisor"}!</h3>
      <div className="text-muted mb-4">Manage your students, meetings, and messages</div>

      <div className="row g-3">
        <StatCard title="Advisees" value={stats?.totalAdvisees ?? 0} icon="pi pi-users" />
        <StatCard title="Upcoming Meetings" value={stats?.upcomingMeetings ?? 0} icon="pi pi-calendar" />
        <StatCard title="Total Meetings" value={stats?.totalMeetings ?? 0} icon="pi pi-clock" />
        <StatCard title="Announcements" value={stats?.totalAnnouncements ?? 0} icon="pi pi-envelope" />
      </div>

      <div className="row g-3 mt-4">
        <div className="col-12 col-lg-6">
          <Card className="shadow-sm border-0 h-100">
            <div className="fw-semibold fs-5">Upcoming Meetings</div>
            <div className="text-muted mb-3">Your next scheduled advising meetings</div>

            {upcomingMeetings.length === 0 ? (
              <div className="text-muted">No upcoming meetings.</div>
            ) : (
              <div className="d-flex flex-column gap-3">
                {upcomingMeetings.map((m) => (
                  <div key={m.meetingId} className="border rounded-3 p-3 bg-white">
                    <div className="fw-semibold">{m.title}</div>
                    <div className="text-muted small mt-1">
                      Student: {m.studentName}
                    </div>
                    <div className="text-muted small">
                      {new Date(m.meetingDate).toLocaleString()}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>

        <div className="col-12 col-lg-6">
          <Card className="shadow-sm border-0 h-100">
            <div className="fw-semibold fs-5">Recent Messages</div>
            <div className="text-muted mb-3">Your latest announcements</div>

            {messages.length === 0 ? (
              <div className="text-muted">No messages sent yet.</div>
            ) : (
              <div className="d-flex flex-column gap-3">
                {messages.map((m) => (
                  <div key={m.announcementId} className="border rounded-3 p-3 bg-white">
                    <div className="fw-semibold">{m.title}</div>
                    <div className="text-muted small mt-1">
                      {new Date(m.createdAt).toLocaleDateString()} • Sent to {m.recipientsCount} students
                    </div>
                  </div>
                ))}
              </div>
            )}
          </Card>
        </div>
      </div>
    </div>
  );
}

function StatCard({ title, value, icon }) {
  return (
    <div className="col-12 col-md-6 col-lg-3">
      <div className="border rounded-3 p-4 bg-white h-100 shadow-sm">
        <div className="d-flex justify-content-between align-items-center">
          <div>
            <div className="text-muted small">{title}</div>
            <div className="fs-2 fw-semibold mt-2">{value}</div>
          </div>
          <i className={`${icon} fs-2 text-muted`} />
        </div>
      </div>
    </div>
  );
}