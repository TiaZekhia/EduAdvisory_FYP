import { useEffect, useState } from "react";
import { Card } from "primereact/card";
import { Skeleton } from "primereact/skeleton";

import { useStudentSummary } from "../context/StudentSummaryProvider";
import { studentMessagesApi } from "../../../services/students/studentMessagesApi";

export default function MessagesPage() {
  const { summary } = useStudentSummary();

  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  const [stats, setStats] = useState({
    totalMessages: 0,
    recent7Days: 0,
    thisMonth: 0,
  });

  const [messages, setMessages] = useState([]);
  const [advisor, setAdvisor] = useState(null);

  useEffect(() => {
    let mounted = true;

    setLoading(true);
    setErr("");

    Promise.all([
      studentMessagesApi.getSummary(),
      studentMessagesApi.getMessages(20),
      studentMessagesApi.getAdvisor(),
    ])
      .then(([summaryRes, messagesRes, advisorRes]) => {
        if (!mounted) return;

        setStats(summaryRes?.data ?? {
          totalMessages: 0,
          recent7Days: 0,
          thisMonth: 0,
        });

        setMessages(messagesRes?.data ?? []);
        setAdvisor(advisorRes?.data ?? null);
      })
      .catch((e) => {
        console.error("Messages load error:", e);
        if (!mounted) return;
        setErr(e?.response?.data ?? e?.message ?? "Failed to load messages");
      })
      .finally(() => {
        if (!mounted) return;
        setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, []);

  if (loading) {
    return (
      <div className="container-fluid p-4">
        <Skeleton width="12rem" height="2rem" className="mb-2" />
        <Skeleton width="28rem" className="mb-4" />

        <div className="row g-3 mb-4">
          <div className="col-12 col-md-4"><Skeleton height="8rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="8rem" /></div>
          <div className="col-12 col-md-4"><Skeleton height="8rem" /></div>
        </div>

        <Skeleton height="20rem" className="mb-4" />
        <Skeleton height="12rem" />
      </div>
    );
  }

  if (err) return <div className="p-4 text-danger">{String(err)}</div>;

  return (
    <div className="container-fluid p-4">
      <div className="mb-3">
        <h2 className="m-0">Messages</h2>
        <div className="text-muted">
          {summary?.programCode ?? "-"} • Semester {summary?.currentSemester ?? "-"}
        </div>
      </div>

      <h3 className="mt-3">Messages from Advisor</h3>
      <div className="text-muted mb-4">
        Important announcements and updates from your academic advisor
      </div>

      {/* Top stats */}
      <div className="row g-3 mb-4">
        <StatCard
          title="Total Messages"
          value={stats.totalMessages}
          icon="pi pi-envelope"
        />
        <StatCard
          title="Recent (7 days)"
          value={stats.recent7Days}
        />
        <StatCard
          title="This Month"
          value={stats.thisMonth}
        />
      </div>

      {/* All messages */}
      <Card className="shadow-sm border-0 mb-4">
        <div className="fw-semibold fs-5">All Messages</div>
        <div className="text-muted mb-4">
          Broadcast messages and announcements from your advisor
        </div>

        {messages.length === 0 ? (
          <div className="text-muted">No messages available.</div>
        ) : (
          <div className="d-flex flex-column gap-4">
            {messages.map((msg) => (
              <MessageCard key={msg.announcementId} message={msg} />
            ))}
          </div>
        )}
      </Card>

      {/* Need help */}
      <Card className="shadow-sm border-0">
        <div className="fw-semibold fs-5">Need Help?</div>
        <div className="text-muted mb-3">
          Contact your advisor for questions or concerns
        </div>

        <div className="mb-3">
          If you have questions about any announcement or need clarification, you can contact your advisor:
        </div>

        <div className="rounded-3 p-4" style={{ background: "#f3f4f6" }}>
          <div className="fw-semibold">{advisor?.name ?? "Not assigned"}</div>
          <div className="mt-2">Email: {advisor?.email ?? "-"}</div>
          <div className="mt-2">Office: {advisor?.office ?? "Building A, Room 305"}</div>
          <div className="mt-2">Office Hours: {advisor?.officeHours ?? "Mon, Wed, Fri: 2-4 PM"}</div>
        </div>
      </Card>
    </div>
  );
}

function StatCard({ title, value, icon }) {
  return (
    <div className="col-12 col-md-4">
      <div className="border rounded-3 p-4 bg-white h-100">
        <div className="fw-semibold d-flex align-items-center gap-2">
          {icon ? <i className={icon} /> : null}
          <span>{title}</span>
        </div>
        <div className="fs-1 fw-semibold mt-4">{value}</div>
      </div>
    </div>
  );
}

function MessageCard({ message }) {
  return (
    <div className="border rounded-4 p-4 bg-white">
      <div className="fw-semibold fs-4">{message.title}</div>

      <div className="text-muted mt-2 d-flex flex-wrap gap-3">
        <span className="d-flex align-items-center gap-2">
          <i className="pi pi-calendar" />
          {formatDate(message.createdAt)}
        </span>
        <span className="d-flex align-items-center gap-2">
          <i className="pi pi-envelope" />
          From: {message.advisorName}
        </span>
      </div>

      <div
        className="rounded-3 p-3 mt-4"
        style={{ background: "#f3f4f6", whiteSpace: "pre-wrap" }}
      >
        {message.content}
      </div>

      <hr className="my-4" />

      <div className="text-muted small">
        This message was sent to {message.recipientsCount} students in your advising group
      </div>
    </div>
  );
}

function formatDate(dateLike) {
  const d = new Date(dateLike);
  return d.toLocaleDateString(undefined, {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}