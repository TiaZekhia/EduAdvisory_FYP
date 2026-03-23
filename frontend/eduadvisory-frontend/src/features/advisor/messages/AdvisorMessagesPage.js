import { useEffect, useState } from "react";
import { Card } from "primereact/card";
import { Skeleton } from "primereact/skeleton";
import { advisorApi } from "../../../services/advisors/advisorApi";
import { useAdvisorSummary } from "../context/AdvisorSummaryProvider";

export default function AdvisorMessagesPage() {
  const { summary } = useAdvisorSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [stats, setStats] = useState(null);
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    setLoading(true);
    setErr("");

    Promise.all([
      advisorApi.getMessagesSummary(),
      advisorApi.getMessages(20),
    ])
      .then(([statsRes, messagesRes]) => {
        setStats(statsRes.data);
        setMessages(messagesRes.data || []);
      })
      .catch((e) => {
        console.error(e);
        setErr(e?.response?.data ?? e?.message ?? "Failed to load messages");
      })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="12rem" height="2rem" className="mb-2" />
        <Skeleton width="28rem" className="mb-4" />
        <div className="row g-3 mb-4">
          <div className="col-12 col-md-4"><Skeleton height="7rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="7rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="7rem" /></div>
        </div>
        <Skeleton height="20rem" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Messages</h2>
        <div className="text-muted">{summary?.name ?? "Advisor"} • Broadcast announcements</div>
      </div>

      <h3 className="mt-3">Messages Sent</h3>
      <div className="text-muted mb-4">Announcements sent to your advisees</div>

      <div className="row g-3 mb-4">
        <StatCard title="Total Messages" value={stats?.totalMessages ?? 0} />
        <StatCard title="Recent (7 days)" value={stats?.recent7Days ?? 0} />
        <StatCard title="This Month" value={stats?.thisMonth ?? 0} />
      </div>

      <Card className="shadow-sm border-0">
        {messages.length === 0 ? (
          <div className="text-muted">No messages sent yet.</div>
        ) : (
          <div className="d-flex flex-column gap-3">
            {messages.map((m) => (
              <div key={m.announcementId} className="border rounded-3 p-3 bg-white">
                <div className="fw-semibold fs-5">{m.title}</div>
                <div className="text-muted small mt-1">
                  {new Date(m.createdAt).toLocaleDateString()} • Sent to {m.recipientsCount} students
                </div>
                <div className="mt-3 text-muted" style={{ whiteSpace: "pre-wrap" }}>
                  {m.content}
                </div>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}

function StatCard({ title, value }) {
  return (
    <div className="col-12 col-md-4">
      <div className="border rounded-3 p-4 bg-white h-100 shadow-sm">
        <div className="text-muted small">{title}</div>
        <div className="fs-1 fw-semibold mt-2">{value}</div>
      </div>
    </div>
  );
}